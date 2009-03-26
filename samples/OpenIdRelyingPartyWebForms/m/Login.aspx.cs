namespace OpenIdRelyingPartyWebForms.m {
	using System;
	using System.Web.UI.MobileControls;

	public partial class Login : MobilePage {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void loginButton_Click(object sender, EventArgs e) {
			this.openIdTextBox.LogOn();
		}
	}
}
