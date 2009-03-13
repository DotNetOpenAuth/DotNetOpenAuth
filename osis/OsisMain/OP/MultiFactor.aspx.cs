using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;

public partial class OP_MultiFactor : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}
	protected void OpenIdBox_LoggingIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		var policyRequest = new PolicyRequest();
		policyRequest.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
		e.Request.AddExtension(policyRequest);
		testResultDisplay.PrepareRequest(e.Request);
	}
	protected void OpenIdBox_LoggedIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;
		var policyResponse = e.Response.GetExtension<PolicyResponse>();
		testResultDisplay.Pass = policyResponse.ActualPolicies.Contains(AuthenticationPolicies.MultiFactor);
		testResultDisplay.LoadResponse(e.Response);
	}
}
