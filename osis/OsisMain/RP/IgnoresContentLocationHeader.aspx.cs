using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;

public partial class RP_IgnoresContentLocationHeader : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		// For the exploited victim in this scenario, we choose a valid OpenID, but one for which
		// this page's built-in OP is not authoritative to assert for.
		Uri victim = new Uri(Request.Url, Request.ApplicationPath + "/RP/AffirmativeIdentity.aspx");
		Response.AddHeader("Content-Location", victim.AbsoluteUri);
	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		multiView1.SetActiveView(testResultsView);
		testResultDisplay1.ProviderEndpoint = new Uri(e.Request.Realm);
		testResultDisplay1.ProtocolVersion = Protocol.Lookup(e.Request.RelyingPartyVersion).Version;
		if (e.Request.ClaimedIdentifier == new Uri(Request.Url, Request.Url.AbsolutePath)) {
			testResultDisplay1.Pass = true;
		} else {
			testResultDisplay1.Pass = false;
			testResultDisplay1.Details = "The RP sent the wrong openid.claimed_id in the request.  It may be considering the Content-Location header to be authoritative, which is a security hole.";
			warningPanel.Visible = true;
			victimIdLabel.Text = e.Request.ClaimedIdentifier;
		}
	}

	protected void continueLoginButton_Click(object sender, EventArgs e) {
		ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
		ProviderEndpoint.SendResponse();
	}
}
