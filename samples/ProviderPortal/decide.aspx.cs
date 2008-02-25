using System;
using System.Web.Security;
using System.Web.UI;
using DotNetOpenId.Provider;
using System.Web.Profile;

/// <summary>
/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
/// </summary>
public partial class decide : Page {
	protected void Page_Load(object src, EventArgs e) {
		State.Session.CheckExpectedStateIsAvailable();

		// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
		String s = Util.ExtractUserName(State.Session.LastRequest.IdentityUrl);
		if (s != User.Identity.Name) {
			FormsAuthentication.SignOut();
			Response.Redirect(Request.Url.AbsoluteUri);
		} else {
			// if simple registration fields were used, then prompt the user for them
			if (State.Session.LastRequest.RequestedProfileFields.AnyRequestedOrRequired) {
				this.profileFields.Visible = true;
				this.profileFields.SetRequiredFieldsFromRequest(State.Session.LastRequest.RequestedProfileFields);
				if (!IsPostBack) {
					this.profileFields.OpenIdProfileFields = new DotNetOpenId.RegistrationExtension.ProfileFieldValues() {
						Email = Membership.GetUser().Email,
					};
				}
			}
		}
	}

	protected void Yes_Click(Object sender, EventArgs e) {
		AuthenticationResponse response;
		if (this.profileFields.DoesAnyFieldHaveAValue) {
			// authenticate with simple registration fields
			response = State.Session.LastRequest.Answer(true, Util.ServerUri, this.profileFields.OpenIdProfileFields);
		} else {
			// no fields available
			response = State.Session.LastRequest.Answer(true, Util.ServerUri);
		}
		Util.GenerateHttpResponse(response);
	}

	protected void No_Click(Object sender, EventArgs e) {
		Util.GenerateHttpResponse(State.Session.LastRequest.Answer(false, Util.ServerUri));
	}
}