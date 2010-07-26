namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
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
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	/// <summary>
	/// Interaction logic for Authorize2.xaml
	/// </summary>
	public partial class Authorize2 : Window {
		private UserAgentClient client;

		internal Authorize2(UserAgentClient client, IAuthorizationState authorizationState) {
			Contract.Requires(client != null, "client");
			Contract.Requires(authorizationState != null, "authorizationState");

			InitializeComponent();

			this.client = client;
			this.Authorization = authorizationState;
			Uri authorizationUrl = this.client.RequestUserAuthorization(this.Authorization);
			this.webBrowser.Navigate(authorizationUrl.AbsoluteUri); // use AbsoluteUri to workaround bug in WebBrowser that calls Uri.ToString instead of Uri.AbsoluteUri leading to escaping errors.
		}

		public IAuthorizationState Authorization { get; set; }

		private static bool SignificantlyEqual(Uri location1, Uri location2, UriComponents components) {
			string value1 = location1.GetComponents(components, UriFormat.Unescaped);
			string value2 = location2.GetComponents(components, UriFormat.Unescaped);
			return string.Equals(value1, value2, StringComparison.Ordinal);
		}

		private void webBrowser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) {
			this.locationChanged(e.Url);
		}

		private void locationChanged(Uri location) {
			//if (location.Scheme == "res") {
			//    this.DialogResult = false;
			//    this.Close();
			//    MessageBox.Show("An error occurred during authorization.");
			//}

			if (SignificantlyEqual(location, this.Authorization.Callback, UriComponents.SchemeAndServer | UriComponents.Path)) {
				try {
					this.client.ProcessUserAuthorization(location, this.Authorization);
				} catch (ProtocolException ex) {
					MessageBox.Show(ex.ToStringDescriptive());
				} finally {
					this.DialogResult = !string.IsNullOrEmpty(this.Authorization.AccessToken);
					this.Close();
				}
			}
		}

		private void webBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e) {
			this.locationChanged(e.Url);
		}

		private void webBrowser_LocationChanged(object sender, EventArgs e) {
			this.locationChanged(webBrowser.Url);
		}
	}
}