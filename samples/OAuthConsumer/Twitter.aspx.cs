namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
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
		private AccessToken AccessToken {
			get { return (AccessToken)(Session["TwitterAccessToken"] ?? new AccessToken()); }
			set { Session["TwitterAccessToken"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var twitter = new TwitterConsumer();
						if (twitter.ConsumerKey != null) {
							this.MultiView1.ActiveViewIndex = 1;

							if (!IsPostBack) {
								// Is Twitter calling back with authorization?
								var accessTokenResponse = await twitter.ProcessUserAuthorizationAsync(this.Request.Url);
								if (accessTokenResponse != null) {
									this.AccessToken = accessTokenResponse.AccessToken;
								} else {
									// If we don't yet have access, immediately request it.
									Uri redirectUri = await twitter.RequestUserAuthorizationAsync(MessagingUtilities.GetPublicFacingUrl());
									this.Response.Redirect(redirectUri.AbsoluteUri);
								}
							}
						}
					}));
		}

		protected void downloadUpdates_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var twitter = new TwitterConsumer();
						var statusesJson = await twitter.GetUpdatesAsync(this.AccessToken);

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
					}));
		}

		protected void uploadProfilePhotoButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (this.profilePhoto.PostedFile.ContentType == null) {
							this.photoUploadedLabel.Visible = true;
							this.photoUploadedLabel.Text = "Select a file first.";
							return;
						}

						var twitter = new TwitterConsumer();
						XDocument imageResult =
							await
							twitter.UpdateProfileImageAsync(
								this.AccessToken, this.profilePhoto.PostedFile.InputStream, this.profilePhoto.PostedFile.ContentType);
						this.photoUploadedLabel.Visible = true;
					}));
		}
	}
}