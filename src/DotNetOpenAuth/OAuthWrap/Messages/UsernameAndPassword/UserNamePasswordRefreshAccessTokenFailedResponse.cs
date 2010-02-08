//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordRefreshAccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access token renewal failed, probably due to an invalid
	/// refresh token.
	/// </summary>
	internal class UserNamePasswordRefreshAccessTokenFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRefreshAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserNamePasswordRefreshAccessTokenFailedResponse(UserNamePasswordRefreshAccessTokenRequest request)
			: base(request) {
		}
	}
}
