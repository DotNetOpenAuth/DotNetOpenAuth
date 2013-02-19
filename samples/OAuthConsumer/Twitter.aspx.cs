namespace OAuthConsumer {
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
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;

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
						var message = await twitter.PrepareRequestUserAuthorizationAsync(Response.ClientDisconnectedToken);
						var response = await twitter.Channel.PrepareResponseAsync(message, Response.ClientDisconnectedToken);
						await response.SendAsync();
					}
				}
			}
		}

		protected async void downloadUpdates_Click(object sender, EventArgs e) {
			var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);
			var statusesJson = await TwitterConsumer.GetUpdatesAsync(twitter, this.AccessToken, Response.ClientDisconnectedToken);

			StringBuilder tableBuilder = new StringBuilder();
			tableBuilder.Append("<table><tr><td>Name</td><td>Update</td></tr>");

			foreach (dynamic update in statusesJson) {
				if (!update.user.@protected.Value) {
					tableBuilder.AppendFormat(
						"<tr><td>{0}</td><td>{1}</td></tr>",
						HttpUtility.HtmlEncode(update.user.screen_name),
						HttpUtility.HtmlEncode(update.text));
				}
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