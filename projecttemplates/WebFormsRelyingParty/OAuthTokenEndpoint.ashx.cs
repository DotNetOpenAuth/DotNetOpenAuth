//-----------------------------------------------------------------------
// <copyright file="OAuthTokenEndpoint.ashx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.SessionState;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using RelyingPartyLogic;

	using WebFormsRelyingParty.Code;

	/// <summary>
	/// An OAuth 2.0 token endpoint.
	/// </summary>
	public class OAuthTokenEndpoint : IHttpAsyncHandler, IRequiresSessionState {
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
			this.ProcessRequestAsync(context).GetAwaiter().GetResult();
		}

		public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
			return this.ProcessRequestAsync(context).ToApm(cb, extraData);
		}

		public void EndProcessRequest(IAsyncResult result) {
			((Task)result).Wait(); // rethrows exceptions
		}

		private async Task ProcessRequestAsync(HttpContext context) {
			var serviceProvider = OAuthServiceProvider.AuthorizationServer;
			var response = await serviceProvider.HandleTokenRequestAsync(new HttpRequestWrapper(context.Request), context.Response.ClientDisconnectedToken);
			await response.SendAsync(new HttpResponseWrapper(context.Response), context.Response.ClientDisconnectedToken);
		}
	}
}
