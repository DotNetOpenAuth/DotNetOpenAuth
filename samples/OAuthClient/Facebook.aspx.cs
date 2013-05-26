namespace OAuthClient
{
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth2;

	public partial class Facebook : System.Web.UI.Page
	{
		private static readonly FacebookClient facebookClient = new FacebookClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["facebookAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e)
		{
			IAuthorizationState authorization = facebookClient.ProcessUserAuthorization();
			if (authorization == null)
			{
				// Kick off authorization request
				facebookClient.RequestUserAuthorization();

				// alternatively you can ask for more information
				// facebookClient.RequestUserAuthorization(scope: new[] { FacebookClient.Scopes.Email, FacebookClient.Scopes.UserBirthday });
			}
			else
			{
				IOAuth2Graph oauth2Graph = facebookClient.GetGraph(authorization);
				//// IOAuth2Graph oauth2Graph = facebookClient.GetGraph(authorization, new[] { FacebookGraph.Fields.Defaults, FacebookGraph.Fields.Email, FacebookGraph.Fields.Picture, FacebookGraph.Fields.Birthday });

				this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.Name);
			}
		}
	}
}