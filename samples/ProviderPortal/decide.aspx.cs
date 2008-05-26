using System;
using System.Diagnostics;
using System.Web.Security;
using System.Web.UI;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.Provider;

/// <summary>
/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
/// </summary>
public partial class decide : Page {
	protected void Page_Load(object src, EventArgs e) {
		if (ProviderEndpoint.PendingAuthenticationRequest == null)
			Response.Redirect("~/");

		if (ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
			ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier = Util.BuildIdentityUrl();
		}
		relyingPartyVerificationResultLabel.Text =
			ProviderEndpoint.PendingAuthenticationRequest.IsReturnUrlDiscoverable ? "passed" : "failed";

		identityUrlLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier.ToString();
		realmLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.Realm.ToString();

		// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
		if (User.Identity.Name == Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier)) {
			// if simple registration fields were used, then prompt the user for them
			var requestedFields = ProviderEndpoint.PendingAuthenticationRequest.GetExtension<ClaimsRequest>();
			if (requestedFields != null) {
				this.profileFields.Visible = true;
				this.profileFields.SetRequiredFieldsFromRequest(requestedFields);
				if (!IsPostBack) {
					this.profileFields.OpenIdProfileFields = new ClaimsResponse() {
						Email = Membership.GetUser().Email,
					};
				}
			}
		} else {
			FormsAuthentication.SignOut();
			Response.Redirect(Request.Url.AbsoluteUri);
		}
	}

	protected void Yes_Click(Object sender, EventArgs e) {
		ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
		ProviderEndpoint.PendingAuthenticationRequest.AddResponseExtension(profileFields.OpenIdProfileFields);
		Debug.Assert(ProviderEndpoint.PendingAuthenticationRequest.IsResponseReady);
		ProviderEndpoint.PendingAuthenticationRequest.Response.Send();
		ProviderEndpoint.PendingAuthenticationRequest = null;
	}

	protected void No_Click(Object sender, EventArgs e) {
		ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = false;
		Debug.Assert(ProviderEndpoint.PendingAuthenticationRequest.IsResponseReady);
		ProviderEndpoint.PendingAuthenticationRequest.Response.Send();
		ProviderEndpoint.PendingAuthenticationRequest = null;
	}
}