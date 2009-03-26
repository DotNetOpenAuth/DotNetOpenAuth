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
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using DotNetOpenAuth;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InMemoryTokenManager tokenManager = new InMemoryTokenManager();
		private DesktopConsumer google;
		private string requestToken;
		private string accessToken;

		public MainWindow() {
			InitializeComponent();

			this.google = GoogleConsumer.CreateDesktopConsumer(this.tokenManager, string.Empty);
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			this.tokenManager.ConsumerKey = consumerKeyBox.Text;
			this.tokenManager.ConsumerSecret = consumerSecretBox.Text;
			this.google.ConsumerKey = consumerKeyBox.Text;

			Cursor original = this.Cursor;
			this.Cursor = Cursors.Wait;
			beginAuthorizationButton.IsEnabled = false;
			ThreadPool.QueueUserWorkItem(delegate(object state) {
				Uri browserAuthorizationLocation = GoogleConsumer.RequestAuthorization(
					this.google,
					GoogleConsumer.Applications.Contacts | GoogleConsumer.Applications.Blogger,
					out this.requestToken);
				System.Diagnostics.Process.Start(browserAuthorizationLocation.AbsoluteUri);
				this.Dispatcher.BeginInvoke(new Action(() => {
					this.Cursor = original;
					beginAuthorizationButton.IsEnabled = true;
					completeAuthorizationButton.IsEnabled = true;
					postButton.IsEnabled = true;
				}));
			});
		}

		private void completeAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			var grantedAccess = this.google.ProcessUserAuthorization(this.requestToken);
			this.accessToken = grantedAccess.AccessToken;
			XDocument contactsDocument = GoogleConsumer.GetContacts(this.google, grantedAccess.AccessToken);
			var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
				select new {
					Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value,
					Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value,
				};
			contactsGrid.Children.Clear();
			foreach (var contact in contacts) {
				contactsGrid.RowDefinitions.Add(new RowDefinition());
				TextBlock name = new TextBlock { Text = contact.Name };
				TextBlock email = new TextBlock { Text = contact.Email };
				Grid.SetRow(name, contactsGrid.RowDefinitions.Count - 1);
				Grid.SetRow(email, contactsGrid.RowDefinitions.Count - 1);
				Grid.SetColumn(email, 1);
				contactsGrid.Children.Add(name);
				contactsGrid.Children.Add(email);
			}
		}

		private void postButton_Click(object sender, RoutedEventArgs e) {
			XElement postBodyXml = XElement.Parse(postBodyBox.Text);
			GoogleConsumer.PostBlogEntry(this.google, this.accessToken, blogUrlBox.Text, postTitleBox.Text, postBodyXml);
		}
	}
}
