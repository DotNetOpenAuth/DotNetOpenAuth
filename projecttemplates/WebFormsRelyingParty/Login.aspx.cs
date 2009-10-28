//-----------------------------------------------------------------------
// <copyright file="Login.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class Login : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.openIdLogin1.Focus();
		}

		protected void openIdLogin1_LoggedIn(object sender, OpenIdEventArgs e) {
			this.LoginUser(e.ClaimedIdentifier, e.Response.FriendlyIdentifierForDisplay, e.Response.GetExtension<ClaimsResponse>());
			e.Cancel = true; // we already logged the user in
		}

		protected void InfoCardSelector1_ReceivedToken(object sender, DotNetOpenAuth.InfoCard.ReceivedTokenEventArgs e) {
			this.LoginUser(AuthenticationToken.SynthesizeClaimedIdentifierFromInfoCard(e.Token.UniqueId), e.Token.SiteSpecificId, null);
		}

		private void LoginUser(string claimedIdentifier, string friendlyIdentifier, ClaimsResponse claims) {
			// Create an account for this user if we don't already have one.
			AuthenticationToken openidToken = Global.DataContext.AuthenticationToken.FirstOrDefault(token => token.ClaimedIdentifier == claimedIdentifier);
			if (openidToken == null) {
				// this is a user we haven't seen before.
				User user = new User();
				openidToken = new AuthenticationToken {
					ClaimedIdentifier = claimedIdentifier,
					FriendlyIdentifier = friendlyIdentifier,
				};
				user.AuthenticationTokens.Add(openidToken);

				// Gather information about the user if it's available.
				if (claims != null) {
					if (!string.IsNullOrEmpty(claims.Email)) {
						user.EmailAddress = claims.Email;
					}
					if (!string.IsNullOrEmpty(claims.FullName)) {
						if (claims.FullName.IndexOf(' ') > 0) {
							user.FirstName = claims.FullName.Substring(0, claims.FullName.IndexOf(' ')).Trim();
							user.LastName = claims.FullName.Substring(claims.FullName.IndexOf(' ')).Trim();
						} else {
							user.FirstName = claims.FullName;
						}
					}
				}

				Global.DataContext.AddToUser(user);
			}

			FormsAuthentication.RedirectFromLoginPage(openidToken.ClaimedIdentifier, false);
		}
	}
}
