using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenId.RelyingParty;

namespace ConsumerPortal {
	public partial class ajaxlogin : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			OpenIdAjaxTextBox1.Focus();
		}

		protected void submitButton_Click(object sender, EventArgs e) {
			if (OpenIdAjaxTextBox1.AuthenticationResponse != null) {
				if (OpenIdAjaxTextBox1.AuthenticationResponse.Status == AuthenticationStatus.Authenticated) {
					// Save comment here!

					multiView.ActiveViewIndex = 1;
				} else {
					multiView.ActiveViewIndex = 2;
				}
			}
		}
	}
}
