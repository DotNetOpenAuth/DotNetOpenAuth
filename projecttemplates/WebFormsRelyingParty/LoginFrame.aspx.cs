namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IdentityModel.Claims;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.InfoCard;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;

	public partial class LoginFrame : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				// Because this page can appear as an iframe in a popup of another page,
				// we need to record which page the hosting page is in order to redirect back
				// to it after login is complete.
				this.ClientScript.RegisterOnSubmitStatement(
					this.GetType(),
					"getTopWindowUrl",
					"document.getElementById('topWindowUrl').value = window.parent.location.href;");
			}
		}

		protected void openIdSelector_LoggedIn(object sender, OpenIdEventArgs e) {
			bool trustedEmail = Policies.ProviderEndpointsProvidingTrustedEmails.Contains(e.Response.Provider.Uri);
			this.LoginUser(e.ClaimedIdentifier, e.Response.FriendlyIdentifierForDisplay, e.Response.GetExtension<ClaimsResponse>(), null, trustedEmail);
		}

		protected void openIdSelector_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
			bool trustedEmail = false; // we don't trust InfoCard email addresses, since these can be self-issued.
			this.LoginUser(AuthenticationToken.SynthesizeClaimedIdentifierFromInfoCard(e.Token.UniqueId), e.Token.SiteSpecificId, null, e.Token, trustedEmail);
		}

		protected void openIdSelector_Failed(object sender, OpenIdEventArgs e) {
			if (e.Response.Exception != null) {
				this.errorMessageLabel.Text = HttpUtility.HtmlEncode(e.Response.Exception.ToStringDescriptive());
			}
			this.errorPanel.Visible = true;
		}

		protected void openIdSelector_TokenProcessingError(object sender, TokenProcessingErrorEventArgs e) {
			this.errorMessageLabel.Text = HttpUtility.HtmlEncode(e.Exception.ToStringDescriptive());
			this.errorPanel.Visible = true;
		}

		private void LoginUser(string claimedIdentifier, string friendlyIdentifier, ClaimsResponse claims, Token samlToken, bool trustedEmail) {
			// Create an account for this user if we don't already have one.
			AuthenticationToken openidToken = Database.DataContext.AuthenticationTokens.FirstOrDefault(token => token.ClaimedIdentifier == claimedIdentifier);
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
						user.EmailAddressVerified = trustedEmail;
					}
					if (!string.IsNullOrEmpty(claims.FullName)) {
						if (claims.FullName.IndexOf(' ') > 0) {
							user.FirstName = claims.FullName.Substring(0, claims.FullName.IndexOf(' ')).Trim();
							user.LastName = claims.FullName.Substring(claims.FullName.IndexOf(' ')).Trim();
						} else {
							user.FirstName = claims.FullName;
						}
					}
				} else if (samlToken != null) {
					string email, givenName, surname;
					if (samlToken.Claims.TryGetValue(ClaimTypes.Email, out email)) {
						user.EmailAddress = email;
						user.EmailAddressVerified = trustedEmail;
					}
					if (samlToken.Claims.TryGetValue(ClaimTypes.GivenName, out givenName)) {
						user.FirstName = givenName;
					}
					if (samlToken.Claims.TryGetValue(ClaimTypes.Surname, out surname)) {
						user.LastName = surname;
					}
				}

				Database.DataContext.AddToUsers(user);
			} else {
				openidToken.UsageCount++;
				openidToken.LastUsedUtc = DateTime.UtcNow;
			}

			bool persistentCookie = false;
			if (string.IsNullOrEmpty(this.Request.QueryString["ReturnUrl"])) {
				FormsAuthentication.SetAuthCookie(openidToken.ClaimedIdentifier, persistentCookie);
				if (!string.IsNullOrEmpty(this.topWindowUrl.Value)) {
					Uri topWindowUri = new Uri(this.topWindowUrl.Value);
					string returnUrl = HttpUtility.ParseQueryString(topWindowUri.Query)["ReturnUrl"];
					if (string.IsNullOrEmpty(returnUrl)) {
						if (string.Equals(topWindowUri.AbsolutePath, Utilities.ApplicationRoot.AbsolutePath + "login.aspx", StringComparison.OrdinalIgnoreCase)) {
							// this happens when the user navigates deliberately directly to login.aspx
							Response.Redirect("~/");
						} else {
							Response.Redirect(this.topWindowUrl.Value);
						}
					} else {
						Response.Redirect(returnUrl);
					}
				} else {
					// This happens for unsolicited assertions.
					Response.Redirect("~/");
				}
			} else {
				FormsAuthentication.RedirectFromLoginPage(openidToken.ClaimedIdentifier, persistentCookie);
			}
		}
	}
}
