using System;
using System.Linq;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DotNetOAuth;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messaging;
using SampleServiceProvider;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.ServiceModel.Security;

/// <summary>
/// Sample consumer of our Service Provider sample's WCF service.
/// </summary>
public partial class SampleWcf : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (Session["WcfTokenManager"] != null) {
				Consumer consumer = this.CreateConsumer();
				var accessTokenMessage = consumer.ProcessUserAuthorization();
				if (accessTokenMessage != null) {
					Session["WcfAccessToken"] = accessTokenMessage.AccessToken;
					authorizationLabel.Text = "Authorized!  Access token: " + accessTokenMessage.AccessToken;
				}
			}
		}
	}

	protected void getAuthorizationButton_Click(object sender, EventArgs e) {
		Consumer consumer = this.CreateConsumer();
		UriBuilder callback = new UriBuilder(Request.Url);
		callback.Query = null;
		string scope = string.Join("|", (from item in scopeList.Items.OfType<ListItem>()
													where item.Selected
													select item.Value).ToArray());
		var requestParams = new Dictionary<string, string> {
			{ "scope", scope },
		};
		consumer.RequestUserAuthorization(callback.Uri, requestParams, null).Send();
	}

	protected void getNameButton_Click(object sender, EventArgs e) {
		try {
			nameLabel.Text = CallService(client => client.GetName());
		} catch (SecurityAccessDeniedException) {
			nameLabel.Text = "Access denied!";
		}
	}

	protected void getAgeButton_Click(object sender, EventArgs e) {
		try {
			int? age = CallService(client => client.GetAge());
			ageLabel.Text = age.HasValue ? age.Value.ToString(CultureInfo.CurrentCulture) : "not available";
		} catch (SecurityAccessDeniedException) {
			ageLabel.Text = "Access denied!";
		}
	}

	protected void getFavoriteSites_Click(object sender, EventArgs e) {
		try {
			string[] favoriteSites = CallService(client => client.GetFavoriteSites());
			favoriteSitesLabel.Text = string.Join(", ", favoriteSites);
		} catch (SecurityAccessDeniedException) {
			favoriteSitesLabel.Text = "Access denied!";
		}
	}

	private T CallService<T>(Func<DataApiClient, T> predicate) {
		DataApiClient client = new DataApiClient();
		var serviceEndpoint = new MessageReceivingEndpoint(client.Endpoint.Address.Uri, HttpDeliveryMethod.AuthorizationHeaderRequest | HttpDeliveryMethod.PostRequest);
		var accessToken = Session["WcfAccessToken"] as string;
		if (accessToken == null) {
			throw new InvalidOperationException("No access token!");
		}
		Consumer consumer = this.CreateConsumer();
		WebRequest httpRequest = consumer.CreateAuthorizedRequest(serviceEndpoint, accessToken);

		HttpRequestMessageProperty httpDetails = new HttpRequestMessageProperty();
		httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
		using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
			OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
			return predicate(client);
		}
	}

	private Consumer CreateConsumer() {
		string consumerKey = "sampleconsumer";
		string consumerSecret = "samplesecret";
		var tokenManager = Session["WcfTokenManager"] as InMemoryTokenManager;
		if (tokenManager == null) {
			tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
			Session["WcfTokenManager"] = tokenManager;
		}
		MessageReceivingEndpoint oauthEndpoint = new MessageReceivingEndpoint(
			new Uri("http://localhost:65169/ServiceProvider/OAuth.ashx"),
			HttpDeliveryMethod.PostRequest);
		Consumer consumer = new Consumer(
			new ServiceProviderDescription {
				RequestTokenEndpoint = oauthEndpoint,
				UserAuthorizationEndpoint = oauthEndpoint,
				AccessTokenEndpoint = oauthEndpoint,
				TamperProtectionElements = new DotNetOAuth.Messaging.ITamperProtectionChannelBindingElement[] {
					new HmacSha1SigningBindingElement(),
				},
			},
			tokenManager) {
				ConsumerKey = consumerKey,
				ConsumerSecret = consumerSecret,
			};

		return consumer;
	}
}
