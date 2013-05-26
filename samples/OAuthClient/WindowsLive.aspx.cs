namespace OAuthClient
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth2;

	public partial class WindowsLive : System.Web.UI.Page
	{
		private static readonly WindowsLiveClient windowsLiveClient = new WindowsLiveClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["windowsLiveAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["WindowsLiveAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e)
		{
			if (string.Equals("localhost", this.Request.Headers["Host"].Split(':')[0], StringComparison.OrdinalIgnoreCase))
			{
				this.localhostDoesNotWorkPanel.Visible = true;
				var builder = new UriBuilder(this.publicLink.NavigateUrl);
				builder.Port = this.Request.Url.Port;
				this.publicLink.NavigateUrl = builder.Uri.AbsoluteUri;
				this.publicLink.Text = builder.Uri.AbsoluteUri;
			}
			else
			{
				IAuthorizationState authorization = windowsLiveClient.ProcessUserAuthorization();
				if (authorization == null)
				{
					// Kick off authorization request
					windowsLiveClient.RequestUserAuthorization(scope: new[] { WindowsLiveClient.Scopes.Basic }); // this scope isn't even required just to log in

					// alternatively you can ask for more information
					// windowsLiveClient.RequestUserAuthorization(scope: new[] { WindowsLiveClient.Scopes.SignIn, WindowsLiveClient.Scopes.Emails, WindowsLiveClient.Scopes.Birthday });
				}
				else
				{
					IOAuth2Graph oauth2Graph = windowsLiveClient.GetGraph(authorization);

					this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.Name);
				}
			}
		}
	}
}
