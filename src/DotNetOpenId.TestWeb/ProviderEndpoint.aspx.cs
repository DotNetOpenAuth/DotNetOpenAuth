using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

public partial class ProviderEndpoint : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}
	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, DotNetOpenId.Provider.ProviderEndpoint.AuthenticationChallengeEventArgs e) {
		// immediately approve
		e.Request.IsAuthenticated = e.Request.IdentityUrl.AbsolutePath == "/bob";
		e.Request.Response.Send();
	}
}
