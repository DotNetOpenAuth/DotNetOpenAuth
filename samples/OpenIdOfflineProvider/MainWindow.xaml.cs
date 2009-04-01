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

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable {
		/// <summary>
		/// The OpenID Provider host object.
		/// </summary>
		private HostedProvider hostedProvider = new HostedProvider();

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
			this.hostedProvider.StopProvider();
			base.OnClosing(e);
		}

		/// <summary>
		/// Handles the Click event of the startButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void startButton_Click(object sender, RoutedEventArgs e) {
			this.hostedProvider.StartProvider();
			this.portLabel.Content = this.hostedProvider.ProviderEndpoint.Port;
			this.opIdentifierLabel.Content = "not yet supported"; // string.Format(url, this.httpHost.Port, OPIdentifier);
			this.noIdentity.Content = this.hostedProvider.NegativeIdentitities.First().AbsoluteUri;
			this.yesIdentity.Content = this.hostedProvider.AffirmativeIdentities.First().AbsoluteUri;
		}

		/// <summary>
		/// Handles the Click event of the stopButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void stopButton_Click(object sender, RoutedEventArgs e) {
			this.hostedProvider.StopProvider();
			this.portLabel.Content = string.Empty;
			this.noIdentity.Content = string.Empty;
			this.yesIdentity.Content = string.Empty;
			this.opIdentifierLabel.Content = string.Empty;
		}
	}
}
