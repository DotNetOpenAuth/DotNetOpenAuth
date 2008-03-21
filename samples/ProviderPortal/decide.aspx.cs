using System;
using System.Web.Security;
using System.Web.UI;
using DotNetOpenId.Provider;
using System.Web.Profile;
using System.Diagnostics;
using DotNetOpenId.Extensions;

/// <summary>
/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
/// </summary>
public partial class decide : Page {
	protected void Page_Load(object src, EventArgs e) {
		if (ProviderEndpoint.PendingAuthenticationRequest == null)
			Response.Redirect("~/");

		identityUrlLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.ClaimedIdentifier.ToString();
		realmLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.Realm.ToString();

		// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
		String s = Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.ClaimedIdentifier);
		if (s != User.Identity.Name) {
			FormsAuthentication.SignOut();
			Response.Redirect(Request.Url.AbsoluteUri);
		} else {
			// if simple registration fields were used, then prompt the user for them
			var requestedFields = SimpleRegistrationRequestFields.ReadFromRequest(ProviderEndpoint.PendingAuthenticationRequest);
			if (!requestedFields.Equals(SimpleRegistrationRequestFields.None)) {
				this.profileFields.Visible = true;
				this.profileFields.SetRequiredFieldsFromRequest(requestedFields);
				if (!IsPostBack) {
					this.profileFields.OpenIdProfileFields = new DotNetOpenId.Extensions.SimpleRegistrationFieldValues() {
						Email = Membership.GetUser().Email,
					};
				}
			}
		}
	}

	protected void Yes_Click(Object sender, EventArgs e) {
		ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
		profileFields.OpenIdProfileFields.AddToResponse(ProviderEndpoint.PendingAuthenticationRequest);
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