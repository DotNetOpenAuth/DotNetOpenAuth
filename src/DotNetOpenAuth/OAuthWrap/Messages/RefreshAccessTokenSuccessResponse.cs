//-----------------------------------------------------------------------
// <copyright file="RefreshAccessTokenSuccessResponse.cs" company="Andrew Arnott">
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
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class RefreshAccessTokenSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshAccessTokenSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal RefreshAccessTokenSuccessResponse(RefreshAccessTokenRequest request)
			: base(request) {
		}

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
		internal TimeSpan Lifetime { get; set; }
	}
}
