using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;

public partial class OP_GSALevel1 : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}

	protected void OpenIdBox_LoggingIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		testResultDisplay.PrepareRequest(e.Request);
	}

	protected void OpenIdBox_LoggedIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.Pass = true;
		testResultDisplay.LoadResponse(e.Response);
	}
}
