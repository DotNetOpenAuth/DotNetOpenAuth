namespace OAuthClient {
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using System.Web.UI;

	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.ApplicationBlock.Facebook;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	public partial class Facebook : System.Web.UI.Page {
		private static readonly FacebookClient client = new FacebookClient {
			ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["facebookAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						IAuthorizationState authorization =
							await client.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), ct);
						if (authorization == null) {
							// Kick off authorization request
							var request =
								await client.PrepareRequestUserAuthorizationAsync(cancellationToken: ct);
							await request.SendAsync(new HttpContextWrapper(Context), ct);
							this.Context.Response.End();
						} else {
							var request =
								WebRequest.Create(
									"https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
							using (var response = request.GetResponse()) {
								using (var responseStream = response.GetResponseStream()) {
									var graph = FacebookGraph.Deserialize(responseStream);
									this.nameLabel.Text = HttpUtility.HtmlEncode(graph.Name);
								}
							}
						}
					}));
		}
	}
}