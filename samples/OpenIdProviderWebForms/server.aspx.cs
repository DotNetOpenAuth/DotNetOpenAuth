namespace OpenIdProviderWebForms {
	using System;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// This is the primary page for this open-id provider.
	/// This page is responsible for handling all open-id compliant requests.
	/// </summary>
	public partial class server : System.Web.UI.Page {
		protected void Page_Load(object src, EventArgs evt) {
			this.serverEndpointUrl.Text = Request.Url.ToString();
		}

		protected void provider_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
			// We store the request in the user's session so that
			// redirects and user prompts can appear and eventually some page can decide
			// to respond to the OpenID authentication request either affirmatively or
			// negatively.
			ProviderEndpoint.PendingRequest = e.Request;

			Code.Util.ProcessAuthenticationChallenge(e.Request);
		}

		protected void provider_AnonymousRequest(object sender, AnonymousRequestEventArgs e) {
			// We store the request in the user's session so that
			// redirects and user prompts can appear and eventually some page can decide
			// to respond to the OpenID authentication request either affirmatively or
			// negatively.
			ProviderEndpoint.PendingRequest = e.Request;

			Code.Util.ProcessAnonymousRequest(e.Request);
		}
	}
}