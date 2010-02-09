//-----------------------------------------------------------------------
// <copyright file="ClientBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A base class for common OAuth WRAP Consumer behaviors.
	/// </summary>
	public class ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientBase"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		protected ClientBase(AuthorizationServerDescription authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
			this.AuthorizationServer = authorizationServer;
		}

		/// <summary>
		/// Gets the token issuer.
		/// </summary>
		/// <value>The token issuer.</value>
		public AuthorizationServerDescription AuthorizationServer { get; private set; }

		/// <summary>
		/// Gets the OAuth WRAP channel.
		/// </summary>
		/// <value>The channel.</value>
		public Channel Channel { get; private set; }

		/// <summary>
		/// Adds the necessary HTTP Authorization header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(accessToken));
			WrapUtilities.AuthorizeWithOAuthWrap(request, accessToken);
		}
	}
}
