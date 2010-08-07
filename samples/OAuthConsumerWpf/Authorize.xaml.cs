namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Shapes;
	using System.Xml.Linq;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// Interaction logic for Authorize.xaml
	/// </summary>
	public partial class Authorize : Window {
		private DesktopConsumer consumer;
		private string requestToken;

		internal Authorize(DesktopConsumer consumer, FetchUri fetchUriCallback) {
			this.InitializeComponent();

			this.consumer = consumer;
			Cursor original = this.Cursor;
			this.Cursor = Cursors.Wait;
			ThreadPool.QueueUserWorkItem(delegate(object state) {
				Uri browserAuthorizationLocation = fetchUriCallback(this.consumer, out this.requestToken);
				System.Diagnostics.Process.Start(browserAuthorizationLocation.AbsoluteUri);
				this.Dispatcher.BeginInvoke(new Action(() => {
					this.Cursor = original;
					finishButton.IsEnabled = true;
				}));
			});
		}

		internal delegate Uri FetchUri(DesktopConsumer consumer, out string requestToken);

		internal string AccessToken { get; set; }

		private void finishButton_Click(object sender, RoutedEventArgs e) {
			var grantedAccess = this.consumer.ProcessUserAuthorization(this.requestToken, this.verifierBox.Text);
			this.AccessToken = grantedAccess.AccessToken;
			DialogResult = true;
			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			Close();
		}
	}
}
