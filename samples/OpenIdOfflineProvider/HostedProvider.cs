//-----------------------------------------------------------------------
// <copyright file="HostedProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Web;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using log4net;

	/// <summary>
	/// The OpenID Provider host.
	/// </summary>
	internal class HostedProvider : IDisposable {
		/// <summary>
		/// The path to the Provider Endpoint.
		/// </summary>
		private const string ProviderPath = "/provider";

		/// <summary>
		/// The path to the OP Identifier.
		/// </summary>
		private const string OPIdentifierPath = "/";

		/// <summary>
		/// The URL path with which all user identities must start.
		/// </summary>
		private const string UserIdentifierPath = "/user/";

		/// <summary>
		/// The <see cref="OpenIdProvider"/> instance that processes incoming requests.
		/// </summary>
		private OpenIdProvider provider = new OpenIdProvider(new StandardProviderApplicationStore());

		/// <summary>
		/// The HTTP listener that acts as the OpenID Provider socket.
		/// </summary>
		private HttpHost httpHost;

		/// <summary>
		/// Initializes a new instance of the <see cref="HostedProvider"/> class.
		/// </summary>
		internal HostedProvider() {
		}

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
		/// </value>
		internal bool IsRunning {
			get { return this.httpHost != null; }
		}

		/// <summary>
		/// Gets the <see cref="OpenIdProvider"/> instance that processes incoming requests.
		/// </summary>
		internal OpenIdProvider Provider {
			get { return this.provider; }
		}

		/// <summary>
		/// Gets or sets the delegate that handles authentication requests.
		/// </summary>
		internal Action<HttpRequestBase, HttpListenerResponse> ProcessRequest { get; set; }

		/// <summary>
		/// Gets the provider endpoint.
		/// </summary>
		internal Uri ProviderEndpoint {
			get {
				Contract.Requires(this.IsRunning);
				return new Uri(this.httpHost.BaseUri, ProviderPath);
			}
		}

		/// <summary>
		/// Gets the base URI that all user identities must start with.
		/// </summary>
		internal Uri UserIdentityPageBase {
			get {
				Contract.Requires(this.IsRunning);
				return new Uri(this.httpHost.BaseUri, UserIdentifierPath);
			}
		}

		/// <summary>
		/// Gets the OP identifier.
		/// </summary>
		internal Uri OPIdentifier {
			get {
				Contract.Requires(this.IsRunning);
				return new Uri(this.httpHost.BaseUri, OPIdentifierPath);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
		}

		/// <summary>
		/// Starts the provider.
		/// </summary>
		internal void StartProvider() {
			Contract.Ensures(this.IsRunning);
			this.httpHost = HttpHost.CreateHost(this.RequestHandler);
		}

		/// <summary>
		/// Stops the provider.
		/// </summary>
		internal void StopProvider() {
			Contract.Ensures(!this.IsRunning);
			if (this.httpHost != null) {
				this.httpHost.Dispose();
				this.httpHost = null;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				var host = this.httpHost as IDisposable;
				if (host != null) {
					host.Dispose();
				}

				this.httpHost = null;
			}
		}

		#endregion

		/// <summary>
		/// Generates HTML for an identity page.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="localId">The local id.</param>
		/// <returns>The HTML document to return to the RP.</returns>
		private static string GenerateHtmlDiscoveryDocument(Uri providerEndpoint, string localId) {
			Contract.Requires(providerEndpoint != null);

			const string DelegatedHtmlDiscoveryFormat = @"<html><head>
				<link rel=""openid.server"" href=""{0}"" />
				<link rel=""openid.delegate"" href=""{1}"" />
				<link rel=""openid2.provider"" href=""{0}"" />
				<link rel=""openid2.local_id"" href=""{1}"" />
			</head><body></body></html>";

			const string NonDelegatedHtmlDiscoveryFormat = @"<html><head>
				<link rel=""openid.server"" href=""{0}"" />
				<link rel=""openid2.provider"" href=""{0}"" />
			</head><body></body></html>";

			return string.Format(
				localId != null ? DelegatedHtmlDiscoveryFormat : NonDelegatedHtmlDiscoveryFormat,
				providerEndpoint.AbsoluteUri,
				localId);
		}

		/// <summary>
		/// Generates the OP Identifier XRDS document.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="supportedExtensions">The supported extensions.</param>
		/// <returns>The content of the XRDS document.</returns>
		private static string GenerateXrdsOPIdentifierDocument(Uri providerEndpoint, IEnumerable<string> supportedExtensions) {
			Contract.Requires(providerEndpoint != null);
			Contract.Requires(supportedExtensions != null);

			const string OPIdentifierDiscoveryFormat = @"<xrds:XRDS
	xmlns:xrds='xri://$xrds'
	xmlns:openid='http://openid.net/xmlns/1.0'
	xmlns='xri://$xrd*($v*2.0)'>
	<XRD>
		<Service priority='10'>
			<Type>http://specs.openid.net/auth/2.0/server</Type>
			{1}
			<URI>{0}</URI>
		</Service>
	</XRD>
</xrds:XRDS>";

			string extensions = string.Join(
				"\n\t\t\t",
				supportedExtensions.Select(ext => "<Type>" + ext + "</Type>").ToArray());
			return string.Format(
				OPIdentifierDiscoveryFormat,
				providerEndpoint.AbsoluteUri,
				extensions);
		}

		/// <summary>
		/// Handles incoming HTTP requests.
		/// </summary>
		/// <param name="context">The HttpListener context.</param>
		private void RequestHandler(HttpListenerContext context) {
			Contract.Requires(context != null);
			Contract.Requires(context.Response.OutputStream != null);
			Contract.Requires(this.ProcessRequest != null);
			Stream outputStream = context.Response.OutputStream;
			Contract.Assume(outputStream != null); // CC static verification shortcoming.

			UriBuilder providerEndpointBuilder = new UriBuilder();
			providerEndpointBuilder.Scheme = Uri.UriSchemeHttp;
			providerEndpointBuilder.Host = "localhost";
			providerEndpointBuilder.Port = context.Request.Url.Port;
			providerEndpointBuilder.Path = ProviderPath;
			Uri providerEndpoint = providerEndpointBuilder.Uri;

			if (context.Request.Url.AbsolutePath == ProviderPath) {
				HttpRequestBase requestInfo = HttpRequestInfo.Create(context.Request);
				this.ProcessRequest(requestInfo, context.Response);
			} else if (context.Request.Url.AbsolutePath.StartsWith(UserIdentifierPath, StringComparison.Ordinal)) {
				using (StreamWriter sw = new StreamWriter(outputStream)) {
					context.Response.StatusCode = (int)HttpStatusCode.OK;

					string localId = null; // string.Format("http://localhost:{0}/user", context.Request.Url.Port);
					string html = GenerateHtmlDiscoveryDocument(providerEndpoint, localId);
					sw.WriteLine(html);
				}
				context.Response.OutputStream.Close();
			} else if (context.Request.Url == this.OPIdentifier) {
				context.Response.ContentType = "application/xrds+xml";
				context.Response.StatusCode = (int)HttpStatusCode.OK;
				App.Logger.Info("Discovery on OP Identifier detected.");
				using (StreamWriter sw = new StreamWriter(outputStream)) {
					sw.Write(GenerateXrdsOPIdentifierDocument(providerEndpoint, Enumerable.Empty<string>()));
				}
				context.Response.OutputStream.Close();
			} else {
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.OutputStream.Close();
			}
		}
	}
}
