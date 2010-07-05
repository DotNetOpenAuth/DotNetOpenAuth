namespace OAuthServiceProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.SessionState;
	using Code;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	public class OAuth2 : IHttpHandler, IRequiresSessionState {
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
			switch (context.Request.PathInfo) {
				case "/token":
					if (Global.AuthorizationServer.TryPrepareAccessTokenResponse(out response)) {
						Global.AuthorizationServer.Channel.Send(response);
					}
					break;
				case "/auth":
					var request = Global.AuthorizationServer.ReadAuthorizationRequest();
					if (request == null) {
						throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
					}

					// Redirect the user to a page that requires the user to be logged in.
					Global.PendingOAuth2Authorization = request;
					context.Response.Redirect("~/Members/Authorize2.aspx");
					break;
			}
		}
	}
}