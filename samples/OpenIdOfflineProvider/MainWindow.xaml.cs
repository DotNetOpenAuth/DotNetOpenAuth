//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
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
	using DotNetOpenAuth.OpenId.Provider;
	using System.Globalization;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using System.Diagnostics.Contracts;
	using System.IO;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable {
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

		/// <summary>
		/// Raises the <see cref="E:Closing"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			this.StopProvider();
			base.OnClosing(e);
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
		/// Handles the Click event of the startButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void startButton_Click(object sender, RoutedEventArgs e) {
			this.StartProvider();
		}

		private void StartProvider() {
			this.httpHost = HttpHost.CreateHost(this.RequestHandler);
			this.portTextBox.Text = this.httpHost.Port.ToString(CultureInfo.InvariantCulture);
			this.opIdentifierLabel.Content = string.Format("http://localhost:{0}/", this.httpHost.Port);
		}

		private void RequestHandler(HttpListenerContext context) {
			Contract.Requires(context != null);

			const string ProviderPath = "/provider";
			const string YesIdentity = "/user";
			const string NoIdentity = "/no";

			if (context.Request.Url.AbsolutePath == ProviderPath) {
				HttpRequestInfo requestInfo = new HttpRequestInfo(context.Request);
				IRequest providerRequest = this.provider.GetRequest(requestInfo);
				if (providerRequest == null) {
					App.Logger.Error("A request came in that did not carry an OpenID message.");
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					using (StreamWriter sw = new StreamWriter(context.Response.OutputStream)) {
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

				Contract.Assert(providerRequest.IsResponseReady);
				this.provider.PrepareResponse(providerRequest).Send(context.Response);
			} else if (context.Request.Url.AbsolutePath == YesIdentity || context.Request.Url.AbsolutePath == NoIdentity) {
				using (StreamWriter sw = new StreamWriter(context.Response.OutputStream)) {
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

		private string GenerateHtmlDiscoveryDocument(string providerEndpoint, string localId) {
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

		private void stopButton_Click(object sender, RoutedEventArgs e) {
			this.StopProvider();
			this.portTextBox.Text = string.Empty;
		}

		private void StopProvider() {
			if (this.httpHost != null) {
				this.httpHost.Dispose();
				this.httpHost = null;
			}
		}
	}
}
