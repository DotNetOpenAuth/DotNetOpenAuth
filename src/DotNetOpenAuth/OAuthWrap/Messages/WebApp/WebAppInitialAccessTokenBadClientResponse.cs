//-----------------------------------------------------------------------
// <copyright file="WebAppInitialAccessTokenBadClientResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that the <see cref="WebAppInitialAccessTokenRequest"/> message had an
	/// invalid Client Identifier and Client Secret combination.
	/// </summary>
	internal class WebAppInitialAccessTokenBadClientResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppInitialAccessTokenBadClientResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppInitialAccessTokenBadClientResponse(WebAppInitialAccessTokenRequest request)
			: base(request) {
		}
	}
}
