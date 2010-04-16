namespace OpenIdProviderWebForms {
	using System;
	using System.Configuration;
	using System.Globalization;
	using System.Web.Security;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Page for handling logins to this server. 
	/// </summary>
	public partial class login : System.Web.UI.Page {
		protected void Page_Load(object src, EventArgs e) {
			if (!IsPostBack) {
				this.yubicoPanel.Visible = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["YubicoAPIKey"]);

				if (ProviderEndpoint.PendingAuthenticationRequest != null &&
					!ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
					this.login1.UserName = Code.Util.ExtractUserName(
						ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier);
					((TextBox)this.login1.FindControl("UserName")).ReadOnly = true;
					this.login1.FindControl("Password").Focus();
				}
			}
		}

		protected void yubicoButton_Click(object sender, EventArgs e) {
			string username;
			if (this.TryVerifyYubikeyAndGetUsername(this.yubicoBox.Text, out username)) {
				FormsAuthentication.RedirectFromLoginPage(username, false);
			}
		}

		private bool TryVerifyYubikeyAndGetUsername(string token, out string username) {
			var yubikey = new YubikeyRelyingParty(int.Parse(ConfigurationManager.AppSettings["YubicoAPIKey"], CultureInfo.InvariantCulture));
			YubikeyResult result = yubikey.IsValid(token);
			switch (result) {
				case YubikeyResult.Ok:
					username = YubikeyRelyingParty.ExtractUsername(token);
					return true;
				default:
					this.yubikeyFailureLabel.Visible = true;
					this.yubikeyFailureLabel.Text = result.ToString();
					username = null;
					return false;
			}
		}
	}
}