namespace OAuthClient {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.ApplicationBlock.Facebook;
	using DotNetOpenAuth.OAuth2;

	public partial class WindowsLive : System.Web.UI.Page {
		private static readonly WindowsLiveClient client = new WindowsLiveClient {
			ClientIdentifier = ConfigurationManager.AppSettings["windowsLiveAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["WindowsLiveAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			if (string.Equals("localhost", this.Request.Headers["Host"].Split(':')[0], StringComparison.OrdinalIgnoreCase)) {
				this.localhostDoesNotWorkPanel.Visible = true;
				var builder = new UriBuilder(this.publicLink.NavigateUrl);
				builder.Port = this.Request.Url.Port;
				this.publicLink.NavigateUrl = builder.Uri.AbsoluteUri;
				this.publicLink.Text = builder.Uri.AbsoluteUri;
			} else {
				IAuthorizationState authorization = client.ProcessUserAuthorization();
				if (authorization == null) {
					// Kick off authorization request
					client.RequestUserAuthorization(scope: new[] { WindowsLiveClient.Scopes.Basic }); // this scope isn't even required just to log in
				} else {
					var request =
						WebRequest.Create("https://apis.live.net/v5.0/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
					using (var response = request.GetResponse()) {
						using (var responseStream = response.GetResponseStream()) {
							var graph = WindowsLiveGraph.Deserialize(responseStream);
							this.nameLabel.Text = HttpUtility.HtmlEncode(graph.Name);
						}
					}
				}
			}
		}
	}
}
