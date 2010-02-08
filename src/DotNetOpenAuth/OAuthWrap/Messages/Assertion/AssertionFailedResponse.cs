//-----------------------------------------------------------------------
// <copyright file="AssertionFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	/// <summary>
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access code failed, probably due to an invalid assertion.
	/// </summary>
	internal class AssertionFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AssertionFailedResponse(AssertionRequest request)
			: base(request) {
		}
	}
}
