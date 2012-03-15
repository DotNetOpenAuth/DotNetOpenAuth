//-----------------------------------------------------------------------
// <copyright file="OAuthTokenEndpoint.ashx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.SessionState;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using RelyingPartyLogic;

	/// <summary>
	/// An OAuth 2.0 token endpoint.
	/// </summary>
	public class OAuthTokenEndpoint : IHttpHandler, IRequiresSessionState {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthTokenEndpoint"/> class.
		/// </summary>
		public OAuthTokenEndpoint() {
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
		/// </returns>
		public bool IsReusable {
			get { return true; }
		}

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context) {
			OAuthServiceProvider.AuthorizationServer.HandleTokenRequest().Respond();
		}
	}
}
