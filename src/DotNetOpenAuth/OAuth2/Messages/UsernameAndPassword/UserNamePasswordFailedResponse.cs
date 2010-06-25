//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Consumer to indicate that a
	/// request for a delegation code failed, probably due to an invalid
	/// username and password.
	/// </summary>
	internal class UserNamePasswordFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserNamePasswordFailedResponse(UserNamePasswordRequest request)
			: base(request) {
		}
	}
}
