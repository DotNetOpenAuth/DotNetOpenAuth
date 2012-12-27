//-----------------------------------------------------------------------
// <copyright file="AccessTokenAuthorizationCodeRequestC.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Validation;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange an authorization code for an access token,
	/// and (at the authorization server's option) a refresh token.
	/// </summary>
	internal class AccessTokenAuthorizationCodeRequestC : AccessTokenAuthorizationCodeRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequestC"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal AccessTokenAuthorizationCodeRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.TokenEndpoint, authorizationServer.Version) {
			Requires.NotNull(authorizationServer, "authorizationServer");
		}
	}
}
