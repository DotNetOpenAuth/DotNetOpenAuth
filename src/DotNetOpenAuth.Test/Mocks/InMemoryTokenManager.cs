//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using DotNetOpenAuth.Test.OAuth;

	internal class InMemoryTokenManager : IServiceProviderTokenManager {
		private KeyedCollectionDelegate<string, ConsumerInfo> consumers = new KeyedCollectionDelegate<string, ConsumerInfo>(c => c.Key);
		private KeyedCollectionDelegate<string, TokenInfo> tokens = new KeyedCollectionDelegate<string, TokenInfo>(t => t.Token);

		/// <summary>
		/// Request tokens that have been issued, and whether they have been authorized yet.
		/// </summary>
		private Dictionary<string, bool> requestTokens = new Dictionary<string, bool>();

		/// <summary>
		/// Access tokens that have been issued and have not yet expired.
		/// </summary>
		private List<string> accessTokens = new List<string>();

		#region ITokenManager Members

		public string GetTokenSecret(string token) {
			return this.tokens[token].Secret;
		}

		public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
			this.tokens.Add(new TokenInfo { ConsumerKey = request.ConsumerKey, Token = response.Token, Secret = response.TokenSecret });
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

		public IConsumerDescription GetConsumer(string consumerKey) {
			return this.consumers[consumerKey];
		}

		public IServiceProviderRequestToken GetRequestToken(string token) {
			return this.tokens[token];
		}

		public IServiceProviderAccessToken GetAccessToken(string token) {
			return this.tokens[token];
		}

		public void UpdateToken(IServiceProviderRequestToken token) {
			// Nothing to do here, since we're using Linq To SQL.
		}

		#endregion

		/// <summary>
		/// Tells a Service Provider's token manager about a consumer and its secret
		/// so that the SP can verify the Consumer's signed messages.
		/// </summary>
		/// <param name="consumerDescription">The consumer description.</param>
		internal void AddConsumer(ConsumerDescription consumerDescription) {
			this.consumers.Add(new ConsumerInfo { Key = consumerDescription.ConsumerKey, Secret = consumerDescription.ConsumerSecret });
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

		private class TokenInfo : IServiceProviderRequestToken, IServiceProviderAccessToken {
			internal TokenInfo() {
				this.CreatedOn = DateTime.Now;
			}

			public string ConsumerKey { get; set; }

			public DateTime CreatedOn { get; set; }

			public string Token { get; set; }

			public string VerificationCode { get; set; }

			public Uri Callback { get; set; }

			public Version ConsumerVersion { get; set; }

			public string Username { get; set; }

			public string[] Roles { get; set; }

			public DateTime? ExpirationDate { get; set; }

			internal string Secret { get; set; }
		}

		private class ConsumerInfo : IConsumerDescription {
			#region IConsumerDescription Members

			public string Key { get; set; }

			public string Secret { get; set; }

			public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate { get; set; }

			public Uri Callback { get; set; }

			public DotNetOpenAuth.OAuth.VerificationCodeFormat VerificationCodeFormat { get; set; }

			public int VerificationCodeLength { get; set; }

			#endregion
		}
	}
}
