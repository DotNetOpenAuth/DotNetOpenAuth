//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationViaUsernamePasswordFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleApiAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Token Issuer to the Consumer to indicate that a
	/// request for a delegation code failed, probably due to an invalid
	/// username and password.
	/// </summary>
	internal class UserAuthorizationViaUsernamePasswordFailedResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationViaUsernamePasswordFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserAuthorizationViaUsernamePasswordFailedResponse(UserAuthorizationViaUsernamePasswordRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the error reason.
		/// </summary>
		/// <value>
		/// The reason for the failure.  Among other values, it may be <c>null</c>
		/// or invalid_user_credentials.
		/// </value>
		[MessagePart(Protocol.sa_error_reason, IsRequired = false, AllowEmpty = true)]
		internal string ErrorReason { get; set; }
	}
}
