//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.ChannelElements;

	internal class InMemoryTokenManager : ITokenManager {
		private Dictionary<string, string> consumersAndSecrets = new Dictionary<string, string>();
		private Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();
		private List<string> authorizedRequestTokens = new List<string>();

		#region ITokenManager Members

		public string GetConsumerSecret(string consumerKey) {
			return this.consumersAndSecrets[consumerKey];
		}

		public string GetTokenSecret(string token) {
			return this.tokensAndSecrets[token];
		}

		public void StoreNewRequestToken(string consumerKey, string requestToken, string requestTokenSecret, IDictionary<string, string> parameters) {
			this.tokensAndSecrets[requestToken] = requestTokenSecret;
		}

		/// <summary>
		/// Checks whether a given request token has already been authorized
		/// by some user for use by the Consumer that requested it.
		/// </summary>
		/// <param name="requestToken">The Consumer's request token.</param>
		/// <returns>
		/// True if the request token has already been fully authorized by the user
		/// who owns the relevant protected resources.  False if the token has not yet
		/// been authorized, has expired or does not exist.
		/// </returns>
		public bool IsRequestTokenAuthorized(string requestToken) {
			return this.authorizedRequestTokens.Contains(requestToken);
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.authorizedRequestTokens.Remove(requestToken);
			this.tokensAndSecrets.Remove(requestToken);
			this.tokensAndSecrets[accessToken] = accessTokenSecret;
		}

		#endregion

		internal void AddConsumer(string key, string secret) {
			this.consumersAndSecrets.Add(key, secret);
		}

		internal void AuthorizedRequestToken(string requestToken) {
			if (requestToken == null) {
				throw new ArgumentNullException("requestToken");
			}

			this.authorizedRequestTokens.Add(requestToken);
		}
	}
}
