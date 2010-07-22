namespace OAuthServiceProvider.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using Code;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;

	public partial class Authorize2 : System.Web.UI.Page {
		private static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		private string AuthorizationSecret {
			get { return Session["OAuthAuthorizationSecret"] as string; }
			set { Session["OAuthAuthorizationSecret"] = value; }
		}

		private EndUserAuthorizationRequest pendingRequest;

		private Client client;

		protected void Page_Load(object sender, EventArgs e) {
			var getRequest = new HttpRequestInfo("GET", this.Request.Url, this.Request.RawUrl, new WebHeaderCollection(), null);
			pendingRequest = Global.AuthorizationServer.ReadAuthorizationRequest(getRequest);
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			client = Global.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			var authServer = new OAuth2AuthorizationServer();
			if (authServer.CanBeAutoApproved(pendingRequest)) {
				Global.AuthorizationServer.ApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
			}

			if (!IsPostBack) {
				this.desiredAccessLabel.Text = OAuthUtilities.JoinScopes(pendingRequest.Scope);
				this.consumerLabel.Text = client.Name;

				// Generate an unpredictable secret that goes to the user agent and must come back
				// with authorization to guarantee the user interacted with this page rather than
				// being scripted by an evil Consumer.
				var randomData = new byte[8];
				CryptoRandomDataGenerator.GetBytes(randomData);
				this.AuthorizationSecret = Convert.ToBase64String(randomData);
				this.OAuthAuthorizationSecToken.Value = this.AuthorizationSecret;
			}
		}

		protected void allowAccessButton_Click(object sender, EventArgs e) {
			if (this.AuthorizationSecret != this.OAuthAuthorizationSecToken.Value) {
				throw new ArgumentException(); // probably someone trying to hack in.
			}
			this.AuthorizationSecret = null; // clear one time use secret
			this.multiView.SetActiveView(this.AuthGranted);

			client.ClientAuthorizations.Add(
				new ClientAuthorization {
					Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
					User = Global.LoggedInUser,
					CreatedOnUtc = DateTime.UtcNow,
				});
			Global.AuthorizationServer.ApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
		}

		protected void denyAccessButton_Click(object sender, EventArgs e) {
			Global.AuthorizationServer.RejectAuthorizationRequest(pendingRequest);
		}
	}
}