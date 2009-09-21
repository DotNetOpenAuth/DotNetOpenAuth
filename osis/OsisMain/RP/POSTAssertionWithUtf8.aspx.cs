using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Messages;

public partial class RP_POSTAssertionWithUtf8 : System.Web.UI.Page {
	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		// To be definitive, the auth request must have come in using a shared association
		// so the RP can do its own signature verification.
		//((AuthenticationRequest)e.Request).positiveResponse.as
		if (((CheckIdRequest)((AuthenticationRequest)e.Request).RequestMessage).AssociationHandle == null) {
			multiView.SetActiveView(sharedAssociationRequired);
		} else {
			e.Request.IsAuthenticated = true;

			// Tack on some multi-byte UTF-8 characters to the response
			e.Request.AddResponseExtension(new ClaimsResponse {
				Nickname = "çá"
			});

			// Rather than let our ProviderEndpoint control send the assertion, send it using a
			// custom channel that is rigged to always use POSTs.
			OpenIdProvider op = new OpenIdProvider();
			op.Channel.IndirectMessageGetToPostThreshold = 1; // force it to always use POST
			op.SendResponse(e.Request);
		}
	}
}
