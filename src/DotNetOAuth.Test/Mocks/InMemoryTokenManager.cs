namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.ChannelElements;

	internal class InMemoryTokenManager : ITokenManager {
		Dictionary<string, string> consumersAndSecrets = new Dictionary<string, string>();
		Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();

		#region ITokenManager Members

		public string GetConsumerSecret(string consumerKey) {
			return consumersAndSecrets[consumerKey];
		}

		public string GetTokenSecret(string token) {
			return tokensAndSecrets[token];
		}

		public void StoreNewRequestToken(string consumerKey, string requestToken, string requestTokenSecret, IDictionary<string, string> parameters) {
			tokensAndSecrets[requestToken] = requestTokenSecret;
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			tokensAndSecrets.Remove(requestToken);
			tokensAndSecrets[accessToken] = accessTokenSecret;
		}

		#endregion

		internal void AddConsumer(string key, string secret) {
			consumersAndSecrets.Add(key, secret);
		}
	}
}
