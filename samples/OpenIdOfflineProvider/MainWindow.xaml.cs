//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http.Headers;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
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
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using log4net;
	using log4net.Appender;
	using log4net.Core;
	using Validation;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable {
		/// <summary>
		/// The OpenID Provider host object.
		/// </summary>
		private HostedProvider hostedProvider = new HostedProvider();

		/// <summary>
		/// The logger the application may use.
		/// </summary>
		private ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary>
		public MainWindow() {
			this.InitializeComponent();
			this.hostedProvider.ProcessRequestAsync = this.ProcessRequestAsync;
			TextWriterAppender boxLogger = log4net.LogManager.GetRepository().GetAppenders().OfType<TextWriterAppender>().FirstOrDefault(a => a.Name == "TextBoxAppender");
			if (boxLogger != null) {
				boxLogger.Writer = new TextBoxTextWriter(this.logBox);
			}

			this.startProvider();
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
				var host = this.hostedProvider as IDisposable;
				if (host != null) {
					host.Dispose();
				}

				this.hostedProvider = null;
			}
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="E:Closing"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			this.stopProvider();
			base.OnClosing(e);
		}

		/// <summary>
		/// Adds a set of HTTP headers to an <see cref="HttpResponse"/> instance,
		/// taking care to set some headers to the appropriate properties of
		/// <see cref="HttpResponse" />
		/// </summary>
		/// <param name="headers">The headers to add.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> instance to set the appropriate values to.</param>
		private static void ApplyHeadersToResponse(HttpResponseHeaders headers, HttpListenerResponse response) {
			Requires.NotNull(headers, "headers");
			Requires.NotNull(response, "response");

			foreach (var header in headers) {
				switch (header.Key) {
					case "Content-Type":
						response.ContentType = header.Value.First();
						break;

					// Add more special cases here as necessary.
					default:
						response.AddHeader(header.Key, header.Value.First());
						break;
				}
			}
		}

		/// <summary>
		/// Processes an incoming request at the OpenID Provider endpoint.
		/// </summary>
		/// <param name="requestInfo">The request info.</param>
		/// <param name="response">The response.</param>
		private async Task ProcessRequestAsync(HttpRequestBase requestInfo, HttpListenerResponse response) {
			IRequest request = await this.hostedProvider.Provider.GetRequestAsync(requestInfo, CancellationToken.None);
			if (request == null) {
				App.Logger.Error("A request came in that did not carry an OpenID message.");
				response.ContentType = "text/html";
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				using (StreamWriter sw = new StreamWriter(response.OutputStream)) {
					sw.WriteLine("<html><body>This is an OpenID Provider endpoint.</body></html>");
				}
				return;
			}

			this.Dispatcher.Invoke(async delegate {
				if (!request.IsResponseReady) {
					var authRequest = request as IAuthenticationRequest;
					if (authRequest != null) {
						switch (this.checkidRequestList.SelectedIndex) {
							case 0:
								if (authRequest.IsDirectedIdentity) {
									string userIdentityPageBase = this.hostedProvider.UserIdentityPageBase.AbsoluteUri;
									if (this.capitalizedHostName.IsChecked.Value) {
										userIdentityPageBase = (this.hostedProvider.UserIdentityPageBase.Scheme + Uri.SchemeDelimiter + this.hostedProvider.UserIdentityPageBase.Authority).ToUpperInvariant() + this.hostedProvider.UserIdentityPageBase.PathAndQuery;
									}
									string leafPath = "directedidentity";
									if (this.directedIdentityTrailingPeriodsCheckbox.IsChecked.Value) {
										leafPath += ".";
									}
									authRequest.ClaimedIdentifier = Identifier.Parse(userIdentityPageBase + leafPath, true);
									authRequest.LocalIdentifier = authRequest.ClaimedIdentifier;
								}
								authRequest.IsAuthenticated = true;
								break;
							case 1:
								authRequest.IsAuthenticated = false;
								break;
							case 2:
								IntPtr oldForegroundWindow = NativeMethods.GetForegroundWindow();
								bool stoleFocus = NativeMethods.SetForegroundWindow(this);
								await CheckIdWindow.ProcessAuthenticationAsync(this.hostedProvider, authRequest);
								if (stoleFocus) {
									NativeMethods.SetForegroundWindow(oldForegroundWindow);
								}
								break;
						}
					}
				}
			});

			var responseMessage = await this.hostedProvider.Provider.PrepareResponseAsync(request, CancellationToken.None);
			ApplyHeadersToResponse(responseMessage.Headers, response);
		}

		/// <summary>
		/// Starts the provider.
		/// </summary>
		private void startProvider() {
			this.hostedProvider.StartProvider();
			this.opIdentifierLabel.Content = this.hostedProvider.OPIdentifier;
		}

		/// <summary>
		/// Stops the provider.
		/// </summary>
		private void stopProvider() {
			this.hostedProvider.StopProvider();
			this.opIdentifierLabel.Content = string.Empty;
		}

		/// <summary>
		/// Handles the MouseDown event of the opIdentifierLabel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void opIdentifierLabel_MouseDown(object sender, MouseButtonEventArgs e) {
			try {
				Clipboard.SetText(this.opIdentifierLabel.Content.ToString());
			} catch (COMException ex) {
				MessageBox.Show(this, ex.Message, "Error while copying OP Identifier to the clipboard", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Handles the Click event of the ClearLogButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void ClearLogButton_Click(object sender, RoutedEventArgs e) {
			this.logBox.Clear();
		}
	}
}
