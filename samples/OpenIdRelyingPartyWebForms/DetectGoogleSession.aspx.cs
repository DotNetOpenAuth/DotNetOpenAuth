namespace OpenIdRelyingPartyWebForms {
	using System;
	using DotNetOpenAuth.ApplicationBlock.CustomExtensions;
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
					// Now see if the UIRequest was mirrored back to us.
					var ext = response.GetUntrustedExtension<UIRequest>();
					this.YouAreLoggedInLabel.Visible = ext != null && ext.Mode == UIModeDetectSession;
					this.YouAreNotLoggedInLabel.Visible = !this.YouAreLoggedInLabel.Visible;
				}
			}
		}
	}
}