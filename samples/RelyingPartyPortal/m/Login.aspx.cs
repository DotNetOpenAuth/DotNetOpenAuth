using System;
using System.Web.UI.MobileControls;

namespace ConsumerPortal.m {
	public partial class Login : MobilePage {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void loginButton_Click(object sender, EventArgs e) {
			openIdTextBox.LogOn();
		}
	}
}
