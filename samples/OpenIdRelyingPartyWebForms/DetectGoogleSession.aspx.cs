namespace OpenIdRelyingPartyWebForms {
	using System;
	using DotNetOpenAuth.ApplicationBlock.CustomExtensions;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class DetectGoogleSession : System.Web.UI.Page {
		private const string GoogleOPIdentifier = "https://www.google.com/accounts/o8/id";

		private const string UIModeDetectSession = "x-has-session";

		protected void Page_Load(object sender, EventArgs e) {
			using (var openid = new OpenIdRelyingParty()) {
				// In order to receive the UIRequest as a response, we must register a custom extension factory.
				openid.ExtensionFactories.Add(new UIRequestAtRelyingPartyFactory());

				var response = openid.GetResponse();
				if (response == null) {
					// Submit an OpenID request which Google must reply to immediately.
					// If the user hasn't established a trust relationship with this site yet,
					// Google will not give us the user identity, but they will tell us whether the user
					// at least has an active login session with them so we know whether to promote the
					// "Log in with Google" button.
					IAuthenticationRequest request = openid.CreateRequest("https://www.google.com/accounts/o8/id");
					request.AddExtension(new UIRequest { Mode = UIModeDetectSession });
					request.Mode = AuthenticationRequestMode.Immediate;
					request.RedirectToProvider();
				} else {
					if (response.Status == AuthenticationStatus.Authenticated) {
						this.YouTrustUsLabel.Visible = true;
					} else if (response.Status == AuthenticationStatus.SetupRequired) {
						// Google refused to authenticate the user without user interaction.
						// This is either because Google doesn't know who the user is yet,
						// or because the user hasn't indicated to Google to trust this site.
						// Google uniquely offers the RP a tip as to which of the above situations is true.
						// Figure out which it is.  In a real app, you might use this value to promote a 
						// Google login button on your site if you detect that a Google session exists.
						var ext = response.GetUntrustedExtension<UIRequest>();
						this.YouAreLoggedInLabel.Visible = ext != null && ext.Mode == UIModeDetectSession;
						this.YouAreNotLoggedInLabel.Visible = !this.YouAreLoggedInLabel.Visible;
					}
				}
			}
		}
	}
}