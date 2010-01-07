namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class ajaxlogin : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				this.OpenIdAjaxTextBox1.Focus();
			}
		}

		protected void OpenIdAjaxTextBox1_LoggingIn(object sender, OpenIdEventArgs e) {
			e.Request.AddExtension(new ClaimsRequest {
				Email = DemandLevel.Require,
			});
		}

		protected void OpenIdAjaxTextBox1_LoggedIn(object sender, OpenIdEventArgs e) {
			Label label = (Label)this.commentSubmitted.FindControl("emailLabel");
			label.Text = e.Response.FriendlyIdentifierForDisplay;

			// We COULD get the sreg extension response here for the email, but since we let the user
			// potentially change the email in the HTML form, we'll use that instead.
			////var claims = OpenIdAjaxTextBox1.AuthenticationResponse.GetExtension<ClaimsResponse>();
			if (this.emailAddressBox.Text.Length > 0) {
				label.Text += " (" + this.emailAddressBox.Text + ")";
			}
		}

		protected void submitButton_Click(object sender, EventArgs e) {
			if (!Page.IsValid) {
				return;
			}
			if (this.OpenIdAjaxTextBox1.AuthenticationResponse != null) {
				if (this.OpenIdAjaxTextBox1.AuthenticationResponse.Status == AuthenticationStatus.Authenticated) {
					// Save comment here!
					this.multiView.ActiveViewIndex = 1;
				} else {
					this.multiView.ActiveViewIndex = 2;
				}
			}
		}

		protected void editComment_Click(object sender, EventArgs e) {
			this.multiView.ActiveViewIndex = 0;
		}

		protected void OpenIdAjaxTextBox1_UnconfirmedPositiveAssertion(object sender, OpenIdEventArgs e) {
			// This is where we register extensions that we want to have available in javascript
			// on the browser.
			this.OpenIdAjaxTextBox1.RegisterClientScriptExtension<ClaimsResponse>("sreg");
		}
	}
}
