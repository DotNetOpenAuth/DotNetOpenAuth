using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;

public partial class login : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		OpenIdLogin1.Focus();
	}

	protected void requireSslCheckBox_CheckedChanged(object sender, EventArgs e) {
		this.OpenIdLogin1.RequireSsl = this.requireSslCheckBox.Checked;
	}

	protected void OpenIdLogin1_LoggingIn(object sender, OpenIdEventArgs e) {
		prepareRequest(e.Request);
	}

	/// <summary>
	/// Fired upon login.
	/// Note, that straight after login, forms auth will redirect the user to their original page. So this page may never be rendererd.
	/// </summary>
	protected void OpenIdLogin1_LoggedIn(object sender, OpenIdEventArgs e) {
		State.FriendlyLoginName = e.Response.FriendlyIdentifierForDisplay;
		State.ProfileFields = e.Response.GetExtension<ClaimsResponse>();
		State.PapePolicies = e.Response.GetExtension<PolicyResponse>();
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
		prepareRequest(req);
		req.RedirectToProvider();
		// We don't listen for the response from the provider explicitly
		// because the OpenIdLogin control is already doing that for us.
	}

	private void prepareRequest(IAuthenticationRequest request) {
		// Setup is the default for the login control.  But the user may have checked the box to override that.
		request.Mode = immediateCheckBox.Checked ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;

		// Collect the PAPE policies requested by the user.
		List<string> policies = new List<string>();
		foreach (ListItem item in papePolicies.Items) {
			if (item.Selected) {
				policies.Add(item.Value);
			}
		}

		// Add the PAPE extension if any policy was requested.
		if (policies.Count > 0) {
			var pape = new PolicyRequest();
			foreach (string policy in policies) {
				pape.PreferredPolicies.Add(policy);
			}

			request.AddExtension(pape);
		}
	}
}
