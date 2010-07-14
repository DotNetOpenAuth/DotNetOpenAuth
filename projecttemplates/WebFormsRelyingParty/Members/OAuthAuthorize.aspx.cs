//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorize.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OAuth2.Messages;

	using RelyingPartyLogic;

	public partial class OAuthAuthorize : System.Web.UI.Page {
		private EndUserAuthorizationRequest pendingRequest;

		protected void Page_Load(object sender, EventArgs e) {
			// We'll mask that on postback it's a POST when looking up the authorization details so that the GET-only
			// message can be picked up.
			var requestInfo = this.IsPostBack
			                  	? new HttpRequestInfo("GET", this.Request.Url, this.Request.RawUrl, new WebHeaderCollection(), null)
			                  	: null;
			this.pendingRequest = OAuthServiceProvider.AuthorizationServer.ReadAuthorizationRequest(requestInfo);
			if (this.pendingRequest == null) {
				Response.Redirect("AccountInfo.aspx");
			}

			if (!IsPostBack) {
				this.csrfCheck.Value = Code.SiteUtilities.SetCsrfCookie();
				var requestingClient = Database.DataContext.Consumers.First(c => c.ConsumerKey == this.pendingRequest.ClientIdentifier);
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(requestingClient.Name);
			} else {
				Code.SiteUtilities.VerifyCsrfCookie(this.csrfCheck.Value);
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			this.outerMultiView.SetActiveView(this.authorizationGrantedView);

			// In this case the resource server and the auth server are the same, so just use the same key.
			var resourceServerPublicKey = OAuthServiceProvider.AuthorizationServer.AuthorizationServer.AccessTokenSigningPrivateKey;
			OAuthServiceProvider.AuthorizationServer.ApproveAuthorizationRequest(this.pendingRequest, HttpContext.Current.User.Identity.Name, resourceServerPublicKey);
		}

		protected void noButton_Click(object sender, EventArgs e) {
			this.outerMultiView.SetActiveView(this.authorizationDeniedView);
			OAuthServiceProvider.AuthorizationServer.RejectAuthorizationRequest(this.pendingRequest);
		}
	}
}
