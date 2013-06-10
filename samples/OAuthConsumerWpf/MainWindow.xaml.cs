namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Threading;
	using System.Threading.Tasks;
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
		private GoogleConsumer google;
		private DotNetOpenAuth.OAuth.AccessToken googleAccessToken;
		private UserAgentClient wcf;
		private IAuthorizationState wcfAccessToken;

		public MainWindow() {
			this.InitializeComponent();

			this.InitializeGoogleConsumer();
			this.InitializeWcfConsumer();
		}

		private void InitializeGoogleConsumer() {
			string pfxFile = ConfigurationManager.AppSettings["googleConsumerCertificateFile"];
			this.google = new GoogleConsumer();
			if (!string.IsNullOrEmpty(pfxFile)) {
				string pfxPassword = ConfigurationManager.AppSettings["googleConsumerCertificatePassword"];
				var signingCertificate = new X509Certificate2(pfxFile, pfxPassword);
				this.google.ConsumerCertificate = signingCertificate;
			}
		}

		private void InitializeWcfConsumer() {
			var authServer = new AuthorizationServerDescription() {
				AuthorizationEndpoint = new Uri("http://localhost:50172/OAuth/Authorize"),
				TokenEndpoint = new Uri("http://localhost:50172/OAuth/Token"),
			};
			this.wcf = new UserAgentClient(authServer, "sampleconsumer", "samplesecret");
		}

		private async void beginAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			if (string.IsNullOrEmpty(this.google.ConsumerKey)) {
				MessageBox.Show(this, "You must modify the App.config or OAuthConsumerWpf.exe.config file for this application to include your Google OAuth consumer key first.", "Configuration required", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			var auth = new Authorize(
				this.google,
				consumer =>
				((GoogleConsumer)consumer).RequestUserAuthorizationAsync(GoogleConsumer.Applications.Contacts | GoogleConsumer.Applications.Blogger));
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.googleAccessToken = auth.AccessToken;
				this.postButton.IsEnabled = true;

				XDocument contactsDocument = await this.google.GetContactsAsync(this.googleAccessToken);
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

		private async void postButton_Click(object sender, RoutedEventArgs e) {
			XElement postBodyXml = XElement.Parse(this.postBodyBox.Text);
			await this.google.PostBlogEntryAsync(this.googleAccessToken, this.blogUrlBox.Text, this.postTitleBox.Text, postBodyXml);
		}

		private async void beginWcfAuthorizationButton_Click(object sender, RoutedEventArgs e) {
			var auth = new Authorize2(this.wcf);
			auth.Authorization.Scope.AddRange(OAuthUtilities.SplitScopes("http://tempuri.org/IDataApi/GetName http://tempuri.org/IDataApi/GetAge http://tempuri.org/IDataApi/GetFavoriteSites"));
			auth.Authorization.Callback = new Uri("http://localhost:59721/");
			auth.Owner = this;
			bool? result = auth.ShowDialog();
			if (result.HasValue && result.Value) {
				this.wcfAccessToken = auth.Authorization;
				this.wcfName.Content = await this.CallServiceAsync(client => client.GetName());
				this.wcfAge.Content = await this.CallServiceAsync(client => client.GetAge());
				this.wcfFavoriteSites.Content = await this.CallServiceAsync(client => string.Join(", ", client.GetFavoriteSites()));
			}
		}

		private async Task<T> CallServiceAsync<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			if (this.wcfAccessToken == null) {
				throw new InvalidOperationException("No access token!");
			}

			var httpRequest = (HttpWebRequest)WebRequest.Create(client.Endpoint.Address.Uri);
			await this.wcf.AuthorizeRequestAsync(httpRequest, this.wcfAccessToken, CancellationToken.None);

			HttpRequestMessageProperty httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
			using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(client);
			}
		}

		private async void beginButton_Click(object sender, RoutedEventArgs e) {
			try {
				var service = new ServiceProviderDescription(
					this.requestTokenUrlBox.Text,
					this.authorizeUrlBox.Text,
					this.accessTokenUrlBox.Text);

				var consumer = new Consumer(this.consumerKeyBox.Text, this.consumerSecretBox.Text, service, new MemoryTemporaryCredentialStorage());
				DotNetOpenAuth.OAuth.AccessToken accessToken;
				var authorizePopup = new Authorize(consumer, c => c.RequestUserAuthorizationAsync(null, null));
				authorizePopup.Owner = this;
				bool? result = authorizePopup.ShowDialog();
				if (result.HasValue && result.Value) {
					accessToken = authorizePopup.AccessToken;
				} else {
					return;
				}

				HttpMethod resourceHttpMethod = this.resourceHttpMethodList.SelectedIndex < 2 ? HttpMethod.Get : HttpMethod.Post;
				using (var handler = consumer.CreateMessageHandler(accessToken)) {
					handler.Location = this.resourceHttpMethodList.SelectedIndex == 1
										   ? OAuth1HttpMessageHandlerBase.OAuthParametersLocation.AuthorizationHttpHeader
										   : OAuth1HttpMessageHandlerBase.OAuthParametersLocation.QueryString;
					using (var httpClient = consumer.CreateHttpClient(handler)) {
						var request = new HttpRequestMessage(resourceHttpMethod, this.resourceUrlBox.Text);
						using (var resourceResponse = await httpClient.SendAsync(request)) {
							this.resultsBox.Text = await resourceResponse.Content.ReadAsStringAsync();
						}
					}
				}
			} catch (DotNetOpenAuth.Messaging.ProtocolException ex) {
				MessageBox.Show(this, ex.Message);
			}
		}

		private async void oauth2BeginButton_Click(object sender, RoutedEventArgs e) {
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
				authorizePopup.Authorization.Callback = new Uri("http://www.microsoft.com/en-us/default.aspx");
				authorizePopup.Owner = this;
				authorizePopup.ClientAuthorizationView.RequestImplicitGrant = this.flowBox.SelectedIndex == 1;
				bool? result = authorizePopup.ShowDialog();
				if (result.HasValue && result.Value) {
					var request = new HttpRequestMessage(
						new HttpMethod(((ComboBoxItem)this.oauth2ResourceHttpMethodList.SelectedValue).Content.ToString()),
						this.oauth2ResourceUrlBox.Text);
					using (var httpClient = new HttpClient(client.CreateAuthorizingHandler(authorizePopup.Authorization))) {
						using (var resourceResponse = await httpClient.SendAsync(request)) {
							this.oauth2ResultsBox.Text = await resourceResponse.Content.ReadAsStringAsync();
						}
					}
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
