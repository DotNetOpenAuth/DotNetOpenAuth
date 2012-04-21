namespace OAuthClient {
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.ApplicationBlock.Facebook;
	using DotNetOpenAuth.OAuth2;

	public partial class Facebook : System.Web.UI.Page {
		private static readonly FacebookClient client = new FacebookClient {
			ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["facebookAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			IAuthorizationState authorization = client.ProcessUserAuthorization();
			if (authorization == null) {
				// Kick off authorization request
				client.RequestUserAuthorization();
			} else {
				var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
				using (var response = request.GetResponse()) {
					using (var responseStream = response.GetResponseStream()) {
						var graph = FacebookGraph.Deserialize(responseStream);
						this.nameLabel.Text = HttpUtility.HtmlEncode(graph.Name);
					}
				}
			}
		}
	}
}