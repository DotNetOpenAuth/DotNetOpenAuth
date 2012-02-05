//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorize.aspx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;
	using RelyingPartyLogic;

	public partial class OAuthAuthorize : System.Web.UI.Page {
		private EndUserAuthorizationRequest pendingRequest;

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				this.pendingRequest = OAuthServiceProvider.AuthorizationServer.ReadAuthorizationRequest();
				if (this.pendingRequest == null) {
					throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
				}

				this.csrfCheck.Value = Code.SiteUtilities.SetCsrfCookie();
				var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == this.pendingRequest.ClientIdentifier);
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(requestingClient.Name);
				this.scopeLabel.Text = HttpUtility.HtmlEncode(OAuthUtilities.JoinScopes(this.pendingRequest.Scope));

				// Consider auto-approving if safe to do so.
				if (((OAuthAuthorizationServer)OAuthServiceProvider.AuthorizationServer.AuthorizationServerServices).CanBeAutoApproved(this.pendingRequest)) {
					OAuthServiceProvider.AuthorizationServer.ApproveAuthorizationRequest(this.pendingRequest, HttpContext.Current.User.Identity.Name);
				}
				this.ViewState["AuthRequest"] = this.pendingRequest;
			} else {
				Code.SiteUtilities.VerifyCsrfCookie(this.csrfCheck.Value);
				this.pendingRequest = (EndUserAuthorizationRequest)this.ViewState["AuthRequest"];
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == this.pendingRequest.ClientIdentifier);
			Database.LoggedInUser.ClientAuthorizations.Add(
				new ClientAuthorization {
					Client = requestingClient,
					Scope = OAuthUtilities.JoinScopes(this.pendingRequest.Scope),
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
