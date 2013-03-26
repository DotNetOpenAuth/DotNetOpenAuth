namespace OpenIdWebRingSsoProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdWebRingSsoProvider.Code;

	public partial class Server : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void providerEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						await Util.ProcessAuthenticationChallengeAsync(e.Request, ct);
					}));
		}
	}
}
