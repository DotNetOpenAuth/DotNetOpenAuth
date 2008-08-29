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
			var claims = OpenIdAjaxTextBox1.AuthenticationResponse.GetExtension<ClaimsResponse>();
			Label label = ((Label)commentSubmitted.FindControl("emailLabel"));
			label.Text = claims != null ? claims.Email : "stranger (no email)";
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
			OpenIdAjaxTextBox1.RegisterClientScriptExtension(new ClaimsResponse(), "sreg");
		}
	}
}
