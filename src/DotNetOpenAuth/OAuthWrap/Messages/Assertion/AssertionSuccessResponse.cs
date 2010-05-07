//-----------------------------------------------------------------------
// <copyright file="AssertionSuccessResponse.cs" company="Andrew Arnott">
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
	/// A response from the Authorization Server to the Client containing an access code.
	/// </summary>
	internal class AssertionSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AssertionSuccessResponse(ClientAccountUsernamePasswordRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart(Protocol.access_token, IsRequired = true, AllowEmpty = false)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }
	}
}
