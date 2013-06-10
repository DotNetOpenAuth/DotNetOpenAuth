namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using OAuthConsumer.SampleServiceProvider;

	/// <summary>
	/// Sample consumer of our Service Provider sample's WCF service.
	/// </summary>
	public partial class SampleWcf : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!IsPostBack) {
							var consumer = this.CreateConsumer();
							if (consumer.ConsumerKey != null) {
								var accessTokenMessage = await consumer.ProcessUserAuthorizationAsync(this.Request.Url);
								if (accessTokenMessage != null) {
									Session["WcfAccessToken"] = accessTokenMessage.AccessToken;
									this.authorizationLabel.Text = "Authorized!  Access token: " + accessTokenMessage.AccessToken.Token;
								}
							}
						}
					}));
		}

		protected void getAuthorizationButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var consumer = this.CreateConsumer();
						UriBuilder callback = new UriBuilder(Request.Url);
						callback.Query = null;
						string[] scopes =
							(from item in this.scopeList.Items.OfType<ListItem>() where item.Selected select item.Value).ToArray();
						string scope = string.Join("|", scopes);
						var requestParams = new Dictionary<string, string> { { "scope", scope }, };
						Uri redirectUri = await consumer.RequestUserAuthorizationAsync(callback.Uri, requestParams);
						this.Response.Redirect(redirectUri.AbsoluteUri);
					}));
		}

		protected void getNameButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							this.nameLabel.Text = await this.CallServiceAsync(client => client.GetName());
						} catch (SecurityAccessDeniedException) {
							this.nameLabel.Text = "Access denied!";
						}
					}));
		}

		protected void getAgeButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							int? age = await this.CallServiceAsync(client => client.GetAge());
							this.ageLabel.Text = age.HasValue ? age.Value.ToString(CultureInfo.CurrentCulture) : "not available";
						} catch (SecurityAccessDeniedException) {
							this.ageLabel.Text = "Access denied!";
						}
					}));
		}

		protected void getFavoriteSites_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							string[] favoriteSites = await this.CallServiceAsync(client => client.GetFavoriteSites());
							this.favoriteSitesLabel.Text = string.Join(", ", favoriteSites);
						} catch (SecurityAccessDeniedException) {
							this.favoriteSitesLabel.Text = "Access denied!";
						}
					}));
		}

		private async Task<T> CallServiceAsync<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			var serviceEndpoint = new MessageReceivingEndpoint(client.Endpoint.Address.Uri, HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest);
			var accessToken = (AccessToken)(Session["WcfAccessToken"] ?? default(AccessToken));
			if (accessToken.Token == null) {
				throw new InvalidOperationException("No access token!");
			}

			var httpRequest = new HttpRequestMessage(HttpMethod.Post, client.Endpoint.Address.Uri);
			var consumer = this.CreateConsumer();
			using (var handler = consumer.CreateMessageHandler(accessToken)) {
				handler.ApplyAuthorization(httpRequest);
			}

			HttpRequestMessageProperty httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers.Authorization.ToString();
			using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(client);
			}
		}

		private Consumer CreateConsumer() {
			string consumerKey = "sampleconsumer";
			string consumerSecret = "samplesecret";
			MessageReceivingEndpoint oauthEndpoint = new MessageReceivingEndpoint(
				new Uri("http://localhost:65169/OAuth.ashx"),
				HttpDeliveryMethods.PostRequest);
			var consumer = new Consumer(
				consumerKey,
				consumerSecret,
				new ServiceProviderDescription(oauthEndpoint.Location.AbsoluteUri, oauthEndpoint.Location.AbsoluteUri, oauthEndpoint.Location.AbsoluteUri),
				new CookieTemporaryCredentialStorage());

			return consumer;
		}
	}
}