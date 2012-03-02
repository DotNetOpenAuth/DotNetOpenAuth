//-----------------------------------------------------------------------
// <copyright file="CheckIdWindow.xaml.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Shapes;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Interaction logic for CheckIdWindow.xaml
	/// </summary>
	public partial class CheckIdWindow : Window {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckIdWindow"/> class.
		/// </summary>
		/// <param name="provider">The OpenID Provider host.</param>
		/// <param name="request">The incoming authentication request.</param>
		private CheckIdWindow(HostedProvider provider, IAuthenticationRequest request) {
			Contract.Requires(request != null);

			this.InitializeComponent();

			// Initialize the window with appropriate values.
			this.realmLabel.Content = request.Realm;
			this.immediateModeLabel.Visibility = request.Immediate ? Visibility.Visible : Visibility.Collapsed;
			this.setupModeLabel.Visibility = request.Immediate ? Visibility.Collapsed : Visibility.Visible;

			bool isRPDiscoverable = request.IsReturnUrlDiscoverable(provider.Provider.Channel.WebRequestHandler) == RelyingPartyDiscoveryResult.Success;
			this.discoverableYesLabel.Visibility = isRPDiscoverable ? Visibility.Visible : Visibility.Collapsed;
			this.discoverableNoLabel.Visibility = isRPDiscoverable ? Visibility.Collapsed : Visibility.Visible;

			if (request.IsDirectedIdentity) {
				this.claimedIdentifierBox.Text = provider.UserIdentityPageBase.AbsoluteUri;
				this.localIdentifierBox.Text = provider.UserIdentityPageBase.AbsoluteUri;
			} else {
				this.claimedIdentifierBox.Text = request.ClaimedIdentifier;
				this.localIdentifierBox.Text = request.LocalIdentifier;
			}
		}

		/// <summary>
		/// Processes an authentication request by a popup window.
		/// </summary>
		/// <param name="provider">The OpenID Provider host.</param>
		/// <param name="request">The incoming authentication request.</param>
		internal static void ProcessAuthentication(HostedProvider provider, IAuthenticationRequest request) {
			Contract.Requires(provider != null);
			Contract.Requires(request != null);

			var window = new CheckIdWindow(provider, request);
			bool? result = window.ShowDialog();

			// If the user pressed Esc or cancel, just send a negative assertion.
			if (!result.HasValue || !result.Value) {
				request.IsAuthenticated = false;
				return;
			}

			request.IsAuthenticated = window.tabControl1.SelectedItem == window.positiveTab;
			if (request.IsAuthenticated.Value) {
				request.ClaimedIdentifier = window.claimedIdentifierBox.Text;
				request.LocalIdentifier = window.localIdentifierBox.Text;
			}
		}

		/// <summary>
		/// Handles the Click event of the sendResponseButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void sendResponseButton_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			Close();
		}
	}
}
