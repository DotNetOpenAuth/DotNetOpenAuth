namespace OAuthClient
{
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using System.Web.UI;

	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	public partial class Facebook : System.Web.UI.Page
	{
		private static readonly FacebookClient facebookClient = new FacebookClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["facebookAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						IAuthorizationState authorization = await facebookClient.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), ct);
						if (authorization == null) {
							// Kick off authorization request
							var request = await facebookClient.PrepareRequestUserAuthorizationAsync(cancellationToken: ct);
							await request.SendAsync(new HttpContextWrapper(Context), ct);
							this.Context.Response.End();

							// alternatively you can ask for more information
							// facebookClient.RequestUserAuthorization(scope: new[] { FacebookClient.Scopes.Email, FacebookClient.Scopes.UserBirthday });
						} else {
							IOAuth2Graph oauth2Graph = await facebookClient.GetGraphAsync(authorization, cancellationToken: ct);
							//// IOAuth2Graph oauth2Graph = facebookClient.GetGraph(authorization, new[] { FacebookGraph.Fields.Defaults, FacebookGraph.Fields.Email, FacebookGraph.Fields.Picture, FacebookGraph.Fields.Birthday });

							this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.Name);
						}
					}));
		}
	}
}