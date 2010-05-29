namespace OAuthServiceProvider.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Security.Cryptography;
	using Code;

	public partial class Authorize2 : System.Web.UI.Page {
		private static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		private string AuthorizationSecret {
			get { return Session["OAuthAuthorizationSecret"] as string; }
			set { Session["OAuthAuthorizationSecret"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				if (Global.PendingOAuth2Authorization == null) {
					Response.Redirect("~/Members/AuthorizedConsumers.aspx");
				} else {
					var pendingRequest = Global.PendingOAuth2Authorization;
					this.desiredAccessLabel.Text = pendingRequest.Scope;
					this.consumerLabel.Text = pendingRequest.ClientIdentifier;

					// Generate an unpredictable secret that goes to the user agent and must come back
					// with authorization to guarantee the user interacted with this page rather than
					// being scripted by an evil Consumer.
					var randomData = new byte[8];
					CryptoRandomDataGenerator.GetBytes(randomData);
					this.AuthorizationSecret = Convert.ToBase64String(randomData);
					this.OAuthAuthorizationSecToken.Value = this.AuthorizationSecret;
				}
			}
		}

		protected void allowAccessButton_Click(object sender, EventArgs e) {
			if (this.AuthorizationSecret != this.OAuthAuthorizationSecToken.Value) {
				throw new ArgumentException(); // probably someone trying to hack in.
			}
			this.AuthorizationSecret = null; // clear one time use secret
			this.multiView.SetActiveView(this.AuthGranted);

			Global.AuthorizationServer.ApproveAuthorizationRequest(Global.PendingOAuth2Authorization);
		}

		protected void denyAccessButton_Click(object sender, EventArgs e) {
			Global.AuthorizationServer.RejectAuthorizationRequest(Global.PendingOAuth2Authorization);
		}
	}
}