//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationRequestC.cs" company="Andrew Arnott">
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
	/// A message sent by a web application Client to the AuthorizationServer
	/// via the user agent to obtain authorization from the user and prepare
	/// to issue an access token to the client if permission is granted.
	/// </summary>
	[Serializable]
	internal class EndUserAuthorizationRequestC : EndUserAuthorizationRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationRequestC"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal EndUserAuthorizationRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.AuthorizationEndpoint, authorizationServer.Version) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			Requires.That(authorizationServer.Version != null, "authorizationServer", "requires Version");
			Requires.That(authorizationServer.AuthorizationEndpoint != null, "authorizationServer", "requires AuthorizationEndpoint");
		}
	}
}
