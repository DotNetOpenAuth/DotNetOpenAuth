using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_RequireSsl : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}

	protected void OpenIdBox_LoggingIn(object sender, OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.ProviderEndpoint = e.Request.Provider.Uri;
		testResultDisplay.ProtocolVersion = e.Request.Provider.Version;
		testResultDisplay.Pass = true;
	}

	protected void OpenIdBox_Failed(object sender, EventArgs e) {
		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.Pass = false;
		testResultDisplay.Details = "Unable to find a secure endpoint.";
	}
}
