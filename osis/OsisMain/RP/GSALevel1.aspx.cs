using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;

public partial class RP_GSALevel1 : System.Web.UI.Page {
	private static OpenIdProvider provider = new OpenIdProvider();

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			Util.EnsureHttpsByRedirection();
			IRequest request = provider.GetRequest();
			if (request != null) {
				if (!request.IsResponseReady) {
					var authRequest = (IAuthenticationRequest)request;
					var pape = authRequest.GetExtension<PolicyRequest>();
					if (pape != null && pape.PreferredPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
						MultiView1.SetActiveView(RequestGsa);
						ProviderEndpoint.PendingAuthenticationRequest = authRequest;
						rpDiscoveryResult.Text = authRequest.IsReturnUrlDiscoverable(provider) == RelyingPartyDiscoveryResult.Success ? "succeeded" : "failed";
					} else {
						MultiView1.SetActiveView(RequestNotGsa);
					}
				}

				if (request.IsResponseReady) {
					provider.SendResponse(request);
				}
			}
		}
	}

	protected void continueButton_Click(object sender, EventArgs e) {
		var authRequest = ProviderEndpoint.PendingAuthenticationRequest;
		ProviderEndpoint.PendingAuthenticationRequest = null;
		authRequest.IsAuthenticated = true;
		authRequest.ClaimedIdentifier = new Uri(Request.Url, Page.ResolveUrl("~/RP/GSALevel1Identity.aspx"));
		authRequest.LocalIdentifier = authRequest.ClaimedIdentifier;
		provider.SendResponse(authRequest);
	}
}
