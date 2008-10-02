namespace DotNetOAuth.Samples.ConsumerWpf {
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
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Xml.Linq;
	using DotNetOAuth;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InMemoryTokenManager tokenManager = new InMemoryTokenManager();
		private Consumer google;
		private string requestToken;

		public MainWindow() {
			InitializeComponent();

			this.google = new Consumer(Constants.GoogleDescription, this.tokenManager);
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			this.tokenManager.ConsumerKey = consumerKeyBox.Text;
			this.tokenManager.ConsumerSecret = consumerSecretBox.Text;
			this.google.ConsumerKey = consumerKeyBox.Text;
			this.google.ConsumerSecret = consumerSecretBox.Text;

			var extraParameters = new Dictionary<string, string> {
				{ "scope", Constants.GoogleScopes.Contacts },
			};
			Uri browserAuthorizationLocation = this.google.RequestUserAuthorization(extraParameters, null, out this.requestToken);
			System.Diagnostics.Process.Start(browserAuthorizationLocation.AbsoluteUri);
		}

		private void completeAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			var grantedAccess = this.google.ProcessUserAuthorization(this.requestToken);
			Response contactsResponse = this.google.SendAuthorizedRequest(Constants.GoogleScopes.GetContacts, grantedAccess.AccessToken);
			XDocument contactsDocument = XDocument.Parse(contactsResponse.Body);
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
	}
}
