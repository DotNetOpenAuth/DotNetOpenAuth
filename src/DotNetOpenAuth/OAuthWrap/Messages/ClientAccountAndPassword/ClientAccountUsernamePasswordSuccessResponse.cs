//-----------------------------------------------------------------------
// <copyright file="ClientAccountUsernamePasswordSuccessResponse.cs" company="Andrew Arnott">
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
	internal class ClientAccountUsernamePasswordSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAccountUsernamePasswordSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal ClientAccountUsernamePasswordSuccessResponse(ClientAccountUsernamePasswordRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The token.</value>
		[MessagePart(Protocol.wrap_refresh_token, IsRequired = true, AllowEmpty = false)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart(Protocol.wrap_access_token, IsRequired = true, AllowEmpty = false)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.wrap_access_token_expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }
	}
}
