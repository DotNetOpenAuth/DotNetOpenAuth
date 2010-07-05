namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth2;
	using OAuthConsumer.SampleServiceProvider;

	public partial class SampleWcf2 : System.Web.UI.Page {
		private static AuthorizationServerDescription AuthServerDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("http://localhost:65169/OAuth2.ashx/token"),
			AuthorizationEndpoint = new Uri("http://localhost:65169/OAuth2.ashx/auth"),
		};

		private static IAuthorizationState Authorization {
			get { return (AuthorizationState)HttpContext.Current.Session["Authorization"]; }
			set { HttpContext.Current.Session["Authorization"] = value; }
		}

		private static WebServerClient Client;

		static SampleWcf2() {
			Client = new WebServerClient(AuthServerDescription, "sampleconsumer", "samplesecret");
		}

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var authorization = Client.ProcessUserAuthorization();
				if (authorization != null) {
					Authorization = authorization;
				}
			}

			// Refresh the access token if it expires and if its lifetime is too short to be of use.
			if (Authorization != null && Authorization.AccessTokenExpirationUtc.HasValue) {
				Client.RefreshToken(Authorization, TimeSpan.FromMinutes(1));
			}
		}

		protected void getAuthorizationButton_Click(object sender, EventArgs e) {
			string[] scopes = (from item in this.scopeList.Items.OfType<ListItem>()
							   where item.Selected
							   select item.Value).ToArray();
			string scope = string.Join(" ", scopes);

			var response = Client.PrepareRequestUserAuthorization(scope);
			Client.Channel.Send(response);
		}

		protected void getNameButton_Click(object sender, EventArgs e) {
			try {
				this.nameLabel.Text = CallService(client => client.GetName());
			} catch (SecurityAccessDeniedException) {
				this.nameLabel.Text = "Access denied!";
			}
		}

		protected void getAgeButton_Click(object sender, EventArgs e) {
			try {
				int? age = CallService(client => client.GetAge());
				this.ageLabel.Text = age.HasValue ? age.Value.ToString(CultureInfo.CurrentCulture) : "not available";
			} catch (SecurityAccessDeniedException) {
				this.ageLabel.Text = "Access denied!";
			}
		}

		protected void getFavoriteSites_Click(object sender, EventArgs e) {
			try {
				string[] favoriteSites = CallService(client => client.GetFavoriteSites());
				this.favoriteSitesLabel.Text = string.Join(", ", favoriteSites);
			} catch (SecurityAccessDeniedException) {
				this.favoriteSitesLabel.Text = "Access denied!";
			}
		}

		private T CallService<T>(Func<DataApiClient, T> predicate) {
			DataApiClient client = new DataApiClient();
			////var serviceEndpoint = new MessageReceivingEndpoint(client.Endpoint.Address.Uri, HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest);
			if (Authorization == null) {
				throw new InvalidOperationException("No access token!");
			}

			var httpRequest = (HttpWebRequest)WebRequest.Create(client.Endpoint.Address.Uri);
			Client.AuthorizeRequest(httpRequest, Authorization.AccessToken);

			var httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
			using (OperationContextScope scope = new OperationContextScope(client.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(client);
			}
		}
	}
}