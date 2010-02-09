//-----------------------------------------------------------------------
// <copyright file="WebAppAccessTokenBadClientResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that the <see cref="WebAppAccessTokenRequest"/> message had an
	/// invalid Client Identifier and Client Secret combination.
	/// </summary>
	internal class WebAppAccessTokenBadClientResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppAccessTokenBadClientResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppAccessTokenBadClientResponse(WebAppAccessTokenRequest request)
			: base(request) {
		}
	}
}
