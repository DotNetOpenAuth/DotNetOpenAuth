using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuthWrap;
using System.Net;
using System.IO;

public partial class Facebook : System.Web.UI.Page {
	private static readonly FacebookClient client = new FacebookClient {
		ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
		ClientSecret = ConfigurationManager.AppSettings["facebookAppSecret"],
	};

	protected void Page_Load(object sender, EventArgs e)
	{
		IAuthorizationState authorization = client.ProcessUserAuthorization();
		if (authorization == null)
		{
			// Kick off authorization request
			client.Channel.Send(client.PrepareRequestUserAuthorization());
		}
		else
		{
			var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
			using (var response = request.GetResponse()) {
				using (var responseReader = new StreamReader(response.GetResponseStream())) {
					string data = responseReader.ReadToEnd();
				}
			}
			nameLabel.Text = "Success!  Access token: " + HttpUtility.HtmlEncode(authorization.AccessToken);
		}
	}
}