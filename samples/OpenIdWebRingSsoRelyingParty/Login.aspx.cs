namespace OpenIdWebRingSsoRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class Login : System.Web.UI.Page {
		private static OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();

		protected void Page_Load(object sender, EventArgs e) {
			UriBuilder returnToBuilder = new UriBuilder(Request.Url);
			returnToBuilder.Path = "/login.aspx";
			returnToBuilder.Query = null;
			returnToBuilder.Fragment = null;
			Uri returnTo = returnToBuilder.Uri;
			returnToBuilder.Path = "/";
			Realm realm = returnToBuilder.Uri;

			var response = relyingParty.GetResponse();
			if (response == null) {
				// Because this is a sample of a controlled SSO environment,
				// we don't ask the user which Provider to use... we just send
				// them straight off to the one Provider we trust.
				var request = relyingParty.CreateRequest(
					ConfigurationManager.AppSettings["SsoProvider"],
					realm,
					returnTo);
				request.RedirectToProvider();
			} else {
				switch (response.Status) {
					case AuthenticationStatus.Canceled:
						errorLabel.Text = "Login canceled.";
						break;
					case AuthenticationStatus.Failed:
						errorLabel.Text = HttpUtility.HtmlEncode(response.Exception.Message);
						break;
					case AuthenticationStatus.Authenticated:
						FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, false);
						break;
					default:
						break;
				}
			}
		}

		protected void retryButton_Click(object sender, EventArgs e) {
			Response.Redirect("/login.aspx");
		}
	}
}
