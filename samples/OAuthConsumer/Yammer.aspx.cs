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
		private string RequestToken {
			get { return (string)ViewState["YammerRequestToken"]; }
			set { ViewState["YammerRequestToken"] = value; }
		}

		private string AccessToken {
			get { return (string)Session["YammerAccessToken"]; }
			set { Session["YammerAccessToken"] = value; }
		}

		private InMemoryTokenManager TokenManager {
			get {
				var tokenManager = (InMemoryTokenManager)Application["YammerTokenManager"];
				if (tokenManager == null) {
					string consumerKey = ConfigurationManager.AppSettings["YammerConsumerKey"];
					string consumerSecret = ConfigurationManager.AppSettings["YammerConsumerSecret"];
					if (!string.IsNullOrEmpty(consumerKey)) {
						tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
						Application["YammerTokenManager"] = tokenManager;
					}
				}

				return tokenManager;
			}
		}

		protected void Page_Load(object sender, EventArgs e) {
			if (this.TokenManager != null) {
				this.MultiView1.SetActiveView(this.BeginAuthorizationView);
			}
		}

		protected void getYammerMessages_Click(object sender, EventArgs e) {
			var yammer = new WebConsumer(YammerConsumer.ServiceDescription, this.TokenManager);
		}

		protected void obtainAuthorizationButton_Click(object sender, EventArgs e) {
			var yammer = YammerConsumer.CreateConsumer(this.TokenManager);
			string requestToken;
			Uri popupWindowLocation = YammerConsumer.PrepareRequestAuthorization(yammer, out requestToken);
			this.RequestToken = requestToken;
			string javascript = "window.open('" + popupWindowLocation.AbsoluteUri + "');";
			this.Page.ClientScript.RegisterStartupScript(GetType(), "YammerPopup", javascript, true);
			this.MultiView1.SetActiveView(this.CompleteAuthorizationView);
		}

		protected void finishAuthorizationButton_Click(object sender, EventArgs e) {
			if (!Page.IsValid) {
				return;
			}

			var yammer = YammerConsumer.CreateConsumer(this.TokenManager);
			var authorizationResponse = YammerConsumer.CompleteAuthorization(yammer, this.RequestToken, this.yammerUserCode.Text);
			if (authorizationResponse != null) {
				this.accessTokenLabel.Text = HttpUtility.HtmlEncode(authorizationResponse.AccessToken);
				this.MultiView1.SetActiveView(this.AuthorizationCompleteView);
			} else {
				this.MultiView1.SetActiveView(this.BeginAuthorizationView);
				this.authorizationErrorLabel.Visible = true;
			}
		}
	}
}