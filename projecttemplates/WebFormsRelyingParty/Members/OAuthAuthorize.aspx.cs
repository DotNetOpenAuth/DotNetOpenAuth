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
	using WebFormsRelyingParty.Code;

	public partial class OAuthAuthorize : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				if (OAuthServiceProvider.PendingAuthorizationRequest == null) {
					Response.Redirect("~/");
				}

				this.csrfCheck.Value = Utilities.SetCsrfCookie();
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(OAuthServiceProvider.PendingAuthorizationConsumer.Name);
			} else {
				Utilities.VerifyCsrfCookie(this.csrfCheck.Value);
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			OAuthServiceProvider.AuthorizePendingRequestToken();
		}

		protected void noButton_Click(object sender, EventArgs e) {
			OAuthServiceProvider.PendingAuthorizationRequest = null;
			Response.Redirect("~/");
		}
	}
}
