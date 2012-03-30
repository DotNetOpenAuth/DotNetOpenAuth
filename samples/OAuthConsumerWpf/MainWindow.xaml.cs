namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Windows;
	using System.Windows.Controls;
	using System.Xml.Linq;

	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Samples.OAuthConsumerWpf.WcfSampleService;

	using OAuth2;

	using OAuth2 = DotNetOpenAuth.OAuth2;
	using ProtocolVersion = DotNetOpenAuth.OAuth.ProtocolVersion;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InMemoryTokenManager googleTokenManager = new InMemoryTokenManager();
		private DesktopConsumer google;
		private string googleAccessToken;
		private UserAgentClient wcf;
		private IAuthorizationState wcfAccessToken;

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
			var authServer = new AuthorizationServerDescription() {
				AuthorizationEndpoint = new Uri("http://localhost:50172/OAuth/Authorize"),
				TokenEndpoint = new Uri("http://localhost:50172/OAuth/Token"),
			};
			this.wcf = new UserAgentClient(authServer, "sampleconsumer", "samplesecret");
		}

		private void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrEmpty(this.googleTokenManager.ConsumerKey)) {
				MessageBox.Show(this, "You must modify the App.config or OAuthConsumerWpf.exe.config file for this application to include your Google OAuth consumer key first.", "Configuration required", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			var auth = new Authorize(
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
			var auth = new Authorize2(this.wcf);
			auth.Authorization.Scope.AddRange(OAuthUtilities.SplitScopes("http://tempuri.org/IDataApi/GetName http://tempuri.org/IDataApi/GetAge http://tempuri.org/IDataApi/GetFavoriteSites"));
			auth.Authorization.Callback = new Uri("http://localhost:59721/");
			auth.Owner = this;
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.wcfAccessToken = auth.Authorization;
				this.wcfName.Content = this.CallService(client => client.GetName());
				this.wcfAge.Content = this.CallService(client => client.GetAge());
				this.wcfFavoriteSites.Content = this.CallService(client => string.Join(", ", client.GetFavoriteSites()));
			}
		}

		private T CallService<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			if (this.wcfAccessToken == null) {
				throw new InvalidOperationException("No access token!");
			}

			var httpRequest = (HttpWebRequest)WebRequest.Create(client.Endpoint.Address.Uri);
			this.wcf.AuthorizeRequest(httpRequest, this.wcfAccessToken);

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

		private void oauth2BeginButton_Click(object sender, RoutedEventArgs e) {
			var authServer = new DotNetOpenAuth.OAuth2.AuthorizationServerDescription {
				AuthorizationEndpoint = new Uri(this.oauth2AuthorizationUrlBox.Text),
			};
			if (this.oauth2TokenEndpointBox.Text.Length > 0) {
				authServer.TokenEndpoint = new Uri(this.oauth2TokenEndpointBox.Text);
			}

			try {
				var client = new OAuth2.UserAgentClient(authServer, this.oauth2ClientIdentifierBox.Text, this.oauth2ClientSecretBox.Text);

				var authorizePopup = new Authorize2(client);
				authorizePopup.Authorization.Scope.AddRange(OAuthUtilities.SplitScopes(this.oauth2ScopeBox.Text));
				authorizePopup.Owner = this;
				bool? result = authorizePopup.ShowDialog();
				if (result.HasValue && result.Value) {
					var requestUri = new UriBuilder(this.oauth2ResourceUrlBox.Text);
					if (this.oauth2ResourceHttpMethodList.SelectedIndex > 0) {
						requestUri.AppendQueryArgument("access_token", authorizePopup.Authorization.AccessToken);
					}

					var request = (HttpWebRequest)WebRequest.Create(requestUri.Uri);
					request.Method = this.oauth2ResourceHttpMethodList.SelectedIndex < 2 ? "GET" : "POST";
					if (this.oauth2ResourceHttpMethodList.SelectedIndex == 0) {
						client.AuthorizeRequest(request, authorizePopup.Authorization);
					}

					using (var resourceResponse = request.GetResponse()) {
						using (var responseStream = new StreamReader(resourceResponse.GetResponseStream())) {
							this.oauth2ResultsBox.Text = responseStream.ReadToEnd();
						}
					}
				} else {
					return;
				}
			} catch (Messaging.ProtocolException ex) {
				MessageBox.Show(this, ex.Message);
			} catch (WebException ex) {
				string responseText = string.Empty;
				if (ex.Response != null) {
					using (var responseReader = new StreamReader(ex.Response.GetResponseStream())) {
						responseText = responseReader.ReadToEnd();
					}
				}
				MessageBox.Show(this, ex.Message + "  " + responseText);
			}
		}
	}
}
