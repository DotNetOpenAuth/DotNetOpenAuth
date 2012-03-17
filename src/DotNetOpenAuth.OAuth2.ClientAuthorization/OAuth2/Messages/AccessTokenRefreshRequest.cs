//-----------------------------------------------------------------------
// <copyright file="AccessTokenRefreshRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request from the client to the token endpoint for a new access token
	/// in exchange for a refresh token that the client has previously obtained.
	/// </summary>
	internal class AccessTokenRefreshRequest : ScopedAccessTokenRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenRefreshRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The token endpoint.</param>
		/// <param name="version">The version.</param>
		protected AccessTokenRefreshRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// REQUIRED. The refresh token associated with the access token to be refreshed. 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = true)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.RefreshToken; }
		}
	}
}
