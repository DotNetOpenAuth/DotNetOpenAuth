using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using System.Net;
using DotNetOpenAuth.Messaging;

public partial class RP_DiscoverableReturnTo : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		resultsPanel.Visible = true;
		realmLabel.Text = e.Request.Realm;

		RelyingPartyDiscoveryResult discoveryResult = e.Request.IsReturnUrlDiscoverable(ProviderEndpoint.Provider);
		if (discoveryResult == RelyingPartyDiscoveryResult.Success) {
			MultiView1.SetActiveView(PassView);
		} else {
			// Let's collect some details of the failure.
			MultiView1.SetActiveView(
				discoveryResult == RelyingPartyDiscoveryResult.NoMatchingReturnTo ? FailNoMatchingReturnTo : FailNoXrds);
		}
	}
}
