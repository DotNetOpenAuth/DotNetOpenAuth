using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenAuth.OpenId.Behaviors;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

public partial class OP_GSALevel1 : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}

	protected void OpenIdBox_LoggingIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		if (!OpenIdBox.EnableRequestProfile) {
			var pape = new PolicyRequest();
			pape.PreferredPolicies.Add(AuthenticationPolicies.NoPersonallyIdentifiableInformation);
			if (maxAuthAgeBox.Text.Trim().Length > 0) {
				pape.MaximumAuthenticationAge = TimeSpan.FromSeconds(double.Parse(maxAuthAgeBox.Text.Trim()));
			}

			e.Request.AddExtension(pape);
		}

		e.Request.AddCallbackArguments("piiAllowed", OpenIdBox.EnableRequestProfile.ToString());

		testResultDisplay.PrepareRequest(e.Request);
	}

	protected void OpenIdBox_LoggedIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;

		var sreg = e.Response.GetExtension<ClaimsResponse>();
		sregResponsePanel.Visible = sreg != null;
		profileFieldsDisplay.ProfileValues = sreg;
		bool wasPiiAllowed = bool.Parse(e.Response.GetCallbackArgument("piiAllowed"));

		testResultDisplay.Pass = true;
		testResultDisplay.LoadResponse(e.Response);
		if (!wasPiiAllowed) {
			var pape = e.Response.GetExtension<PolicyResponse>();
			if (pape == null || !pape.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "The PAPE authentication policy " + AuthenticationPolicies.NoPersonallyIdentifiableInformation + " is missing from the response.";
			}
			if (sreg != null) {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "Provider included PII when it was forbidden.";
			}
		}
	}

	protected void includePii_CheckedChanged(object sender, EventArgs e) {
		OpenIdBox.EnableRequestProfile = true;
	}
}
