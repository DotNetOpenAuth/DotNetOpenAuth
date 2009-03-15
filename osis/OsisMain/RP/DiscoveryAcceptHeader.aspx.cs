using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;

public partial class RP_DiscoveryAcceptHeader : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		
	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		AuthPanel.Visible = true;
	}
}
