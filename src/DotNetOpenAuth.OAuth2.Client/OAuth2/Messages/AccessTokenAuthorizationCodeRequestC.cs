// -----------------------------------------------------------------------
// <copyright file="AccessTokenAuthorizationCodeRequestC.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	internal class AccessTokenAuthorizationCodeRequestC : AccessTokenAuthorizationCodeRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal AccessTokenAuthorizationCodeRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.TokenEndpoint, authorizationServer.Version) {
			Requires.NotNull(authorizationServer, "authorizationServer");
		}

	}
}
