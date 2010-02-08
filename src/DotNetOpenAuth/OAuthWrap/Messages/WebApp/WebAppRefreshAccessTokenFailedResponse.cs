//-----------------------------------------------------------------------
// <copyright file="WebAppRefreshAccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that the <see cref="WebAppRefreshAccessTokenRequest"/> message had an
	/// invalid calback URL or verification code.
	/// </summary>
	internal class WebAppRefreshAccessTokenFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppRefreshAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppRefreshAccessTokenFailedResponse(WebAppRefreshAccessTokenRequest request)
			: base(request) {
		}
	}
}
