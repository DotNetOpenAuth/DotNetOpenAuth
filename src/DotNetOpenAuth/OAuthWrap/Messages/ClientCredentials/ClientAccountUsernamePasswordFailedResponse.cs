//-----------------------------------------------------------------------
// <copyright file="ClientAccountUsernamePasswordFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access code failed, probably due to an invalid account
	/// name and password.
	/// </summary>
	internal class ClientAccountUsernamePasswordFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAccountUsernamePasswordFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal ClientAccountUsernamePasswordFailedResponse(ClientAccountUsernamePasswordRequest request)
			: base(request) {
		}
	}
}
