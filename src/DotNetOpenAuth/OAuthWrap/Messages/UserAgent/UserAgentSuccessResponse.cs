//-----------------------------------------------------------------------
// <copyright file="UserAgentSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class UserAgentSuccessResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The version.</param>
		internal UserAgentSuccessResponse(Uri clientCallback, Version version)
			: base (version, MessageTransport.Indirect, clientCallback)
		{
		}

		[MessagePart(Protocol.access_token, IsRequired = true, AllowEmpty = false)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// OPTIONAL. The refresh token used to obtain new access tokens using the same end-user access grant as described in Section 6  (Refreshing an Access Token). 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = false, AllowEmpty = false)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		/// <value>The access token secret.</value>
		/// <remarks>
		/// REQUIRED if requested by the client. The corresponding access token secret as requested by the client. 
		/// </remarks>
		[MessagePart(Protocol.access_token_secret, IsRequired = false, AllowEmpty = false)]
		internal string AccessTokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>The state.</value>
		/// <remarks>
		/// REQUIRED if the state parameter was present in the client authorization request. Set to the exact value received from the client. 
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }
	}
}
