using System;
using System.Web.UI;
using DotNetOpenId.RelyingParty;
using System.Web.Security;
using RelyingPartyCustomStore;

public partial class login : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		openIdBox.Focus();

		OpenIdRelyingParty rp = new OpenIdRelyingParty(CustomStore.Instance, Request.Url);
		if (rp.Response != null) {
			switch (rp.Response.Status) {
				case AuthenticationStatus.Authenticated:
					FormsAuthentication.RedirectFromLoginPage(rp.Response.ClaimedIdentifier, false);
					break;
				case AuthenticationStatus.Canceled:
					loginCanceledLabel.Visible = true;
					break;
				case AuthenticationStatus.Failed:
					loginFailedLabel.Visible = true;
					break;
			}
		}
	}

	protected void loginButton_Click(object sender, EventArgs e) {
		OpenIdRelyingParty rp = new OpenIdRelyingParty(CustomStore.Instance, Request.Url);
		rp.CreateRequest(openIdBox.Text).RedirectToProvider();
	}
}
