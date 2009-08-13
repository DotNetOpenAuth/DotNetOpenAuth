using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;

public partial class OP_MaxAuthAge : System.Web.UI.Page {
	private DateTime? EarliestAllowableAuthTime {
		get { return (DateTime?)Session["AuthBeginTime"]; }
		set { Session["AuthBeginTime"] = value; }
	}
	
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdBox.Focus();
		}
	}

	protected void OpenIdBox_LoggingIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		if (!Page.IsValid || maxAuthAgeBox.Text.Length == 0) {
			e.Cancel = true;
			return;
		}
		var policyRequest = new PolicyRequest();
		policyRequest.MaximumAuthenticationAge = TimeSpan.FromSeconds(double.Parse(maxAuthAgeBox.Text.Trim()));
		e.Request.AddExtension(policyRequest);
		testResultDisplay.PrepareRequest(e.Request);
		EarliestAllowableAuthTime = DateTime.UtcNow - policyRequest.MaximumAuthenticationAge;
	}

	protected void OpenIdBox_LoggedIn(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true;
		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.Pass = false;
		var policyResponse = e.Response.GetExtension<PolicyResponse>();
		if (policyResponse != null) {
			if (policyResponse.AuthenticationTimeUtc.HasValue) {
				if (policyResponse.AuthenticationTimeUtc.Value > EarliestAllowableAuthTime) {
					testResultDisplay.Pass = true;
				} else {
					testResultDisplay.Details = "PAPE response included an authentication time that indicates it is older than the age the RP demanded.";
				}
			} else {
				testResultDisplay.Details = "PAPE response omitted the actual authentication time.";
			}

			testResultDisplay.LoadResponse(e.Response);
		} else {
			testResultDisplay.LoadResponse(e.Response);
			testResultDisplay.Details = "No PAPE response.";
		}
	}
}
