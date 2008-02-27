using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
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
				login1.UserName = Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.IdentityUrl);
				((TextBox)login1.FindControl("UserName")).ReadOnly = true;
				login1.FindControl("Password").Focus();
			}
		}
	}
}
