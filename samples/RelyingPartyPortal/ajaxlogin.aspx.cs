using System;
using System.Web.UI.WebControls;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;

namespace ConsumerPortal {
	public partial class ajaxlogin : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				OpenIdAjaxTextBox1.Focus();
			}
		}

		protected void OpenIdAjaxTextBox1_LoggingIn(object sender, OpenIdEventArgs e) {
			e.Request.AddExtension(new ClaimsRequest {
				Email = DemandLevel.Request,
			});
		}

		protected void OpenIdAjaxTextBox1_LoggedIn(object sender, OpenIdEventArgs e) {
			Label label = ((Label)commentSubmitted.FindControl("emailLabel"));
			label.Text = e.Response.FriendlyIdentifierForDisplay;

			// We COULD get the sreg extension response here for the email, but since we let the user
			// potentially change the email in the HTML form, we'll use that instead.
			//var claims = OpenIdAjaxTextBox1.AuthenticationResponse.GetExtension<ClaimsResponse>();
			if (emailAddressBox.Text.Length > 0) {
				label.Text += " (" + emailAddressBox.Text + ")";
			}
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

		protected void editComment_Click(object sender, EventArgs e) {
			multiView.ActiveViewIndex = 0;
		}

		protected void OpenIdAjaxTextBox1_UnconfirmedPositiveAssertion(object sender, OpenIdEventArgs e) {
			// This is where we register extensions that we want to have available in javascript
			// on the browser.
			OpenIdAjaxTextBox1.RegisterClientScriptExtension<ClaimsResponse>("sreg");
		}
	}
}
