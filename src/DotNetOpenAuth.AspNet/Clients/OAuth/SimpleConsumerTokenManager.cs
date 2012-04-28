//-----------------------------------------------------------------------
// <copyright file="SimpleConsumerTokenManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// Simple wrapper around IConsumerTokenManager
	/// </summary>
	public class SimpleConsumerTokenManager : IConsumerTokenManager {
		/// <summary>
		/// Store the token manager.
		/// </summary>
		private readonly IOAuthTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleConsumerTokenManager"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		/// <param name="tokenManager">The OAuth token manager.</param>
		public SimpleConsumerTokenManager(string consumerKey, string consumerSecret, IOAuthTokenManager tokenManager) {
			Requires.NotNullOrEmpty(consumerKey, "consumerKey");
			Requires.NotNullOrEmpty(consumerSecret, "consumerSecret");
			Requires.NotNull(tokenManager, "oAuthTokenManager");

			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.tokenManager = tokenManager;
		}

		/// <summary>
		/// Gets the consumer key.
		/// </summary>
		/// <value>
		/// The consumer key.
		/// </value>
		public string ConsumerKey {
			get;
			private set;
		}

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>
		/// The consumer secret.
		/// </value>
		public string ConsumerSecret {
			get;
			private set;
		}

		/// <summary>
		/// Gets the Token Secret given a request or access token.
		/// </summary>
		/// <param name="token">The request or access token.</param>
		/// <returns>
		/// The secret associated with the given token.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown if the secret cannot be found for the given token.</exception>
		public string GetTokenSecret(string token) {
			return this.tokenManager.GetTokenSecret(token);
		}

		/// <summary>
		/// Stores a newly generated unauthorized request token, secret, and optional
		/// application-specific parameters for later recall.
		/// </summary>
		/// <param name="request">The request message that resulted in the generation of a new unauthorized request token.</param>
		/// <param name="response">The response message that includes the unauthorized request token.</param>
		/// <exception cref="ArgumentException">Thrown if the consumer key is not registered, or a required parameter was not found in the parameters collection.</exception>
		public void StoreNewRequestToken(DotNetOpenAuth.OAuth.Messages.UnauthorizedTokenRequest request, DotNetOpenAuth.OAuth.Messages.ITokenSecretContainingMessage response) {
			this.tokenManager.StoreRequestToken(response.Token, response.TokenSecret);
		}

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.tokenManager.ReplaceRequestTokenWithAccessToken(requestToken, accessToken, accessTokenSecret);
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>
		/// Request or Access token, or invalid if the token is not recognized.
		/// </returns>
		public TokenType GetTokenType(string token) {
			throw new NotSupportedException();
		}
	}
}