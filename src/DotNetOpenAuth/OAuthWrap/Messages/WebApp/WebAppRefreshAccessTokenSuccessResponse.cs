//-----------------------------------------------------------------------
// <copyright file="WebAppRefreshAccessTokenSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The direct response message that contains the access token from the Authorization Server
	/// to the Client.
	/// </summary>
	internal class WebAppRefreshAccessTokenSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppRefreshAccessTokenSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppRefreshAccessTokenSuccessResponse(WebAppRefreshAccessTokenRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The token.</value>
		[MessagePart(Protocol.wrap_access_token, IsRequired = true, AllowEmpty = false)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.wrap_access_token_expires_in, IsRequired = false, AllowEmpty = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }
	}
}
