//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationViaUsernamePasswordSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Authorization Server to the Consumer containing a delegation code
	/// that the Consumer should use to obtain an access token.
	/// </summary>
	internal class UserAuthorizationViaUsernamePasswordSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationViaUsernamePasswordSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserAuthorizationViaUsernamePasswordSuccessResponse(UserAuthorizationViaUsernamePasswordRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Consumer for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.wrap_verification_code, IsRequired = true, AllowEmpty = true)]
		internal string VerificationCode { get; set; }
	}
}
