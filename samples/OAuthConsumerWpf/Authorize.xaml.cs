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
using System.Windows.Shapes;
using System.Threading;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.ApplicationBlock;
using System.Xml.Linq;

namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	/// <summary>
	/// Interaction logic for Authorize.xaml
	/// </summary>
	partial class Authorize : Window {
		private DesktopConsumer google;
		private string requestToken;

		internal string AccessToken { get; set; }

		internal Authorize(DesktopConsumer consumer) {
			InitializeComponent();

			this.google = consumer;
			Cursor original = this.Cursor;
			this.Cursor = Cursors.Wait;
			ThreadPool.QueueUserWorkItem(delegate(object state) {
				Uri browserAuthorizationLocation = GoogleConsumer.RequestAuthorization(
					this.google,
					GoogleConsumer.Applications.Contacts | GoogleConsumer.Applications.Blogger,
					out this.requestToken);
				System.Diagnostics.Process.Start(browserAuthorizationLocation.AbsoluteUri);
				this.Dispatcher.BeginInvoke(new Action(() => {
					this.Cursor = original;
					finishButton.IsEnabled = true;
				}));
			});

		}

		private void finishButton_Click(object sender, RoutedEventArgs e) {
			var grantedAccess = this.google.ProcessUserAuthorization(this.requestToken, verifierBox.Text);
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
