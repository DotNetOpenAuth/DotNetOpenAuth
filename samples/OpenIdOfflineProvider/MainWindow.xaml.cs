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
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Runtime.InteropServices;
	using System.ServiceModel;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Http;
	using System.Web.Http.Routing;
	using System.Web.Http.SelfHost;
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
		/// The main window for the app.
		/// </summary>
		internal static MainWindow Instance;

		/// <summary>
		/// The logger the application may use.
		/// </summary>
		private ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));

		/// <summary>
		/// The HTTP listener that acts as the OpenID Provider socket.
		/// </summary>
		private HttpSelfHostServer hostServer;

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary>
		public MainWindow() {
			this.InitializeComponent();
			TextWriterAppender boxLogger = log4net.LogManager.GetRepository().GetAppenders().OfType<TextWriterAppender>().FirstOrDefault(a => a.Name == "TextBoxAppender");
			if (boxLogger != null) {
				boxLogger.Writer = new TextBoxTextWriter(this.logBox);
			}

			Instance = this;
			this.StartProviderAsync();
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
				if (this.hostServer != null) {
					this.hostServer.Dispose();
				}

				this.hostServer = null;
			}
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="E:Closing"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			this.StopProviderAsync();
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

		/// <summary>
		/// Starts the provider.
		/// </summary>
		/// <returns>A task that completes when the asynchronous operation is finished.</returns>
		private async Task StartProviderAsync() {
			Exception exception = null;
			try {
				Verify.Operation(this.hostServer == null, "Server already started.");

				int port = 45235;
				var baseUri = new UriBuilder("http", "localhost", port);
				var configuration = new HttpSelfHostConfiguration(baseUri.Uri);
				configuration.Routes.MapHttpRoute("default", "{controller}/{id}", new { controller = "Home", id = RouteParameter.Optional });
				try {
					var hostServer = new HttpSelfHostServer(configuration);
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

				this.opIdentifierLabel.Content = baseUri.Uri.AbsoluteUri;
			} catch (InvalidOperationException ex) {
				exception = ex;
			}

			if (exception != null) {
				if (MessageBox.Show(exception.Message, "Configuration error", MessageBoxButton.OKCancel, MessageBoxImage.Error)
					== MessageBoxResult.OK) {
					await this.StartProviderAsync();
					return;
				} else {
					this.Close();
				}
			}
		}

		/// <summary>
		/// Stops the provider.
		/// </summary>
		/// <returns>A task that completes when the asynchronous operation is finished.</returns>
		private async Task StopProviderAsync() {
			if (this.hostServer != null) {
				await this.hostServer.CloseAsync();
				this.hostServer.Dispose();
				this.hostServer = null;
			}

			this.opIdentifierLabel.Content = string.Empty;
		}
	}
}
