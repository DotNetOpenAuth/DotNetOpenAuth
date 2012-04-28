//-----------------------------------------------------------------------
// <copyright file="IOAuthTokenManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	/// <summary>
	/// A token manager for use by a web site in its role as a consumer of
	/// an individual ServiceProvider.
	/// </summary>
	/// <remarks>
	/// This interface is used by clients of the DotNetOpenAuth.AspNet classes.
	/// </remarks>
	public interface IOAuthTokenManager {
		/// <summary>
		/// Gets the token secret from the specified token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The token's secret</returns>
		string GetTokenSecret(string token);

		/// <summary>
		/// Stores the request token together with its secret.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="requestTokenSecret">The request token secret.</param>
		void StoreRequestToken(string requestToken, string requestTokenSecret);

		/// <summary>
		/// Replaces the request token with access token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="accessTokenSecret">The access token secret.</param>
		void ReplaceRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret);
	}
}