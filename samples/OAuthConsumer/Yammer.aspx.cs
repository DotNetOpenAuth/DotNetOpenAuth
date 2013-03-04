namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;

	public partial class Yammer : System.Web.UI.Page {
		private AccessToken AccessToken {
			get { return (AccessToken)Session["YammerAccessToken"]; }
			set { Session["YammerAccessToken"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			var yammer = new YammerConsumer();
			if (yammer.ConsumerKey != null) {
				this.MultiView1.SetActiveView(this.BeginAuthorizationView);
			}
		}

		protected void getYammerMessages_Click(object sender, EventArgs e) {
			var yammer = new YammerConsumer();

			// TODO: code here
		}

		protected void obtainAuthorizationButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var yammer = new YammerConsumer();
						Uri popupWindowLocation = await yammer.RequestUserAuthorizationAsync(MessagingUtilities.GetPublicFacingUrl());
						string javascript = "window.open('" + popupWindowLocation.AbsoluteUri + "');";
						this.Page.ClientScript.RegisterStartupScript(GetType(), "YammerPopup", javascript, true);
						this.MultiView1.SetActiveView(this.CompleteAuthorizationView);
					}));
		}

		protected void finishAuthorizationButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!Page.IsValid) {
							return;
						}

						var yammer = new YammerConsumer();
						var authorizationResponse = await yammer.ProcessUserAuthorizationAsync(this.yammerUserCode.Text);
						if (authorizationResponse != null) {
							this.accessTokenLabel.Text = HttpUtility.HtmlEncode(authorizationResponse.AccessToken);
							this.MultiView1.SetActiveView(this.AuthorizationCompleteView);
						} else {
							this.MultiView1.SetActiveView(this.BeginAuthorizationView);
							this.authorizationErrorLabel.Visible = true;
						}
					}));
		}
	}
}