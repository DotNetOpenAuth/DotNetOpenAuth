//-----------------------------------------------------------------------
// <copyright file="HostedProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

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
		private const string OPIdentifier = "/";

		/// <summary>
		/// The path to the user identity page that always generates a positive assertion.
		/// </summary>
		private const string YesIdentity = "/user";

		/// <summary>
		/// The path to the user identity page that always generates a negative response.
		/// </summary>
		private const string NoIdentity = "/no";

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
			this.AffirmativeIdentities = new HashSet<Uri>();
			this.NegativeIdentities = new HashSet<Uri>();
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
		/// Gets a collection of identity URLs that always produce positive assertions.
		/// </summary>
		internal ICollection<Uri> AffirmativeIdentities { get; private set; }

		/// <summary>
		/// Gets a collection of identity URLs that always produce cancellation responses.
		/// </summary>
		internal ICollection<Uri> NegativeIdentities { get; private set; }

		/// <summary>
		/// Gets the <see cref="OpenIdProvider"/> instance that processes incoming requests.
		/// </summary>
		internal OpenIdProvider Provider {
			get { return this.provider; }
		}

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
		/// Gets or sets the delegate that handles authentication requests.
		/// </summary>
		internal Action<IAuthenticationRequest> ProcessAuthenticationRequest { get; set; }

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
			this.AffirmativeIdentities.Add(new Uri(this.httpHost.BaseUri, YesIdentity));
			this.NegativeIdentities.Add(new Uri(this.httpHost.BaseUri, NoIdentity));
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
		private static string GenerateHtmlDiscoveryDocument(string providerEndpoint, string localId) {
			Contract.Requires(providerEndpoint != null && providerEndpoint.Length > 0);

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
				providerEndpoint,
				localId);
		}

		/// <summary>
		/// Handles incoming HTTP requests.
		/// </summary>
		/// <param name="context">The HttpListener context.</param>
		private void RequestHandler(HttpListenerContext context) {
			Contract.Requires(context != null);
			Contract.Requires(context.Response.OutputStream != null);
			Contract.Requires(this.ProcessAuthenticationRequest != null);
			Stream outputStream = context.Response.OutputStream;
			Contract.Assume(outputStream != null); // CC static verification shortcoming.

			if (context.Request.Url.AbsolutePath == ProviderPath) {
				HttpRequestInfo requestInfo = new HttpRequestInfo(context.Request);
				IRequest providerRequest = this.Provider.GetRequest(requestInfo);
				if (providerRequest == null) {
					App.Logger.Error("A request came in that did not carry an OpenID message.");
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					using (StreamWriter sw = new StreamWriter(outputStream)) {
						sw.WriteLine("<html><body>This is an OpenID Provider endpoint.</body></html>");
					}
					return;
				}

				if (!providerRequest.IsResponseReady) {
					var authRequest = (IAuthenticationRequest)providerRequest;
					this.ProcessAuthenticationRequest(authRequest);
				}

				this.Provider.PrepareResponse(providerRequest).Send(context.Response);
			} else if (context.Request.Url.AbsolutePath == YesIdentity || context.Request.Url.AbsolutePath == NoIdentity) {
				using (StreamWriter sw = new StreamWriter(outputStream)) {
					string providerEndpoint = string.Format("http://localhost:{0}{1}", context.Request.Url.Port, ProviderPath);
					string localId = null; // string.Format("http://localhost:{0}/user", context.Request.Url.Port);
					string html = GenerateHtmlDiscoveryDocument(providerEndpoint, localId);
					sw.WriteLine(html);
				}

				context.Response.StatusCode = (int)HttpStatusCode.OK;
				context.Response.OutputStream.Close();
			} else {
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.OutputStream.Close();
			}
		}
	}
}
