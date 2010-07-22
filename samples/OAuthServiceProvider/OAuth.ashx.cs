namespace OAuthServiceProvider {
	using System.Web;
	using System.Web.SessionState;
	using Code;
	using DotNetOpenAuth.Messaging;

	public class OAuth : IHttpHandler, IRequiresSessionState {
		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <value>Always <c>true</c></value>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
		/// </returns>
		public bool IsReusable {
			get { return true; }
		}

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context) {
			IDirectResponseProtocolMessage response;
			if (Global.AuthorizationServer.TryPrepareAccessTokenResponse(out response)) {
				Global.AuthorizationServer.Channel.Send(response);
			}
		}
	}
}