//-----------------------------------------------------------------------
// <copyright file="RichAppRefreshAccessTokenRequest.cs" company="Andrew Arnott">
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
	/// A message from the Client to the Authorization Server requesting a new Access Token.
	/// </summary>
	internal class RichAppRefreshAccessTokenRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RichAppRefreshAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal RichAppRefreshAccessTokenRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
		}

		/// <summary>
		/// Gets or sets the refresh token that was received in
		/// <see cref="UserNamePasswordSuccessResponse.RefreshToken"/>.
		/// </summary>
		/// <value>The refresh token.</value>
		[MessagePart(Protocol.wrap_refresh_token, IsRequired = true, AllowEmpty = false)]
		internal string RefreshToken { get; set; }
	}
}
