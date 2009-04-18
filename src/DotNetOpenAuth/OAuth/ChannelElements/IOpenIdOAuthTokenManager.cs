//-----------------------------------------------------------------------
// <copyright file="IOpenIdOAuthTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using DotNetOpenAuth.OpenId.Extensions.OAuth;

	/// <summary>
	/// Additional methods an <see cref="ITokenManager"/> implementing class
	/// may implement to support the OpenID+OAuth extension.
	/// </summary>
	public interface IOpenIdOAuthTokenManager {
		/// <summary>
		/// Stores a new request token obtained over an OpenID request.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="authorization">The authorization message carrying the request token and authorized access scope.</param>
		/// <remarks>
		/// The token secret is the empty string.
		/// </remarks>
		void StoreOpenIdAuthorizedRequestToken(string consumerKey, AuthorizationApprovedResponse authorization);
	}
}
