using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Messages;

public partial class RP_HMACSHA256 : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		// This page acts as both an identity endpoint and an OP endpoint
		// (crazy, but hey, why not?).  The identity page functionality
		// is provided by the IdentityEndpoint control this page hosts.
		if (!IsPostBack) {
			// A very miniature Provider that requires HMAC-SHA256 associations.
			OpenIdProvider provider = new OpenIdProvider();
			provider.SecuritySettings.MinimumHashBitLength = 256;
			provider.SecuritySettings.MaximumHashBitLength = 256;
			IRequest request = provider.GetRequest();
			if (request != null) {
				var authRequest = request as IAuthenticationRequest;
				if (authRequest != null) {
					var opRequest = (AuthenticationRequest)authRequest;
					var checkIdRequest = (CheckIdRequest)opRequest.RequestMessage;

					// The RP must not be operating in dumb mode, or else 
					// we're not actually verifying its use of HMAC-SHA256.
					bool pass = !string.IsNullOrEmpty(checkIdRequest.AssociationHandle);

					// We never send an assertion back, as the test is complete.
					MultiView1.ActiveViewIndex = 1;
					testResultDisplay.ProviderEndpoint = checkIdRequest.Realm.NoWildcardUri;
					testResultDisplay.ProtocolVersion = checkIdRequest.Version;
					testResultDisplay.Pass = pass;
				} else {
					provider.SendResponse(request);
				}
			}
		}
	}
}
