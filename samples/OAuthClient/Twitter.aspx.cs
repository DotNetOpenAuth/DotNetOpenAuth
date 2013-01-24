namespace OAuthClient {
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
	using DotNetOpenAuth.OAuth;

	using DotNetOpenAuth.Messaging;

	public partial class Twitter : System.Web.UI.Page {
		private string AccessToken {
			get { return (string)Session["TwitterAccessToken"]; }
			set { Session["TwitterAccessToken"] = value; }
		}

		private InMemoryTokenManager TokenManager {
			get {
				var tokenManager = (InMemoryTokenManager)Application["TwitterTokenManager"];
				if (tokenManager == null) {
					string consumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
					string consumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
					if (!string.IsNullOrEmpty(consumerKey)) {
						tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
						Application["TwitterTokenManager"] = tokenManager;
					}
				}

				return tokenManager;
			}
		}

		protected async void Page_Load(object sender, EventArgs e) {
			if (this.TokenManager != null) {
				this.MultiView1.ActiveViewIndex = 1;

				if (!IsPostBack) {
					var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);

					// Is Twitter calling back with authorization?
					var accessTokenResponse = await twitter.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), Response.ClientDisconnectedToken);
					if (accessTokenResponse != null) {
						this.AccessToken = accessTokenResponse.AccessToken;
					} else if (this.AccessToken == null) {
						// If we don't yet have access, immediately request it.
						var request = await twitter.PrepareRequestUserAuthorizationAsync(Response.ClientDisconnectedToken);
						var response = await twitter.Channel.PrepareResponseAsync(request, Response.ClientDisconnectedToken);
						await response.SendAsync(new HttpResponseWrapper(Response), Response.ClientDisconnectedToken);
					}
				}
			}
		}

		protected async void downloadUpdates_Click(object sender, EventArgs e) {
			var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);
			XPathDocument updates = new XPathDocument((await TwitterConsumer.GetUpdatesAsync(twitter, this.AccessToken, Response.ClientDisconnectedToken)).CreateReader());
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
			this.resultsPlaceholder.Controls.Add(new Literal { Text = tableBuilder.ToString() });
		}

		protected async void uploadProfilePhotoButton_Click(object sender, EventArgs e) {
			if (this.profilePhoto.PostedFile.ContentType == null) {
				this.photoUploadedLabel.Visible = true;
				this.photoUploadedLabel.Text = "Select a file first.";
				return;
			}

			var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);
			XDocument imageResult = await TwitterConsumer.UpdateProfileImageAsync(
				twitter,
				this.AccessToken,
				this.profilePhoto.PostedFile.InputStream,
				this.profilePhoto.PostedFile.ContentType,
				Response.ClientDisconnectedToken);
			this.photoUploadedLabel.Visible = true;
		}
	}
}