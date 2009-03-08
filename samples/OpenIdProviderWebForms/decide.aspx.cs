namespace OpenIdProviderWebForms {
	using System;
	using System.Diagnostics;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
	/// </summary>
	public partial class decide : Page {
		protected void Page_Load(object src, EventArgs e) {
			if (ProviderEndpoint.PendingAuthenticationRequest == null) {
				Response.Redirect("~/");
			}

			if (ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
				ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier = Code.Util.BuildIdentityUrl();
			}
			this.relyingPartyVerificationResultLabel.Text =
				ProviderEndpoint.PendingAuthenticationRequest.IsReturnUrlDiscoverable(ProviderEndpoint.Provider.Channel.WebRequestHandler) ? "passed" : "failed";

			this.identityUrlLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier.ToString();
			this.realmLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.Realm.ToString();

			// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
			if (string.Equals(User.Identity.Name, Code.Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier), StringComparison.OrdinalIgnoreCase)) {
				// if simple registration fields were used, then prompt the user for them
				var requestedFields = ProviderEndpoint.PendingAuthenticationRequest.GetExtension<ClaimsRequest>();
				if (requestedFields != null) {
					this.profileFields.Visible = true;
					this.profileFields.SetRequiredFieldsFromRequest(requestedFields);
					if (!IsPostBack) {
						var sregResponse = requestedFields.CreateResponse();
						sregResponse.Email = Membership.GetUser().Email;
						this.profileFields.SetOpenIdProfileFields(sregResponse);
					}
				}
			} else {
				FormsAuthentication.SignOut();
				Response.Redirect(Request.Url.AbsoluteUri);
			}
		}

		protected void Yes_Click(object sender, EventArgs e) {
			var sregRequest = ProviderEndpoint.PendingAuthenticationRequest.GetExtension<ClaimsRequest>();
			ClaimsResponse sregResponse = null;
			if (sregRequest != null) {
				sregResponse = this.profileFields.GetOpenIdProfileFields(sregRequest);
				ProviderEndpoint.PendingAuthenticationRequest.AddResponseExtension(sregResponse);
			}
			var papeRequest = ProviderEndpoint.PendingAuthenticationRequest.GetExtension<PolicyRequest>();
			PolicyResponse papeResponse = null;
			if (papeRequest != null) {
				papeResponse = new PolicyResponse();
				papeResponse.NistAssuranceLevel = NistAssuranceLevel.InsufficientForLevel1;
				ProviderEndpoint.PendingAuthenticationRequest.AddResponseExtension(papeResponse);
			}

			ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
			Debug.Assert(ProviderEndpoint.PendingAuthenticationRequest.IsResponseReady, "Setting authentication should be all that's necessary.");
			ProviderEndpoint.SendResponse();
		}

		protected void No_Click(object sender, EventArgs e) {
			ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = false;
			Debug.Assert(ProviderEndpoint.PendingAuthenticationRequest.IsResponseReady, "Setting authentication should be all that's necessary.");
			ProviderEndpoint.SendResponse();
		}
	}
}