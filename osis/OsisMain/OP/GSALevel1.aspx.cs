using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenAuth.OpenId.Behaviors;

public partial class OP_GSALevel1 : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}

	protected void OpenIdBox_LoggingIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		// Always mark it as allowed, although we'll add the PAPE no-PII URI if we don't want any for this test.
		USGovernmentLevel1.AllowPersonallyIdentifiableInformation = true;

		if (!OpenIdBox.EnableRequestProfile) {
			var pape = new PolicyRequest();
			pape.PreferredPolicies.Add(AuthenticationPolicies.NoPersonallyIdentifiableInformation);
			e.Request.AddExtension(pape);
		}

		testResultDisplay.PrepareRequest(e.Request);
	}

	protected void OpenIdBox_LoggedIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.Pass = true;
		testResultDisplay.LoadResponse(e.Response);
	}

	protected void includePii_CheckedChanged(object sender, EventArgs e) {
		OpenIdBox.EnableRequestProfile = true;
	}
}
