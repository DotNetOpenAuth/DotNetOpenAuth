//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorize.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using RelyingPartyLogic;
	using WebFormsRelyingParty.Code;

	public partial class OAuthAuthorize : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var pendingRequest = OAuthServiceProvider.PendingAuthorizationRequest;
				if (pendingRequest == null) {
					Response.Redirect("AccountInfo.aspx");
				}

				this.csrfCheck.Value = Code.SiteUtilities.SetCsrfCookie();
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(OAuthServiceProvider.PendingAuthorizationConsumer.Name);
				this.OAuth10ConsumerWarning.Visible = pendingRequest.IsUnsafeRequest;

				this.serviceProviderDomainNameLabel.Text = HttpUtility.HtmlEncode(this.Request.Url.Host);
				this.consumerDomainNameLabel3.Text = this.consumerDomainNameLabel2.Text = this.consumerDomainNameLabel1.Text = HttpUtility.HtmlEncode(OAuthServiceProvider.PendingAuthorizationConsumer.Name);
			} else {
				Code.SiteUtilities.VerifyCsrfCookie(this.csrfCheck.Value);
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			this.outerMultiView.SetActiveView(this.authorizationGrantedView);

			var consumer = OAuthServiceProvider.PendingAuthorizationConsumer;
			var tokenManager = OAuthServiceProvider.ServiceProvider.TokenManager;
			var pendingRequest = OAuthServiceProvider.PendingAuthorizationRequest;
			ITokenContainingMessage requestTokenMessage = pendingRequest;
			var requestToken = tokenManager.GetRequestToken(requestTokenMessage.Token);

			OAuthServiceProvider.AuthorizePendingRequestToken();

			// The rest of this method only executes if we couldn't automatically
			// redirect to the consumer.
			if (pendingRequest.IsUnsafeRequest) {
				this.verifierMultiView.SetActiveView(this.noCallbackView);
			} else {
				this.verifierMultiView.SetActiveView(this.verificationCodeView);
				string verifier = ServiceProvider.CreateVerificationCode(consumer.VerificationCodeFormat, consumer.VerificationCodeLength);
				this.verificationCodeLabel.Text = HttpUtility.HtmlEncode(verifier);
				requestToken.VerificationCode = verifier;
				tokenManager.UpdateToken(requestToken);
			}
		}

		protected void noButton_Click(object sender, EventArgs e) {
			this.outerMultiView.SetActiveView(this.authorizationDeniedView);
			OAuthServiceProvider.PendingAuthorizationRequest = null;
		}
	}
}
