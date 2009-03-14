using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.ChannelElements;

public partial class RP_ResponseNonceCheck : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		OpenIdProvider op = new OpenIdProvider();

		// A pending authentication may already have been saved, in which case
		// the querystring will still contain the last incoming auth request, 
		// which we DON'T want to handle again.
		if (ViewState["PendingAuth"] == null) {
			IRequest req = op.GetRequest();
			if (req != null) {
				var authReq = req as IAuthenticationRequest;
				if (authReq != null) {
					// This test is only valid if the RP is using shared associations.
					// Private associations allow the RP to avoid response_nonce checking
					// because the OP does it for them, but since we're testing RP nonce
					// specifically, we cannot report success or failure unless the RP
					// is in "smart" mode.
					var opAuthReq = (AuthenticationRequest)authReq;
					if (((ITamperResistantOpenIdMessage)opAuthReq.positiveResponse).AssociationHandle == null) {
						// The RP is in dumb mode, which invalidates the test.
						DumbModeInvalidPanel.Visible = true;
					} else {
						authReq.IsAuthenticated = true;
						// Deliberately prepare the response message and store the user-agent
						// response off for reuse later so the response_nonce stays the same
						// on the second attempt.
						ViewState["PendingAuth"] = op.GetResponse(authReq);
						AuthPanel.Visible = true;
					}
				} else {
					op.SendResponse(req);
				}
			}
		}
	}

	protected void loginButton_Click(object sender, EventArgs e) {
		var response = (UserAgentResponse)ViewState["PendingAuth"];
		response.Send();
	}
}
