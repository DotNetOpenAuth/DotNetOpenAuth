namespace OAuthServiceProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using OAuthServiceProvider.Code;

	/// <summary>
	/// Conducts the user through a Consumer authorization process.
	/// </summary>
	public partial class Authorize : System.Web.UI.Page {
		private static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		private string AuthorizationSecret {
			get { return Session["OAuthAuthorizationSecret"] as string; }
			set { Session["OAuthAuthorizationSecret"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				if (Global.PendingOAuthAuthorization == null) {
					Response.Redirect("~/Members/AuthorizedConsumers.aspx");
				} else {
					ITokenContainingMessage pendingToken = Global.PendingOAuthAuthorization;
					var token = Global.DataContext.OAuthTokens.Single(t => t.Token == pendingToken.Token);
					this.desiredAccessLabel.Text = token.Scope;
					this.consumerLabel.Text = Global.TokenManager.GetConsumerForToken(token.Token).ConsumerKey;

					// Generate an unpredictable secret that goes to the user agent and must come back
					// with authorization to guarantee the user interacted with this page rather than
					// being scripted by an evil Consumer.
					byte[] randomData = new byte[8];
					CryptoRandomDataGenerator.GetBytes(randomData);
					this.AuthorizationSecret = Convert.ToBase64String(randomData);
					this.OAuthAuthorizationSecToken.Value = this.AuthorizationSecret;

					this.OAuth10ConsumerWarning.Visible = Global.PendingOAuthAuthorization.IsUnsafeRequest;
				}
			}
		}

		protected void allowAccessButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (this.AuthorizationSecret != this.OAuthAuthorizationSecToken.Value) {
							throw new ArgumentException(); // probably someone trying to hack in.
						}
						this.AuthorizationSecret = null; // clear one time use secret
						var pending = Global.PendingOAuthAuthorization;
						Global.AuthorizePendingRequestToken();
						this.multiView.ActiveViewIndex = 1;

						ServiceProvider sp = new ServiceProvider(Constants.SelfDescription, Global.TokenManager);
						var response = sp.PrepareAuthorizationResponse(pending);
						if (response != null) {
							var responseMessage = await sp.Channel.PrepareResponseAsync(response, Response.ClientDisconnectedToken);
							await responseMessage.SendAsync();
						} else {
							if (pending.IsUnsafeRequest) {
								this.verifierMultiView.ActiveViewIndex = 1;
							} else {
								string verifier = ServiceProvider.CreateVerificationCode(VerificationCodeFormat.AlphaNumericNoLookAlikes, 10);
								this.verificationCodeLabel.Text = verifier;
								ITokenContainingMessage requestTokenMessage = pending;
								var requestToken = Global.TokenManager.GetRequestToken(requestTokenMessage.Token);
								requestToken.VerificationCode = verifier;
								Global.TokenManager.UpdateToken(requestToken);
							}
						}
					}));
		}

		protected void denyAccessButton_Click(object sender, EventArgs e) {
			// erase the request token.
			this.multiView.ActiveViewIndex = 2;
		}
	}
}