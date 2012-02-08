//-----------------------------------------------------------------------
// <copyright file="IOpenIdOAuthTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using DotNetOpenAuth.OpenId;
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
		/// <para>The token secret is the empty string.</para>
		/// <para>Tokens stored by this method should be short-lived to mitigate 
		/// possible security threats.  Their lifetime should be sufficient for the
		/// relying party to receive the positive authentication assertion and immediately
		/// send a follow-up request for the access token.</para>
		/// </remarks>
		void StoreOpenIdAuthorizedRequestToken(string consumerKey, AuthorizationApprovedResponse authorization);
	}
}
