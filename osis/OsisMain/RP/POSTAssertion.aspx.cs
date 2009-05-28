using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;

public partial class RP_POSTAssertion : System.Web.UI.Page {
	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		e.Request.IsAuthenticated = true;

		// Rather than let our ProviderEndpoint control send the assertion, send it using a
		// custom channel that is rigged to always use POSTs.
		OpenIdProvider op = new OpenIdProvider();
		op.Channel.IndirectMessageGetToPostThreshold = 1; // force it to always use POST
		op.SendResponse(e.Request);
	}
}
