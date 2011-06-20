namespace OpenIdProviderWebForms {
	using System.Web;
	using System.Web.SessionState;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// A fast OpenID message handler that responds to OpenID messages
	/// directed at the Provider.
	/// </summary>
	/// <remarks>
	/// This performs the same function as server.aspx, which uses the ProviderEndpoint
	/// control to reduce the amount of source code in the web site.  A typical Provider
	/// site will have EITHER this .ashx handler OR the .aspx page -- NOT both.
	/// </remarks>
	public class Provider : IHttpHandler, IRequiresSessionState {
		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest(HttpContext context) {
			IRequest request = ProviderEndpoint.Provider.GetRequest();
			if (request != null) {
				// Some OpenID requests are automatable and can be responded to immediately.
				// But authentication requests cannot be responded to until something on
				// this site decides whether to approve or disapprove the authentication.
				if (!request.IsResponseReady) {
					// We store the request in the user's session so that
					// redirects and user prompts can appear and eventually some page can decide
					// to respond to the OpenID authentication request either affirmatively or
					// negatively.
					ProviderEndpoint.PendingRequest = request as IHostProcessedRequest;

					// We delegate that approval process to our utility method that we share
					// with our other Provider sample page server.aspx.
					if (ProviderEndpoint.PendingAuthenticationRequest != null) {
						Code.Util.ProcessAuthenticationChallenge(ProviderEndpoint.PendingAuthenticationRequest);
					} else if (ProviderEndpoint.PendingAnonymousRequest != null) {
						Code.Util.ProcessAnonymousRequest(ProviderEndpoint.PendingAnonymousRequest);
					}

					// As part of authentication approval, the user may need to authenticate
					// to this Provider and/or decide whether to allow the requesting RP site
					// to log this user in.  If any UI needs to be presented to the user, 
					// the previous call to ProcessAuthenticationChallenge MAY not return
					// due to a redirect to some ASPX page.
				}

				// Whether this was an automated message or an authentication message,
				// if there is a response ready to send back immediately, do so.
				if (request.IsResponseReady) {
					// We DON'T use ProviderEndpoint.SendResponse because
					// that only sends responses to requests in PendingAuthenticationRequest,
					// but we don't set that for associate and other non-checkid requests.
					ProviderEndpoint.Provider.Respond(request);

					// Make sure that any PendingAuthenticationRequest that MAY be set is cleared.
					ProviderEndpoint.PendingRequest = null;
				}
			}
		}
	}
}
