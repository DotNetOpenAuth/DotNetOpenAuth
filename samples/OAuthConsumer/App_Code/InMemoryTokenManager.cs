//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;

public class InMemoryTokenManager : IConsumerTokenManager {
	private Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();

	public InMemoryTokenManager(string consumerKey, string consumerSecret) {
		if (String.IsNullOrEmpty(consumerKey)) {
			throw new ArgumentNullException("consumerKey");
		}

		this.ConsumerKey = consumerKey;
		this.ConsumerSecret = consumerSecret;
	}

	public string ConsumerKey { get; private set; }

	public string ConsumerSecret { get; private set; }

	#region ITokenManager Members

	public string GetTokenSecret(string token) {
		return this.tokensAndSecrets[token];
	}

	public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
		this.tokensAndSecrets[response.Token] = response.TokenSecret;
	}

	public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
		this.tokensAndSecrets.Remove(requestToken);
		this.tokensAndSecrets[accessToken] = accessTokenSecret;
	}

	/// <summary>
	/// Classifies a token as a request token or an access token.
	/// </summary>
	/// <param name="token">The token to classify.</param>
	/// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
	public TokenType GetTokenType(string token) {
		throw new NotImplementedException();
	}

	#endregion
}
