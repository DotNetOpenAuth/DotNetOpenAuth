using System;
using System.Web.Security;
using System.Web.UI;
using DotNetOpenId.Provider;
using System.Web.Profile;
using System.Diagnostics;
using DotNetOpenId.RegistrationExtension;

/// <summary>
/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
/// </summary>
public partial class decide : Page {
	protected void Page_Load(object src, EventArgs e) {
		if (ProviderEndpoint.PendingAuthenticationRequest == null)
			Response.Redirect("~/");

		identityUrlLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.IdentityUrl.AbsoluteUri;
		trustRootLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.TrustRoot.Url;

		// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
		String s = Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.IdentityUrl);
		if (s != User.Identity.Name) {
			FormsAuthentication.SignOut();
			Response.Redirect(Request.Url.AbsoluteUri);
		} else {
			// if simple registration fields were used, then prompt the user for them
			var requestedFields = ProfileRequestFields.ReadFromRequest(ProviderEndpoint.PendingAuthenticationRequest);
			if (!requestedFields.Equals(ProfileRequestFields.None)) {
				this.profileFields.Visible = true;
				this.profileFields.SetRequiredFieldsFromRequest(requestedFields);
				if (!IsPostBack) {
					this.profileFields.OpenIdProfileFields = new DotNetOpenId.RegistrationExtension.ProfileFieldValues() {
						Email = Membership.GetUser().Email,
					};
				}
			}
		}
	}

	protected void Yes_Click(Object sender, EventArgs e) {
		ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
		profileFields.OpenIdProfileFields.SendWithAuthenticationResponse(ProviderEndpoint.PendingAuthenticationRequest);
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