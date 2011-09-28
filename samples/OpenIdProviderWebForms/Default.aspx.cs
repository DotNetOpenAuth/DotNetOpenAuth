namespace OpenIdProviderWebForms {
	using System;
	using System.Web.Security;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderWebForms.Code;

	/// <summary>
	/// Page for handling logins to this server. 
	/// </summary>
	public partial class _default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (Request.QueryString["rp"] != null) {
				if (Page.User.Identity.IsAuthenticated) {
					this.SendAssertion(Request.QueryString["rp"]);
				} else {
					FormsAuthentication.RedirectToLoginPage();
				}
			} else {
				TextBox relyingPartySite = (TextBox)this.loginView.FindControl("relyingPartySite");
				if (relyingPartySite != null) {
					relyingPartySite.Focus();
				}
			}
		}

		protected void sendAssertionButton_Click(object sender, EventArgs e) {
			TextBox relyingPartySite = (TextBox)this.loginView.FindControl("relyingPartySite");
			this.SendAssertion(relyingPartySite.Text);
		}

		private void SendAssertion(string relyingPartyRealm) {
			Uri providerEndpoint = new Uri(Request.Url, Page.ResolveUrl("~/server.aspx"));
			OpenIdProvider op = new OpenIdProvider();
			try {
				// Send user input through identifier parser so we accept more free-form input.
				string rpSite = Identifier.Parse(relyingPartyRealm);
				op.PrepareUnsolicitedAssertion(providerEndpoint, rpSite, Util.BuildIdentityUrl(), Util.BuildIdentityUrl()).Send();
			} catch (ProtocolException ex) {
				Label errorLabel = (Label)this.loginView.FindControl("errorLabel");
				errorLabel.Visible = true;
				errorLabel.Text = ex.Message;
			}
		}
	}
}