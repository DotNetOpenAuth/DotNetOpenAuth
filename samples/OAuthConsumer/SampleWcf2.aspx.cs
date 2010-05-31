using DotNetOpenAuth.OAuthWrap;

namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	public partial class SampleWcf2 : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack)
			{
				var client = CreateClient();
				var authorization = client.ProcessUserAuthorization();
				if (authorization != null)
				{
					Response.Write("Obtained access token: " + authorization.AccessToken);
				}
			}
		}

		protected void getAuthorizationButton_Click(object sender, EventArgs e) {
			var client = CreateClient();
			var response = client.PrepareRequestUserAuthorization();

			string[] scopes = (from item in this.scopeList.Items.OfType<ListItem>()
			                   where item.Selected
			                   select item.Value).ToArray();
			response.Scope = string.Join("|", scopes);
			client.Channel.Send(response);
		}

		private static WebAppClient CreateClient() {
			var authServerDescription = new AuthorizationServerDescription {
				TokenEndpoint = new Uri("http://localhost:65169/OAuth2.ashx/token"),
				AuthorizationEndpoint = new Uri("http://localhost:65169/OAuth2.ashx/auth"),
			};
			var client = new WebAppClient(authServerDescription)
			{
				ClientIdentifier = "sampleconsumer",
				ClientSecret = "samplesecret",
			};
			return client;
		}
	}
}