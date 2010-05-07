//-----------------------------------------------------------------------
// <copyright file="RichAppAccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that an access token could not be granted.
	/// </summary>
	internal class RichAppAccessTokenFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="RichAppAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal RichAppAccessTokenFailedResponse(RichAppAccessTokenRequest request)
			: base(request) {
		}
	}
}
