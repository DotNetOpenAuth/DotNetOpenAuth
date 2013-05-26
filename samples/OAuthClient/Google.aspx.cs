namespace OAuthClient
{
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth2;

	public partial class Google : System.Web.UI.Page
	{
		private static readonly GoogleClient googleClient = new GoogleClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["googleClientID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["googleClientSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e)
		{
			IAuthorizationState authorization = googleClient.ProcessUserAuthorization();
			if (authorization == null)
			{
				// Kick off authorization request
				googleClient.RequestUserAuthorization();

				// alternatively you can ask for more information
				// googleClient.RequestUserAuthorization(scope: new[] { GoogleClient.Scopes.UserInfo.Profile, GoogleClient.Scopes.UserInfo.Email });
			}
			else
			{
				IOAuth2Graph oauth2Graph = googleClient.GetGraph(authorization);

				this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.Name);
			}
		}
	}
}