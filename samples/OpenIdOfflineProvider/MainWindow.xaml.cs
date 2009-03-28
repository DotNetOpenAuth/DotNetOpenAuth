//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable {
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
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary>
		public MainWindow() {
			this.InitializeComponent();
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
		}

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
		/// Raises the <see cref="E:Closing"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			this.StopProvider();
			base.OnClosing(e);
		}

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
		/// Handles the Click event of the startButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void startButton_Click(object sender, RoutedEventArgs e) {
			this.StartProvider();
		}

		private void StartProvider() {
			this.httpHost = HttpHost.CreateHost(this.RequestHandler);
			this.portLabel .Content = this.httpHost.Port.ToString(CultureInfo.InvariantCulture);
			string url = "http://localhost:{0}{1}";
			this.opIdentifierLabel.Content = "not yet supported"; // string.Format(url, this.httpHost.Port, OPIdentifier);
			this.noIdentity.Content = string.Format(url, this.httpHost.Port, NoIdentity);
			this.yesIdentity.Content = string.Format(url, this.httpHost.Port, YesIdentity);
		}

		private void RequestHandler(HttpListenerContext context) {
			Contract.Requires(context != null);
			Contract.Requires(context.Response.OutputStream != null);
			Stream outputStream = context.Response.OutputStream;
			Contract.Assume(outputStream != null); // CC static verification shortcoming.

			if (context.Request.Url.AbsolutePath == ProviderPath) {
				HttpRequestInfo requestInfo = new HttpRequestInfo(context.Request);
				IRequest providerRequest = this.provider.GetRequest(requestInfo);
				if (providerRequest == null) {
					App.Logger.Error("A request came in that did not carry an OpenID message.");
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					using (StreamWriter sw = new StreamWriter(outputStream)) {
						sw.WriteLine("<html><body>This is an OpenID Provider endpoint.</body></html>");
					}
					return;
				}

				if (!providerRequest.IsResponseReady) {
					var authRequest = providerRequest as IAuthenticationRequest;
					if (authRequest.IsDirectedIdentity) {
						throw new NotImplementedException();
					}

					authRequest.IsAuthenticated = new Uri(authRequest.ClaimedIdentifier).AbsolutePath == YesIdentity;
				}

				this.provider.PrepareResponse(providerRequest).Send(context.Response);
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

		private void stopButton_Click(object sender, RoutedEventArgs e) {
			this.StopProvider();
			this.portLabel.Content = string.Empty;
			this.noIdentity.Content = string.Empty;
			this.yesIdentity.Content = string.Empty;
			this.opIdentifierLabel.Content = string.Empty;
		}

		private void StopProvider() {
			if (this.httpHost != null) {
				this.httpHost.Dispose();
				this.httpHost = null;
			}
		}
	}
}
