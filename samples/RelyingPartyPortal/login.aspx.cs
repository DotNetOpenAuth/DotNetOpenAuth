using System;
using System.Web.UI;
using DotNetOpenId.RelyingParty;

public partial class login : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		OpenIdLogin1.Focus();
		OpenIdLogin1.ImmediateMode = immediateCheckBox.Checked;
	}

	/// <summary>
	/// Fired upon login.
	/// Note, that straight after login, forms auth will redirect the user to their original page. So this page may never be rendererd.
	/// </summary>
	protected void OpenIdLogin1_LoggedIn(object sender, OpenIdEventArgs e) {
		State.ProfileFields = e.ProfileFields;
	}
	protected void OpenIdLogin1_Failed(object sender, OpenIdEventArgs e) {
		loginFailedLabel.Visible = true;
		loginFailedLabel.Text += ": " + e.Response.Exception.Message;
	}
	protected void OpenIdLogin1_Canceled(object sender, OpenIdEventArgs e) {
		loginCanceledLabel.Visible = true;
	}
	protected void OpenIdLogin1_SetupRequired(object sender, OpenIdEventArgs e) {
		loginFailedLabel.Text = "You must log into your Provider first to use Immediate mode.";
		loginFailedLabel.Visible = true;
	}

	protected void yahooLoginButton_Click(object sender, ImageClickEventArgs e) {
		OpenIdRelyingParty openid = new OpenIdRelyingParty();
		var req = openid.CreateRequest("yahoo.com");
		req.RedirectToProvider();
		// We don't listen for the response from the provider explicitly
		// because the OpenIdLogin control is already doing that for us.
	}
}
