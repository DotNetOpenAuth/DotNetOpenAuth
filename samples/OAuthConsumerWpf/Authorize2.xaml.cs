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
	using DotNetOpenAuth.OAuthWrap;

	/// <summary>
	/// Interaction logic for Authorize2.xaml
	/// </summary>
	public partial class Authorize2 : Window {
		private UserAgentClient client;

		internal Authorize2(UserAgentClient client) {
			Contract.Requires(client != null, "client");

			InitializeComponent();

			this.client = client;
			this.Authorization = new AuthorizationState();
			this.webBrowser.Navigate(this.client.RequestUserAuthorization(this.Authorization));
		}

		public IAuthorizationState Authorization { get; set; }

		private void webBrowser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) {
			this.locationChanged(e.Url);
		}

		private void locationChanged(Uri location) {
			if (location == this.Authorization.Callback) {
				this.client.ProcessUserAuthorization(location, this.Authorization);
				this.DialogResult = !string.IsNullOrEmpty(this.Authorization.AccessToken);
				this.Close();
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
