namespace OpenIdWebRingSsoProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdWebRingSsoProvider.Code;

	public partial class _Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			// The user may have just completed a login.  If they're logged in, see if we can complete the OpenID login.
			if (User.Identity.IsAuthenticated && ProviderEndpoint.PendingAuthenticationRequest != null) {
				Util.ProcessAuthenticationChallenge(ProviderEndpoint.PendingAuthenticationRequest);
				if (ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated.HasValue) {
					ProviderEndpoint.SendResponse();
				}
			}
		}
	}
}
