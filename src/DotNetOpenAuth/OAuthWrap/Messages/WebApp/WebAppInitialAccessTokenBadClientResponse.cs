//-----------------------------------------------------------------------
// <copyright file="WebAppInitialAccessTokenBadClientResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that the <see cref="WebAppInitialAccessTokenRequest"/> message had an
	/// invalid Client Identifier and Client Secret combination.
	/// </summary>
	internal class WebAppInitialAccessTokenBadClientResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppInitialAccessTokenBadClientResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppInitialAccessTokenBadClientResponse(WebAppInitialAccessTokenRequest request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return System.Net.HttpStatusCode.Unauthorized; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get {
				return new WebHeaderCollection() {
					{ HttpResponseHeader.WwwAuthenticate, Protocol.HttpAuthorizationScheme },
				};
			}
		}

		#endregion
	}
}
