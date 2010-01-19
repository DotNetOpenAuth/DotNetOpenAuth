//-----------------------------------------------------------------------
// <copyright file="ConsumerBase.cs" company="Andrew Arnott">
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
	public class ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer.</param>
		protected ConsumerBase(AuthorizationServerDescription tokenIssuer) {
			ErrorUtilities.VerifyArgumentNotNull(tokenIssuer, "tokenIssuer");
			this.TokenIssuer = tokenIssuer;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
		protected ConsumerBase(Uri tokenIssuerEndpoint)
			: this(new AuthorizationServerDescription(tokenIssuerEndpoint)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
		protected ConsumerBase(string tokenIssuerEndpoint)
			: this(new Uri(tokenIssuerEndpoint)) {
		}

		/// <summary>
		/// Gets the token issuer.
		/// </summary>
		/// <value>The token issuer.</value>
		public AuthorizationServerDescription TokenIssuer { get; private set; }

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
		/// <param name="accessToken">The access token previously obtained from the Token Issuer.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			request.Headers[HttpRequestHeader.Authorization] = Protocol.HttpAuthorizationScheme + " " + accessToken;
		}
	}
}
