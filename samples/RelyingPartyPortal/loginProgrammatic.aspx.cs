using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;

public partial class loginProgrammatic : System.Web.UI.Page {
	protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args) {
		// This catches common typos that result in an invalid OpenID Identifier.
		args.IsValid = Identifier.IsValid(args.Value);
	}

	protected void loginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) return; // don't login if custom validation failed.
		OpenIdRelyingParty openid = new OpenIdRelyingParty();
		try {
			IAuthenticationRequest request = openid.CreateRequest(openIdBox.Text);
			// This is where you would add any OpenID extensions you wanted
			// to include in the authentication request.
			// request.AddExtension(someExtensionRequestInstance);

			// Send your visitor to their Provider for authentication.
			request.RedirectToProvider();
		} catch (OpenIdException ex) {
			// The user probably entered an Identifier that 
			// was not a valid OpenID endpoint.
			openidValidator.Text = ex.Message;
			openidValidator.IsValid = false;
		}
	}

	protected void Page_Load(object sender, EventArgs e) {
		openIdBox.Focus();

		OpenIdRelyingParty openid = new OpenIdRelyingParty();
		if (openid.Response != null) {
			switch (openid.Response.Status) {
				case AuthenticationStatus.Authenticated:
					// This is where you would look for any OpenID extension responses included
					// in the authentication assertion.
					// var extension = openid.Response.GetExtension<SomeExtensionResponseType>();

					// Use FormsAuthentication to tell ASP.NET that the user is now logged in,
					// with the OpenID Claimed Identifier as their username.
					FormsAuthentication.RedirectFromLoginPage(openid.Response.ClaimedIdentifier, false);
					break;
				case AuthenticationStatus.Canceled:
					loginCanceledLabel.Visible = true;
					break;
				case AuthenticationStatus.Failed:
					loginFailedLabel.Visible = true;
					break;
				// We don't need to handle SetupRequired because we're not setting
				// IAuthenticationRequest.Mode to immediate mode.
				//case AuthenticationStatus.SetupRequired:
				//    break;
			}
		}
	}
}
