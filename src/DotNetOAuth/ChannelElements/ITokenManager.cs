//-----------------------------------------------------------------------
// <copyright file="ITokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public interface ITokenManager {
		string GetConsumerSecret(string consumerKey);
		string GetTokenSecret(string token);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerKey"></param>
		/// <param name="requestToken"></param>
		/// <param name="requestTokenSecret"></param>
		/// <param name="parameters"></param>
		/// <returns>True if there was no conflict with an existing token.  False if a new token should be generated.</returns>
		void StoreNewRequestToken(string consumerKey, string requestToken, string requestTokenSecret, IDictionary<string, string> parameters);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerKey"></param>
		/// <param name="requestToken"></param>
		/// <param name="accessToken"></param>
		/// <param name="accessTokenSecret"></param>
		/// <returns>True if there was no conflict with an existing token.  False if a new token should be generated.</returns>
		void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret);
	}
}
