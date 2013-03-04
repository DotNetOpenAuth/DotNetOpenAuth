namespace OpenIdWebRingSsoProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdWebRingSsoProvider.Code;

	/// <summary>
	/// Challenges the user to authenticate to the OpenID SSO Provider.
	/// </summary>
	/// <remarks>
	/// This login page is used only when the Provider is configured for 
	/// FormsAuthentication.  The default configuration is to use 
	/// Windows authentication.
	/// </remarks>
	public partial class Login : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			// This site doesn't need XSRF protection because only trusted RPs are ever allowed to receive authentication results
			// and because the login page itself is the only page the user could ever see as an in-between step to logging in,
			// and a login form isn't vulnerable to XSRF.
			if (!IsPostBack) {
				if (ProviderEndpoint.PendingAuthenticationRequest != null) {
					if (!ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
						this.login1.UserName = Code.Util.ExtractUserName(
							ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier);
						((TextBox)this.login1.FindControl("UserName")).ReadOnly = true;
						this.login1.FindControl("Password").Focus();
					}
				}
				this.cancelButton.Visible = ProviderEndpoint.PendingAuthenticationRequest != null;
			}
		}

		protected void cancelButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var req = ProviderEndpoint.PendingAuthenticationRequest;
						if (req != null) {
							req.IsAuthenticated = false;
							var providerEndpoint = new ProviderEndpoint();
							var response = await providerEndpoint.PrepareResponseAsync(Response.ClientDisconnectedToken);
							await response.SendAsync(new HttpContextWrapper(this.Context), Response.ClientDisconnectedToken);
							this.Context.Response.End();
						}
					}));
		}
	}
}