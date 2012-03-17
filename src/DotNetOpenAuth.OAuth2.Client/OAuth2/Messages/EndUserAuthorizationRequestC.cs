// -----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationRequestC.cs" company="">
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
	internal class EndUserAuthorizationRequestC : EndUserAuthorizationRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal EndUserAuthorizationRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.AuthorizationEndpoint, authorizationServer.Version) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			Requires.True(authorizationServer.Version != null, "authorizationServer");
			Requires.True(authorizationServer.AuthorizationEndpoint != null, "authorizationServer");
		}

	}
}
