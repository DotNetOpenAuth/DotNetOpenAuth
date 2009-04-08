using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetOpenAuth.ApplicationBlock;

public partial class Twitter : System.Web.UI.Page {
	private string AccessToken {
		get { return (string)ViewState["AccessToken"]; }
		set { ViewState["AccessToken"] = value; }
	}

	private InMemoryTokenManager TokenManager {
		get {
			var tokenManager = (InMemoryTokenManager)Session["TokenManager"];
			if (tokenManager == null) {
				string consumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
				string consumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
				if (!string.IsNullOrEmpty(consumerKey)) {
					tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
					Session["TokenManager"] = tokenManager;
				}
			}

			return tokenManager;
		}
	}

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (TokenManager != null) {
				InMemoryTokenManager tokenManager = (InMemoryTokenManager)Session["TokenManager"];
				var twitter = TwitterConsumer.CreateWebConsumer(tokenManager, tokenManager.ConsumerKey);

				var accessTokenResponse = twitter.ProcessUserAuthorization();
				if (accessTokenResponse != null) {
					// User has approved access
					MultiView1.ActiveViewIndex = 1;

					this.AccessToken = accessTokenResponse.AccessToken;
				}
			} else {
				MultiView1.ActiveViewIndex = 2;
			}
		}
	}

	protected void authorizeButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		var twitter = TwitterConsumer.CreateWebConsumer(this.TokenManager, this.TokenManager.ConsumerKey);
		twitter.Channel.Send(twitter.PrepareRequestUserAuthorization());
	}

	protected void downloadUpdates_Click(object sender, EventArgs e) {
		var twitter = TwitterConsumer.CreateWebConsumer(this.TokenManager, this.TokenManager.ConsumerKey);
		XPathDocument updates = new XPathDocument(TwitterConsumer.GetUpdates(twitter, AccessToken).CreateReader());
		XPathNavigator nav = updates.CreateNavigator();
		var parsedUpdates = from status in nav.Select("/statuses/status").OfType<XPathNavigator>()
							where !status.SelectSingleNode("user/protected").ValueAsBoolean
							select new {
								User = status.SelectSingleNode("user/name").InnerXml,
								Status = status.SelectSingleNode("text").InnerXml,
							};

		StringBuilder tableBuilder = new StringBuilder();
		tableBuilder.Append("<table><tr><td>Name</td><td>Update</td></tr>");

		foreach (var update in parsedUpdates) {
			tableBuilder.AppendFormat(
				"<tr><td>{0}</td><td>{1}</td></tr>",
				HttpUtility.HtmlEncode(update.User),
				HttpUtility.HtmlEncode(update.Status));
		}
		tableBuilder.Append("</table>");
		resultsPlaceholder.Controls.Add(new Literal { Text = tableBuilder.ToString() });
	}
}
