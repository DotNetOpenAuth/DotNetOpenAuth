// -----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationImplicitRequestC.cs" company="">
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
	public class EndUserAuthorizationImplicitRequestC : EndUserAuthorizationImplicitRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationImplicitRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal EndUserAuthorizationImplicitRequestC(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer.AuthorizationEndpoint, authorizationServer.Version) {
		}

	}
}
