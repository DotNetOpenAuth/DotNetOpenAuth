namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IdentityModel.Claims;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.InfoCard;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;

	public partial class LoginFrame : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				// Because this page can appear as an iframe in a popup of another page,
				// we need to record which page the hosting page is in order to redirect back
				// to it after login is complete.
				this.ClientScript.RegisterOnSubmitStatement(
					this.GetType(),
					"getTopWindowUrl",
					"document.getElementById('topWindowUrl').value = window.parent.location.href;");
			}

			// We set the privacy policy URL here instead of in the ASPX page with the rest of the
			// Simple Registration extension so that we can construct the absolute URL rather than
			// hard-coding it.
			this.openIdSelector.Extensions.OfType<ClaimsRequest>().Single().PolicyUrl = new Uri(Request.Url, Page.ResolveUrl("~/PrivacyPolicy.aspx"));
		}

		protected void openIdSelector_LoggedIn(object sender, OpenIdEventArgs e) {
			this.LoginUser(RelyingPartyLogic.User.ProcessUserLogin(e.Response));
		}

		protected void openIdSelector_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
			this.LoginUser(RelyingPartyLogic.User.ProcessUserLogin(e.Token));
		}

		protected void openIdSelector_Failed(object sender, OpenIdEventArgs e) {
			if (e.Response.Exception != null) {
				this.errorMessageLabel.Text = HttpUtility.HtmlEncode(e.Response.Exception.ToStringDescriptive());
			}
			this.errorPanel.Visible = true;
		}

		protected void openIdSelector_TokenProcessingError(object sender, TokenProcessingErrorEventArgs e) {
			this.errorMessageLabel.Text = HttpUtility.HtmlEncode(e.Exception.ToStringDescriptive());
			this.errorPanel.Visible = true;
		}

		private void LoginUser(AuthenticationToken openidToken) {
			bool persistentCookie = false;
			if (string.IsNullOrEmpty(this.Request.QueryString["ReturnUrl"])) {
				FormsAuthentication.SetAuthCookie(openidToken.ClaimedIdentifier, persistentCookie);
				if (!string.IsNullOrEmpty(this.topWindowUrl.Value)) {
					Uri topWindowUri = new Uri(this.topWindowUrl.Value);
					string returnUrl = HttpUtility.ParseQueryString(topWindowUri.Query)["ReturnUrl"];
					if (string.IsNullOrEmpty(returnUrl)) {
						if (string.Equals(topWindowUri.AbsolutePath, Utilities.ApplicationRoot.AbsolutePath + "login.aspx", StringComparison.OrdinalIgnoreCase)) {
							// this happens when the user navigates deliberately directly to login.aspx
							Response.Redirect("~/");
						} else {
							Response.Redirect(this.topWindowUrl.Value);
						}
					} else {
						Response.Redirect(returnUrl);
					}
				} else {
					// This happens for unsolicited assertions.
					Response.Redirect("~/");
				}
			} else {
				FormsAuthentication.RedirectFromLoginPage(openidToken.ClaimedIdentifier, persistentCookie);
			}
		}
	}
}
