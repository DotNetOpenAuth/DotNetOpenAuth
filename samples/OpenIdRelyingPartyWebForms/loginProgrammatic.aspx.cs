namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Net;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class loginProgrammatic : System.Web.UI.Page {
		protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args) {
			// This catches common typos that result in an invalid OpenID Identifier.
			args.IsValid = Identifier.IsValid(args.Value);
		}

		protected void loginButton_Click(object sender, EventArgs e) {
			if (!this.Page.IsValid) {
				return; // don't login if custom validation failed.
			}
			try {
				using (OpenIdRelyingParty openid = this.createRelyingParty()) {
					IAuthenticationRequest request = openid.CreateRequest(this.openIdBox.Text);

					// This is where you would add any OpenID extensions you wanted
					// to include in the authentication request.
					request.AddExtension(new ClaimsRequest {
						Country = DemandLevel.Request,
						Email = DemandLevel.Request,
						Gender = DemandLevel.Require,
						PostalCode = DemandLevel.Require,
						TimeZone = DemandLevel.Require,
					});

					// Send your visitor to their Provider for authentication.
					request.RedirectToProvider();
				}
			} catch (ProtocolException ex) {
				// The user probably entered an Identifier that 
				// was not a valid OpenID endpoint.
				this.openidValidator.Text = ex.Message;
				this.openidValidator.IsValid = false;
			}
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.openIdBox.Focus();

			// For debugging/testing, we allow remote clearing of all associations...
			// NOT a good idea on a production site.
			if (Request.QueryString["clearAssociations"] == "1") {
				Application.Remove("DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty.ApplicationStore");

				// Force a redirect now to prevent the user from logging in while associations
				// are constantly being cleared.
				UriBuilder builder = new UriBuilder(Request.Url);
				builder.Query = null;
				Response.Redirect(builder.Uri.AbsoluteUri);
			}

			OpenIdRelyingParty openid = this.createRelyingParty();
			var response = openid.GetResponse();
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						// This is where you would look for any OpenID extension responses included
						// in the authentication assertion.
						var claimsResponse = response.GetExtension<ClaimsResponse>();
						State.ProfileFields = claimsResponse;

						// Store off the "friendly" username to display -- NOT for username lookup
						State.FriendlyLoginName = response.FriendlyIdentifierForDisplay;

						// Use FormsAuthentication to tell ASP.NET that the user is now logged in,
						// with the OpenID Claimed Identifier as their username.
						FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, false);
						break;
					case AuthenticationStatus.Canceled:
						this.loginCanceledLabel.Visible = true;
						break;
					case AuthenticationStatus.Failed:
						this.loginFailedLabel.Visible = true;
						break;

					// We don't need to handle SetupRequired because we're not setting
					// IAuthenticationRequest.Mode to immediate mode.
					////case AuthenticationStatus.SetupRequired:
					////    break;
				}
			}
		}

		private OpenIdRelyingParty createRelyingParty() {
			OpenIdRelyingParty openid = new OpenIdRelyingParty();
			int minsha, maxsha, minversion;
			if (int.TryParse(Request.QueryString["minsha"], out minsha)) {
				openid.SecuritySettings.MinimumHashBitLength = minsha;
			}
			if (int.TryParse(Request.QueryString["maxsha"], out maxsha)) {
				openid.SecuritySettings.MaximumHashBitLength = maxsha;
			}
			if (int.TryParse(Request.QueryString["minversion"], out minversion)) {
				switch (minversion) {
					case 1: openid.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V10; break;
					case 2: openid.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20; break;
					default: throw new ArgumentOutOfRangeException("minversion");
				}
			}
			return openid;
		}
	}
}