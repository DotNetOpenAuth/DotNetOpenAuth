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
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using RelyingPartyLogic;
	using WebFormsRelyingParty.Code;

	/// <summary>
	/// An OAuth 2.0 token endpoint.
	/// </summary>
	public class OAuthTokenEndpoint : HttpAsyncHandlerBase, IRequiresSessionState {
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
		public override bool IsReusable {
			get { return true; }
		}

		protected override async Task ProcessRequestAsync(HttpContext context) {
			var serviceProvider = OAuthServiceProvider.AuthorizationServer;
			var response = await serviceProvider.HandleTokenRequestAsync(new HttpRequestWrapper(context.Request), context.Response.ClientDisconnectedToken);
			await response.SendAsync(new HttpContextWrapper(context), context.Response.ClientDisconnectedToken);
		}
	}
}
