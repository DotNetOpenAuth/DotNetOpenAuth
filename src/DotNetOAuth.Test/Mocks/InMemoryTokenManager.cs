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

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.tokensAndSecrets.Remove(requestToken);
			this.tokensAndSecrets[accessToken] = accessTokenSecret;
		}

		#endregion

		internal void AddConsumer(string key, string secret) {
			this.consumersAndSecrets.Add(key, secret);
		}
	}
}
