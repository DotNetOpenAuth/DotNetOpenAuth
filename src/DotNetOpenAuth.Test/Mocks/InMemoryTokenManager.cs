//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	internal class InMemoryTokenManager : IConsumerTokenManager, IServiceProviderTokenManager {
		private Dictionary<string, string> consumersAndSecrets = new Dictionary<string, string>();
		private KeyedCollectionDelegate<string, TokenInfo> tokens = new KeyedCollectionDelegate<string, TokenInfo>(t => t.Token);

		/// <summary>
		/// Request tokens that have been issued, and whether they have been authorized yet.
		/// </summary>
		private Dictionary<string, bool> requestTokens = new Dictionary<string, bool>();

		/// <summary>
		/// Access tokens that have been issued and have not yet expired.
		/// </summary>
		private List<string> accessTokens = new List<string>();

		#region IConsumerTokenManager Members

		public string ConsumerKey {
			get { return this.consumersAndSecrets.Keys.Single(); }
		}

		public string ConsumerSecret {
			get { return this.consumersAndSecrets.Values.Single(); }
		}

		#endregion

		#region ITokenManager Members

		public string GetTokenSecret(string token) {
			return this.tokens[token].Secret;
		}

		public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
			this.tokens.Add(new TokenInfo { Token = response.Token, Secret = response.TokenSecret });
			this.requestTokens.Add(response.Token, false);
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
			this.tokens.Remove(requestToken);
			this.tokens.Add(new TokenInfo { Token = accessToken, Secret = accessTokenSecret });
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

		#region IServiceProviderTokenManager Members

		public string GetConsumerSecret(string consumerKey) {
			return this.consumersAndSecrets[consumerKey];
		}

		public void SetRequestTokenVerifier(string requestToken, string verifier) {
			this.tokens[requestToken].Verifier = verifier;
		}

		public string GetRequestTokenVerifier(string requestToken) {
			return this.tokens[requestToken].Verifier;
		}

		public void SetRequestTokenCallback(string requestToken, Uri callback) {
			this.tokens[requestToken].Callback = callback;
		}

		public Uri GetRequestTokenCallback(string requestToken) {
			return this.tokens[requestToken].Callback;
		}

		#endregion

		/// <summary>
		/// Tells a Service Provider's token manager about a consumer and its secret
		/// so that the SP can verify the Consumer's signed messages.
		/// </summary>
		/// <param name="consumerDescription">The consumer description.</param>
		internal void AddConsumer(ConsumerDescription consumerDescription) {
			this.consumersAndSecrets.Add(consumerDescription.ConsumerKey, consumerDescription.ConsumerSecret);
		}

		/// <summary>
		/// Marks an existing token as authorized.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		internal void AuthorizeRequestToken(string requestToken) {
			if (requestToken == null) {
				throw new ArgumentNullException("requestToken");
			}

			this.requestTokens[requestToken] = true;
		}

		private class TokenInfo {
			internal string Token;
			internal string Verifier;
			internal string Secret;
			internal Uri Callback;
		}
	}
}
