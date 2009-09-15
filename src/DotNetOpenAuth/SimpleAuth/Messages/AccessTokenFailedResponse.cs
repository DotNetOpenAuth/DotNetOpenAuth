//-----------------------------------------------------------------------
// <copyright file="AccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The direct response message that may contain the reason the access token 
	/// was NOT returned from the Token Issuer to the Consumer.
	/// </summary>
	internal class AccessTokenFailedResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenFailedResponse(AccessTokenWithDelegationCodeRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the error reason.
		/// </summary>
		/// <value>
		/// The reason for the failure.  Among other values, it may be <c>null</c>
		/// or expired_delegation_code.
		/// </value>
		[MessagePart(Protocol.sa_error_reason, IsRequired = false, AllowEmpty = true)]
		internal string ErrorReason { get; set; }
	}
}
