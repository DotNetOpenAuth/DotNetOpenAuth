//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordRefreshAccessTokenFailedResponse.cs" company="Andrew Arnott">
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
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access token renewal failed, probably due to an invalid
	/// refresh token.
	/// </summary>
	internal class UserNamePasswordRefreshAccessTokenFailedResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRefreshAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserNamePasswordRefreshAccessTokenFailedResponse(UserNamePasswordRefreshAccessTokenRequest request)
			: base(request) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct respones should be sent with.
		/// </summary>
		/// <value><see cref="HttpStatusCode.Unauthorized"/></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.Unauthorized; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
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
