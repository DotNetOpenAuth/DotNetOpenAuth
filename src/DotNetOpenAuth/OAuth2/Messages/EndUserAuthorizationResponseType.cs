//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationResponseType.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;

	/// <summary>
	/// An indication of what kind of response the client is requesting from the authorization server
	/// after the user has granted authorized access.
	/// </summary>
	[Flags]
	public enum EndUserAuthorizationResponseTypes {
		/// <summary>
		/// An access token should be returned immediately.
		/// </summary>
		AccessToken = 0x1,

		/// <summary>
		/// An authorization code should be returned, which can later be exchanged for refresh and access tokens.
		/// </summary>
		AuthorizationCode = 0x2,

		/// <summary>
		/// Both an access token and an authorization code should be returned.
		/// </summary>
		Both = AccessToken | AuthorizationCode,
	}
}
