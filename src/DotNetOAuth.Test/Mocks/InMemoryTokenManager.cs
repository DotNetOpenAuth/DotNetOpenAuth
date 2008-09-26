//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using DotNetOAuth.ChannelElements;

	internal class InMemoryTokenManager : ITokenManager {
		private Dictionary<string, string> consumersAndSecrets = new Dictionary<string, string>();
		private Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();

		/// <summary>
		/// Request tokens that have been issued, and whether they have been authorized yet.
		/// </summary>
		private Dictionary<string, bool> requestTokens = new Dictionary<string, bool>();

		/// <summary>
		/// Access tokens that have been issued and have not yet expired.
		/// </summary>
		private List<string> accessTokens = new List<string>();

		#region ITokenManager Members

		public string GetConsumerSecret(string consumerKey) {
			return this.consumersAndSecrets[consumerKey];
		}

		public string GetTokenSecret(string token) {
			return this.tokensAndSecrets[token];
		}

		public void StoreNewRequestToken(string consumerKey, string requestToken, string requestTokenSecret, IDictionary<string, string> parameters) {
			this.tokensAndSecrets[requestToken] = requestTokenSecret;
			this.requestTokens.Add(requestToken, false);
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
			return this.requestTokens[requestToken];
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			// The following line is commented out because consumers don't mark their own tokens
			// as authorized... only the SPs do.  And since we multi-purpose this test class for
			// both SPs and Consumers, we won't do this extra check.
			////Debug.Assert(this.requestTokens[requestToken], "Unauthorized token should not be exchanged for access token.");
			this.requestTokens.Remove(requestToken);
			this.accessTokens.Add(accessToken);
			this.tokensAndSecrets.Remove(requestToken);
			this.tokensAndSecrets[accessToken] = accessTokenSecret;
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
		public TokenType GetTokenType(string token) {
			if (this.requestTokens.ContainsKey(token)) {
				return TokenType.RequestToken;
			} else if (this.accessTokens.Contains(token)) {
				return TokenType.AccessToken;
			} else {
				return TokenType.InvalidToken;
			}
		}

		#endregion

		internal void AddConsumer(string key, string secret) {
			this.consumersAndSecrets.Add(key, secret);
		}

		internal void AuthorizeRequestToken(string requestToken) {
			if (requestToken == null) {
				throw new ArgumentNullException("requestToken");
			}

			this.requestTokens[requestToken] = true;
		}
	}
}
