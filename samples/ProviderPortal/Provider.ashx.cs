using System.Web;
using System.Web.SessionState;
using DotNetOpenId.Provider;

namespace ProviderPortal {
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
		const string pendingAuthenticationRequestKey = "pendingAuthenticationRequestKey";
		internal static IAuthenticationRequest PendingAuthenticationRequest {
			get { return HttpContext.Current.Session[pendingAuthenticationRequestKey] as IAuthenticationRequest; }
			set { HttpContext.Current.Session[pendingAuthenticationRequestKey] = value; }
		}

		public void ProcessRequest(HttpContext context) {
			OpenIdProvider provider = new OpenIdProvider();
			if (provider.Request != null) {
				// Some OpenID requests are automatable and can be responded to immediately.
				if (!provider.Request.IsResponseReady) {
					// But authentication requests cannot be responded to until something on
					// this site decides whether to approve or disapprove the authentication.
					var idrequest = (IAuthenticationRequest)provider.Request;
					// We store the authentication request in the user's session so that
					// redirects and user prompts can appear and eventually some page can decide
					// to respond to the OpenID authentication request either affirmatively or
					// negatively.
					PendingAuthenticationRequest = idrequest;
					// We delegate that approval process to our utility method that we share
					// with our other Provider sample page server.aspx.
					Util.ProcessAuthenticationChallenge(idrequest);
					// As part of authentication approval, the user may need to authenticate
					// to this Provider and/or decide whether to allow the requesting RP site
					// to log this user in.  If any UI needs to be presented to the user, 
					// the previous call to ProcessAuthenticationChallenge MAY not return
					// due to a redirect to some ASPX page.
				} else {
					// Some other automatable OpenID request is coming down, so clear
					// any previously session stored authentication request that might be
					// stored for this user.
					PendingAuthenticationRequest = null;
				}
				// Whether this was an automated message or an authentication message,
				// if there is a response ready to send back immediately, do so.
				if (provider.Request.IsResponseReady) {
					provider.Request.Response.Send();
					PendingAuthenticationRequest = null;
				}
			}
		}

		public bool IsReusable {
			get { return true; }
		}
	}
}
