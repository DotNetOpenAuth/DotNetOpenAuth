//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationResponseType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;

	/// <summary>
	/// An indication of what kind of response the client is requesting from the authorization server
	/// after the user has granted authorized access.
	/// </summary>
	public enum EndUserAuthorizationResponseType {
		/// <summary>
		/// An access token should be returned immediately.
		/// </summary>
		AccessToken,

		/// <summary>
		/// An authorization code should be returned, which can later be exchanged for refresh and access tokens.
		/// </summary>
		AuthorizationCode,
	}
}
