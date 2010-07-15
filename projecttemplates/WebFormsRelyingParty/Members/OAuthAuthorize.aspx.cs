//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorize.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Members {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
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
				var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == this.pendingRequest.ClientIdentifier);
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(requestingClient.Name);
				this.scopeLabel.Text = HttpUtility.HtmlEncode(this.pendingRequest.Scope);

				// Consider auto-approving if safe to do so.
				if (((OAuthAuthorizationServer)OAuthServiceProvider.AuthorizationServer.AuthorizationServer).CanBeAutoApproved(this.pendingRequest)) {
					OAuthServiceProvider.AuthorizationServer.ApproveAuthorizationRequest(this.pendingRequest, HttpContext.Current.User.Identity.Name);
				}
			} else {
				Code.SiteUtilities.VerifyCsrfCookie(this.csrfCheck.Value);
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == this.pendingRequest.ClientIdentifier);
			Database.LoggedInUser.ClientAuthorizations.Add(
				new ClientAuthorization {
					Client = requestingClient,
					Scope = this.pendingRequest.Scope,
					User = Database.LoggedInUser,
					CreatedOnUtc = DateTime.UtcNow.CutToSecond(),
				});
			OAuthServiceProvider.AuthorizationServer.ApproveAuthorizationRequest(this.pendingRequest, HttpContext.Current.User.Identity.Name);
		}

		protected void noButton_Click(object sender, EventArgs e) {
			OAuthServiceProvider.AuthorizationServer.RejectAuthorizationRequest(this.pendingRequest);
		}
	}
}
