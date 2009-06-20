namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
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
		private string accessToken;

		public MainWindow() {
			InitializeComponent();

			this.tokenManager.ConsumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
			this.tokenManager.ConsumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];

			string pfxFile = ConfigurationManager.AppSettings["googleConsumerCertificateFile"];
			if (string.IsNullOrEmpty(pfxFile)) {
				this.google = new DesktopConsumer(GoogleConsumer.ServiceDescription, this.tokenManager);
			} else {
				string pfxPassword = ConfigurationManager.AppSettings["googleConsumerCertificatePassword"];
				var signingCertificate = new X509Certificate2(pfxFile, pfxPassword);
				var service = GoogleConsumer.CreateRsaSha1ServiceDescription(signingCertificate);
				this.google = new DesktopConsumer(service, this.tokenManager);
			}
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrEmpty(this.tokenManager.ConsumerKey)) {
				MessageBox.Show(this, "You must modify the App.config or OAuthConsumerWpf.exe.config file for this application to include your Google OAuth consumer key first.", "Configuration required", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Authorize auth = new Authorize(this.google);
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.accessToken = auth.AccessToken;
				postButton.IsEnabled = true;

				XDocument contactsDocument = GoogleConsumer.GetContacts(this.google, this.accessToken);
				var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
							   select new { Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value, Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value };
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

		private void postButton_Click(object sender, RoutedEventArgs e) {
			XElement postBodyXml = XElement.Parse(postBodyBox.Text);
			GoogleConsumer.PostBlogEntry(this.google, this.accessToken, blogUrlBox.Text, postTitleBox.Text, postBodyXml);
		}
	}
}
