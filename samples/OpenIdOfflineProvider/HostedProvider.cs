//-----------------------------------------------------------------------
// <copyright file="HostedProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.ServiceModel;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Http;
	using System.Web.Http.SelfHost;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using log4net;
	using Validation;

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
		/// The HTTP listener that acts as the OpenID Provider socket.
		/// </summary>
		private HttpSelfHostServer hostServer;

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
		/// </value>
		internal bool IsRunning {
			get { return this.hostServer != null; }
		}

		/// <summary>
		/// Gets the provider endpoint.
		/// </summary>
		internal Uri ProviderEndpoint { get; private set; }

		/// <summary>
		/// Gets the base URI that all user identities must start with.
		/// </summary>
		internal Uri UserIdentityPageBase {
			get {
				Assumes.True(this.IsRunning);
				return new Uri(this.ProviderEndpoint, UserIdentifierPath);
			}
		}

		/// <summary>
		/// Gets the OP identifier.
		/// </summary>
		internal Uri OPIdentifier {
			get {
				Assumes.True(this.IsRunning);
				return new Uri(this.ProviderEndpoint, OPIdentifierPath);
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
		internal async Task StartProviderAsync(HttpMessageHandler providerEndpointHandler) {
			Requires.NotNull(providerEndpointHandler, "providerEndpointHandler");
			Verify.Operation(this.hostServer == null, "Server already started.");

			int port = 45235;
			var baseUri = new UriBuilder("http", "localhost", port);
			var configuration = new HttpSelfHostConfiguration(baseUri.Uri);
			try {
				var hostServer = new HttpSelfHostServer(configuration, new Handler(this, providerEndpointHandler));
				await hostServer.OpenAsync();
				this.hostServer = hostServer;
			} catch (AddressAccessDeniedException ex) {
				// If this throws an exception, use an elevated command prompt and execute:
				// netsh http add urlacl url=http://+:45235/ user=YOUR_USERNAME_HERE
				string message = string.Format(
					CultureInfo.CurrentCulture,
					"Use an elevated command prompt and execute: \nnetsh http add urlacl url=http://+:{0}/ user={1}\\{2}",
					port,
					Environment.UserDomainName,
					Environment.UserName);
				throw new InvalidOperationException(message, ex);
			}

			this.ProviderEndpoint = new Uri(baseUri.Uri, ProviderPath);
		}

		/// <summary>
		/// Stops the provider.
		/// </summary>
		internal async Task StopProviderAsync() {
			if (this.hostServer != null) {
				await this.hostServer.CloseAsync();
				this.hostServer.Dispose();
				this.hostServer = null;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.hostServer.Dispose();
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
			Requires.NotNull(providerEndpoint, "providerEndpoint");

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
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNull(supportedExtensions, "supportedExtensions");

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

		private class Handler : DelegatingHandler {
			private HostedProvider host;

			internal Handler(HostedProvider host, HttpMessageHandler innerHandler)
				: base(innerHandler) {
				Requires.NotNull(host, "host");

				this.host = host;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) {
				Uri providerEndpoint = new Uri(request.RequestUri, ProviderPath);
				if (request.RequestUri.AbsolutePath == ProviderPath) {
					return await base.SendAsync(request, cancellationToken);
				} else if (request.RequestUri.AbsolutePath.StartsWith(UserIdentifierPath, StringComparison.Ordinal)) {
					string localId = null; // string.Format("http://localhost:{0}/user", context.Request.Url.Port);
					return new HttpResponseMessage() {
						RequestMessage = request,
						Content = new StringContent(GenerateHtmlDiscoveryDocument(providerEndpoint, localId), Encoding.UTF8, "text/html"),
					};
				} else if (request.RequestUri == this.host.OPIdentifier) {
					App.Logger.Info("Discovery on OP Identifier detected.");
					return new HttpResponseMessage() {
						Content = new StringContent(GenerateXrdsOPIdentifierDocument(providerEndpoint, Enumerable.Empty<string>()), Encoding.UTF8, "application/xrds+xml"),
					};
				} else {
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}
			}
		}
	}
}
