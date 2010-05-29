namespace OAuthServiceProvider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Web;
	using Code;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap;

	/// <summary>
	/// Summary description for OAuth2
	/// </summary>
	public class OAuth2 : IHttpHandler {
		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context) {
			IDirectResponseProtocolMessage response;
			if (Global.AuthorizationServer.TryPrepareAccessTokenResponse(out response)) {
				Global.AuthorizationServer.Channel.Send(response);
			} else {
				var request = Global.AuthorizationServer.ReadAuthorizationRequest();
				if (request == null) {
					throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
				}

				// This sample doesn't implement support for immediate mode.
				if (!request.IsUserInteractionAllowed) {
					Global.AuthorizationServer.RejectAuthorizationRequest(request);
				}

				// Redirect the user to a page that requires the user to be logged in.
				Global.PendingOAuth2Authorization = request;
				context.Response.Redirect("~/Members/Authorize2.aspx");
			}
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <value>Always <c>true</c></value>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
		/// </returns>
		public bool IsReusable {
			get { return true; }
		}
	}
}