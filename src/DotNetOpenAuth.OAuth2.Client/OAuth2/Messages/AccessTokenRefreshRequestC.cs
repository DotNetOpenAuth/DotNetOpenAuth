//-----------------------------------------------------------------------
// <copyright file="AccessTokenRefreshRequestC.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A request from the client to the token endpoint for a new access token
	/// in exchange for a refresh token that the client has previously obtained.
	/// </summary>
	internal class AccessTokenRefreshRequestC : AccessTokenRefreshRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenRefreshRequestC"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal AccessTokenRefreshRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.TokenEndpoint, authorizationServer.Version) {
		}
	}
}
