//-----------------------------------------------------------------------
// <copyright file="ClientAuthorizationView.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A WinForms control that hosts a mini-browser for hosting by native applications to
	/// allow the user to authorize the client without leaving the application.
	/// </summary>
	public partial class ClientAuthorizationView : UserControl {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAuthorizationView"/> class.
		/// </summary>
		public ClientAuthorizationView() {
			this.InitializeComponent();

			this.Authorization = new AuthorizationState();
		}

		/// <summary>
		/// Occurs when the authorization flow has completed.
		/// </summary>
		public event EventHandler<ClientAuthorizationCompleteEventArgs> Completed;

		/// <summary>
		/// Gets the authorization tracking object.
		/// </summary>
		public IAuthorizationState Authorization { get; private set; }

		/// <summary>
		/// Gets or sets the client used to coordinate the authorization flow.
		/// </summary>
		public UserAgentClient Client { get; set; }

		/// <summary>
		/// Gets the set of scopes that describe the requested level of access.
		/// </summary>
		public HashSet<string> Scope {
			get { return this.Authorization.Scope; }
		}

		/// <summary>
		/// Gets or sets the callback URL used to indicate the flow has completed.
		/// </summary>
		public Uri Callback {
			get { return this.Authorization.Callback; }
			set { this.Authorization.Callback = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the authorization flow has been completed.
		/// </summary>
		public bool IsCompleted {
			get { return this.Authorization == null || this.Authorization.AccessToken != null; }
		}

		/// <summary>
		/// Gets a value indicating whether authorization has been granted.
		/// </summary>
		/// <value>Null if <see cref="IsCompleted"/> is <c>false</c></value>
		public bool? IsGranted {
			get {
				if (this.Authorization == null) {
					return false;
				}

				return this.Authorization.AccessToken != null ? (bool?)true : null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether authorization has been rejected.
		/// </summary>
		/// <value>Null if <see cref="IsCompleted"/> is <c>false</c></value>
		public bool? IsRejected {
			get {
				bool? granted = this.IsGranted;
				return granted.HasValue ? (bool?)(!granted.Value) : null;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the implicit grant type should be used instead of the authorization code grant.
		/// </summary>
		/// <value>
		/// <c>true</c> if [request implicit grant]; otherwise, <c>false</c>.
		/// </value>
		public bool RequestImplicitGrant { get; set; }

		/// <summary>
		/// Called when the authorization flow has been completed.
		/// </summary>
		protected virtual void OnCompleted() {
			var completed = this.Completed;
			if (completed != null) {
				completed(this, new ClientAuthorizationCompleteEventArgs(this.Authorization));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Avoid bug in .NET WebBrowser control.")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "It's a new instance we control.")]
		protected override async void OnLoad(EventArgs e) {
			base.OnLoad(e);

			Uri authorizationUrl = await this.Client.RequestUserAuthorizationAsync(this.Authorization, implicitResponseType: this.RequestImplicitGrant);
			this.webBrowser1.Navigate(authorizationUrl.AbsoluteUri); // use AbsoluteUri to workaround bug in WebBrowser that calls Uri.ToString instead of Uri.AbsoluteUri leading to escaping errors.
		}

		/// <summary>
		/// Tests whether two URLs are equal for purposes of detecting the conclusion of authorization.
		/// </summary>
		/// <param name="location1">The first location.</param>
		/// <param name="location2">The second location.</param>
		/// <param name="components">The components to compare.</param>
		/// <returns><c>true</c> if the given components are equal.</returns>
		private static bool SignificantlyEqual(Uri location1, Uri location2, UriComponents components) {
			string value1 = location1.GetComponents(components, UriFormat.Unescaped);
			string value2 = location2.GetComponents(components, UriFormat.Unescaped);
			return string.Equals(value1, value2, StringComparison.Ordinal);
		}

		/// <summary>
		/// Handles the Navigating event of the webBrowser1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.WebBrowserNavigatingEventArgs"/> instance containing the event data.</param>
		private async void WebBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
			await this.ProcessLocationChangedAsync(e.Url);
		}

		/// <summary>
		/// Processes changes in the URL the browser has navigated to.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		private async Task ProcessLocationChangedAsync(Uri location) {
			if (SignificantlyEqual(location, this.Authorization.Callback, UriComponents.SchemeAndServer | UriComponents.Path)) {
				try {
					await this.Client.ProcessUserAuthorizationAsync(location, this.Authorization);
				} catch (ProtocolException ex) {
					var options = (MessageBoxOptions)0;
					if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes) {
						options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
					}
					MessageBox.Show(this, ex.ToStringDescriptive(), ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
				} finally {
					this.OnCompleted();
				}
			}
		}

		/// <summary>
		/// Handles the Navigated event of the webBrowser1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.WebBrowserNavigatedEventArgs"/> instance containing the event data.</param>
		private async void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
			await this.ProcessLocationChangedAsync(e.Url);
		}

		/// <summary>
		/// Handles the LocationChanged event of the webBrowser1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private async void WebBrowser1_LocationChanged(object sender, EventArgs e) {
			await this.ProcessLocationChangedAsync(this.webBrowser1.Url);
		}
	}
}
