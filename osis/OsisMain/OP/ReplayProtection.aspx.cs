using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using System.ServiceModel;
using System.Diagnostics;

public partial class OP_ReplayProtection : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		OpenIdBox.Focus();
	}

	protected void OpenIdBox_LoggedIn(object sender, OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.SetActiveView(View2);
		testResultDisplay.ProviderEndpoint = e.Response.Provider.Uri;
		testResultDisplay.ProtocolVersion = e.Response.Provider.Version;
		try {
			// Call GetResponse again, which will cause ANOTHER direct verification message
			// to be sent out, since we don't cache the prior result.
			var response = OpenIdBox.RelyingParty.GetResponse();
			if (response.Status == AuthenticationStatus.Failed) {
				testResultDisplay.Pass = true;
				testResultDisplay.Details = "Provider correctly rejected the second attempt at direct verification.";
			} else {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "Provider verified positive assertion twice.";
			}
		} catch (ProtocolException ex) {
			testResultDisplay.Pass = true;
			testResultDisplay.Details = "Unexpected response: " + ex.Message;
		}
	}
}
