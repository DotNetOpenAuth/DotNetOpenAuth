using System;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Provider;

/// <summary>
/// Page for handling logins to this server. 
/// </summary>
public partial class login : System.Web.UI.Page {
	protected void Page_Load(object src, EventArgs e) {
		if (!IsPostBack) {
			if (ProviderEndpoint.PendingAuthenticationRequest != null) {
				login1.UserName = Util.ExtractUserName(new Uri(ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier));
				((TextBox)login1.FindControl("UserName")).ReadOnly = true;
				login1.FindControl("Password").Focus();
			}
		}
	}
}
