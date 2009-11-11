//-----------------------------------------------------------------------
// <copyright file="OAuth.ashx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using WebFormsRelyingParty.Code;

	/// <summary>
	/// Responds to incoming OAuth Service Provider messages.
	/// </summary>
	public class OAuth : IHttpHandler {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth"/> class.
		/// </summary>
		public OAuth() {
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
			var serviceProvider = OAuthServiceProvider.ServiceProvider;
			var requestMessage = serviceProvider.ReadRequest(new HttpRequestInfo(context.Request));

			UnauthorizedTokenRequest unauthorizedTokenRequestMessage;
			AuthorizedTokenRequest authorizedTokenRequestMessage;
			UserAuthorizationRequest userAuthorizationRequest;
			if ((unauthorizedTokenRequestMessage = requestMessage as UnauthorizedTokenRequest) != null) {
				var response = serviceProvider.PrepareUnauthorizedTokenMessage(unauthorizedTokenRequestMessage);
				serviceProvider.Channel.Send(response);
			} else if ((authorizedTokenRequestMessage = requestMessage as AuthorizedTokenRequest) != null) {
				var response = serviceProvider.PrepareAccessTokenMessage(authorizedTokenRequestMessage);
				serviceProvider.Channel.Send(response);
			} else if ((userAuthorizationRequest = requestMessage as UserAuthorizationRequest) != null) {
				// This is a browser opening to allow the user to authorize a request token,
				// so redirect to the authorization page, which will automatically redirect
				// to have the user log in if necessary.
				OAuthServiceProvider.PendingAuthorizationRequest = userAuthorizationRequest;
				HttpContext.Current.Response.Redirect("~/Members/OAuthAuthorize.aspx");
			} else {
				throw new InvalidOperationException();
			}
		}
	}
}
