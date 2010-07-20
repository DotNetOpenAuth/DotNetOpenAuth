namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
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
	using DotNetOpenAuth.Samples.OAuthConsumerWpf.WcfSampleService;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InMemoryTokenManager googleTokenManager = new InMemoryTokenManager();
		private DesktopConsumer google;
		private string googleAccessToken;
		private InMemoryTokenManager wcfTokenManager = new InMemoryTokenManager();
		private DesktopConsumer wcf;
		private string wcfAccessToken;

		public MainWindow() {
			this.InitializeComponent();

			this.InitializeGoogleConsumer();
			this.InitializeWcfConsumer();
		}

		private void InitializeGoogleConsumer() {
			this.googleTokenManager.ConsumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
			this.googleTokenManager.ConsumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];

			string pfxFile = ConfigurationManager.AppSettings["googleConsumerCertificateFile"];
			if (string.IsNullOrEmpty(pfxFile)) {
				this.google = new DesktopConsumer(GoogleConsumer.ServiceDescription, this.googleTokenManager);
			} else {
				string pfxPassword = ConfigurationManager.AppSettings["googleConsumerCertificatePassword"];
				var signingCertificate = new X509Certificate2(pfxFile, pfxPassword);
				var service = GoogleConsumer.CreateRsaSha1ServiceDescription(signingCertificate);
				this.google = new DesktopConsumer(service, this.googleTokenManager);
			}
		}

		private void InitializeWcfConsumer() {
			this.wcfTokenManager.ConsumerKey = "sampleconsumer";
			this.wcfTokenManager.ConsumerSecret = "samplesecret";
			MessageReceivingEndpoint oauthEndpoint = new MessageReceivingEndpoint(
				new Uri("http://localhost:65169/OAuth.ashx"),
				HttpDeliveryMethods.PostRequest);
			this.wcf = new DesktopConsumer(
				new ServiceProviderDescription {
					RequestTokenEndpoint = oauthEndpoint,
					UserAuthorizationEndpoint = oauthEndpoint,
					AccessTokenEndpoint = oauthEndpoint,
					TamperProtectionElements = new DotNetOpenAuth.Messaging.ITamperProtectionChannelBindingElement[] {
						new HmacSha1SigningBindingElement(),
					},
				},
				this.wcfTokenManager);
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrEmpty(this.googleTokenManager.ConsumerKey)) {
				MessageBox.Show(this, "You must modify the App.config or OAuthConsumerWpf.exe.config file for this application to include your Google OAuth consumer key first.", "Configuration required", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Authorize auth = new Authorize(
				this.google,
				(DesktopConsumer consumer, out string requestToken) =>
				GoogleConsumer.RequestAuthorization(
					consumer,
					GoogleConsumer.Applications.Contacts | GoogleConsumer.Applications.Blogger,
					out requestToken));
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.googleAccessToken = auth.AccessToken;
				this.postButton.IsEnabled = true;

				XDocument contactsDocument = GoogleConsumer.GetContacts(this.google, this.googleAccessToken, 25, 1);
				var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
							   select new { Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value, Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value };
				this.contactsGrid.Children.Clear();
				foreach (var contact in contacts) {
					this.contactsGrid.RowDefinitions.Add(new RowDefinition());
					TextBlock name = new TextBlock { Text = contact.Name };
					TextBlock email = new TextBlock { Text = contact.Email };
					Grid.SetRow(name, this.contactsGrid.RowDefinitions.Count - 1);
					Grid.SetRow(email, this.contactsGrid.RowDefinitions.Count - 1);
					Grid.SetColumn(email, 1);
					this.contactsGrid.Children.Add(name);
					this.contactsGrid.Children.Add(email);
				}
			}
		}

		private void postButton_Click(object sender, RoutedEventArgs e) {
			XElement postBodyXml = XElement.Parse(this.postBodyBox.Text);
			GoogleConsumer.PostBlogEntry(this.google, this.googleAccessToken, this.blogUrlBox.Text, this.postTitleBox.Text, postBodyXml);
		}

		private void beginWcfAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			var requestArgs = new Dictionary<string, string>();
			requestArgs["scope"] = "http://tempuri.org/IDataApi/GetName|http://tempuri.org/IDataApi/GetAge|http://tempuri.org/IDataApi/GetFavoriteSites";
			Authorize auth = new Authorize(
				this.wcf,
				(DesktopConsumer consumer, out string requestToken) => consumer.RequestUserAuthorization(requestArgs, null, out requestToken));
			auth.Owner = this;
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.wcfAccessToken = auth.AccessToken;
				this.wcfName.Content = CallService(client => client.GetName());
				this.wcfAge.Content = CallService(client => client.GetAge());
				this.wcfFavoriteSites.Content = CallService(client => string.Join(", ", client.GetFavoriteSites()));
			}
		}

		private T CallService<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			var serviceEndpoint = new MessageReceivingEndpoint(client.Endpoint.Address.Uri, HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest);
			if (this.wcfAccessToken == null) {
				throw new InvalidOperationException("No access token!");
			}
			WebRequest httpRequest = this.wcf.PrepareAuthorizedRequest(serviceEndpoint, this.wcfAccessToken);

			HttpRequestMessageProperty httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
			using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(client);
			}
		}

		private void beginButton_Click(object sender, RoutedEventArgs e) {
			try {
				var service = new ServiceProviderDescription {
					RequestTokenEndpoint = new MessageReceivingEndpoint(this.requestTokenUrlBox.Text, this.requestTokenHttpMethod.SelectedIndex == 0 ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest),
					UserAuthorizationEndpoint = new MessageReceivingEndpoint(this.authorizeUrlBox.Text, HttpDeliveryMethods.GetRequest),
					AccessTokenEndpoint = new MessageReceivingEndpoint(this.accessTokenUrlBox.Text, this.accessTokenHttpMethod.SelectedIndex == 0 ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest),
					TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
					ProtocolVersion = this.oauthVersion.SelectedIndex == 0 ? ProtocolVersion.V10 : ProtocolVersion.V10a,
				};
				var tokenManager = new InMemoryTokenManager();
				tokenManager.ConsumerKey = this.consumerKeyBox.Text;
				tokenManager.ConsumerSecret = this.consumerSecretBox.Text;

				var consumer = new DesktopConsumer(service, tokenManager);
				string accessToken;
				if (service.ProtocolVersion == ProtocolVersion.V10) {
					string requestToken;
					Uri authorizeUrl = consumer.RequestUserAuthorization(null, null, out requestToken);
					Process.Start(authorizeUrl.AbsoluteUri);
					MessageBox.Show(this, "Click OK when you've authorized the app.");
					var authorizationResponse = consumer.ProcessUserAuthorization(requestToken, null);
					accessToken = authorizationResponse.AccessToken;
				} else {
					var authorizePopup = new Authorize(
						consumer,
						(DesktopConsumer c, out string requestToken) => c.RequestUserAuthorization(null, null, out requestToken));
					authorizePopup.Owner = this;
					bool? result = authorizePopup.ShowDialog();
					if (result.HasValue && result.Value) {
						accessToken = authorizePopup.AccessToken;
					} else {
						return;
					}
				}
				HttpDeliveryMethods resourceHttpMethod = this.resourceHttpMethodList.SelectedIndex < 2 ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest;
				if (this.resourceHttpMethodList.SelectedIndex == 1) {
					resourceHttpMethod |= HttpDeliveryMethods.AuthorizationHeaderRequest;
				}
				var resourceEndpoint = new MessageReceivingEndpoint(this.resourceUrlBox.Text, resourceHttpMethod);
				using (IncomingWebResponse resourceResponse = consumer.PrepareAuthorizedRequestAndSend(resourceEndpoint, accessToken)) {
					this.resultsBox.Text = resourceResponse.GetResponseReader().ReadToEnd();
				}
			} catch (DotNetOpenAuth.Messaging.ProtocolException ex) {
				MessageBox.Show(this, ex.Message);
			}
		}
	}
}
