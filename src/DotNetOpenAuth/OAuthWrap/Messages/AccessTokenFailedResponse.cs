//-----------------------------------------------------------------------
// <copyright file="AccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Authorization Server to the Client to indicate that a
	/// request for an access token renewal failed, probably due to an invalid
	/// refresh token.
	/// </summary>
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class AccessTokenFailedResponse : UnauthorizedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenFailedResponse(IDirectedProtocolMessage request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>The error.</value>
		/// <remarks>
		/// REQUIRED. The parameter value MUST be set to one of the values specified by each flow. 
		/// </remarks>
		[MessagePart(Protocol.error, IsRequired = true, AllowEmpty = false)]
		internal string Error { get; set; }
	}
}
