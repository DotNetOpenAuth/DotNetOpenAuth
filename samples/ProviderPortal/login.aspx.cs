namespace OpenIdProviderWebForms {
	using System;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Page for handling logins to this server. 
	/// </summary>
	public partial class login : System.Web.UI.Page {
		protected void Page_Load(object src, EventArgs e) {
			if (!IsPostBack) {
				if (ProviderEndpoint.PendingAuthenticationRequest != null &&
					!ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
					this.login1.UserName = Code.Util.ExtractUserName(
						ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier);
					((TextBox)this.login1.FindControl("UserName")).ReadOnly = true;
					this.login1.FindControl("Password").Focus();
				}
			}
		}
	}
}