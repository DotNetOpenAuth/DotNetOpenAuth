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

	public partial class Google : System.Web.UI.Page
	{
		private static readonly GoogleClient googleClient = new GoogleClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["googleClientID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["googleClientSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						IAuthorizationState authorization = await googleClient.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), ct);
						if (authorization == null) {
							// Kick off authorization request
							var request = await googleClient.PrepareRequestUserAuthorizationAsync(cancellationToken: ct);
							await request.SendAsync(new HttpContextWrapper(Context), ct);
							this.Context.Response.End();

							// alternatively you can ask for more information
							// googleClient.RequestUserAuthorization(scope: new[] { GoogleClient.Scopes.UserInfo.Profile, GoogleClient.Scopes.UserInfo.Email });
						} else {
							IOAuth2Graph oauth2Graph = await googleClient.GetGraphAsync(authorization, cancellationToken: ct);

							this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.Name);
						}
					}));
		}
	}
}