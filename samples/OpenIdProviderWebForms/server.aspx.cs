namespace OpenIdProviderWebForms {
	using System;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// This is the primary page for this open-id provider.
	/// This page is responsible for handling all open-id compliant requests.
	/// </summary>
	public partial class server : System.Web.UI.Page {
		protected void Page_Load(object src, System.EventArgs evt) {
			this.serverEndpointUrl.Text = Request.Url.ToString();
		}

		protected void provider_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
			Code.Util.ProcessAuthenticationChallenge(e.Request);
		}

		protected void provider_AnonymousRequest(object sender, AnonymousRequestEventArgs e) {
			Code.Util.ProcessAnonymousRequest(e.Request);
		}
	}
}