//-----------------------------------------------------------------------
// <copyright file="ClientBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
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
			ErrorUtilities.VerifyArgumentNotNull(authorizationServer, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientBase"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer endpoint.</param>
		protected ClientBase(Uri authorizationServer)
			: this(new AuthorizationServerDescription(authorizationServer)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientBase"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer endpoint.</param>
		protected ClientBase(string authorizationServer)
			: this(new Uri(authorizationServer)) {
		}

		/// <summary>
		/// Gets the token issuer.
		/// </summary>
		/// <value>The token issuer.</value>
		public AuthorizationServerDescription AuthorizationServer { get; private set; }

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		public Channel Channel { get; private set; }

		/// <summary>
		/// Adds the necessary HTTP header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			request.Headers[HttpRequestHeader.Authorization] = Protocol.HttpAuthorizationScheme + " " + accessToken;
		}
	}
}
