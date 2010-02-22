//-----------------------------------------------------------------------
// <copyright file="RefreshAccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access token renewal failed, probably due to an invalid
	/// refresh token.
	/// </summary>
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class RefreshAccessTokenFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal RefreshAccessTokenFailedResponse(RefreshAccessTokenRequest request)
			: base(request) {
		}
	}
}
