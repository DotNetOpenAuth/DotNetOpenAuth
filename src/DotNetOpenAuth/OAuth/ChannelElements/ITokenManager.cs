//-----------------------------------------------------------------------
// <copyright file="ITokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// An interface OAuth hosts must implement for persistent storage and recall of tokens and secrets.
	/// </summary>
	public interface ITokenManager {
		/// <summary>
		/// Gets the Consumer Secret given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>The Consumer Secret.</returns>
		/// <exception cref="ArgumentException">Thrown if the consumer key cannot be found.</exception>
		/// <remarks>
		/// TODO: In the case of RSA hashing, the consumer may not have a secret
		/// like this.  What to do in that case?
		/// </remarks>
		string GetConsumerSecret(string consumerKey);

		/// <summary>
		/// Gets the Token Secret given a request or access token.
		/// </summary>
		/// <param name="token">The request or access token.</param>
		/// <returns>The secret associated with the given token.</returns>
		/// <exception cref="ArgumentException">Thrown if the secret cannot be found for the given token.</exception>
		string GetTokenSecret(string token);

		/// <summary>
		/// Stores a newly generated unauthorized request token, secret, and optional
		/// application-specific parameters for later recall.
		/// </summary>
		/// <param name="request">The request message that resulted in the generation of a new unauthorized request token.</param>
		/// <param name="response">The response message that includes the unauthorized request token.</param>
		/// <exception cref="ArgumentException">Thrown if the consumer key is not registered, or a required parameter was not found in the parameters collection.</exception>
		void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response);

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
		bool IsRequestTokenAuthorized(string requestToken);

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		/// <remarks>
		/// Any scope of granted privileges associated with the request token from the
		/// original call to <see cref="StoreNewRequestToken"/> should be carried over
		/// to the new Access Token.
		/// </remarks>
		void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret);

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
		TokenType GetTokenType(string token);
	}
}
