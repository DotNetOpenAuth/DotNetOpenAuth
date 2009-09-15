//-----------------------------------------------------------------------
// <copyright file="AccessTokenWithDelegationCodeSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The direct response message that contains the access token from the Token Issuer
	/// to the Consumer.
	/// </summary>
	internal class AccessTokenWithDelegationCodeSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenWithDelegationCodeSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenWithDelegationCodeSuccessResponse(AccessTokenWithDelegationCodeRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The token.</value>
		[MessagePart(Protocol.sa_token, IsRequired = true, AllowEmpty = false)]
		internal string Token { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.sa_token_expires_in, IsRequired = false, AllowEmpty = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }
	}
}
