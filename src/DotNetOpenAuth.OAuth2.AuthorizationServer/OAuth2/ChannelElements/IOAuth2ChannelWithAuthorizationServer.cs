//-----------------------------------------------------------------------
// <copyright file="IOAuth2ChannelWithAuthorizationServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	/// <summary>
	/// An interface on an OAuth 2 Authorization Server channel
	/// to expose the host provided authorization server object.
	/// </summary>
	internal interface IOAuth2ChannelWithAuthorizationServer {
		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.</value>
		IAuthorizationServerHost AuthorizationServer { get; }

		/// <summary>
		/// Gets or sets the service that checks whether a granted set of scopes satisfies a required set of scopes.
		/// </summary>
		IScopeSatisfiedCheck ScopeSatisfiedCheck { get; set; }
	}
}
