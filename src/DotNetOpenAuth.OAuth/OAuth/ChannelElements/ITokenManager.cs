//-----------------------------------------------------------------------
// <copyright file="ITokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// An interface OAuth hosts must implement for persistent storage
	/// and recall of tokens and secrets for an individual OAuth consumer
	/// or service provider.
	/// </summary>
	public interface ITokenManager {
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
		/// <remarks>
		/// Request tokens stored by this method SHOULD NOT associate any user account with this token.
		/// It usually opens up security holes in your application to do so.  Instead, you associate a user
		/// account with access tokens (not request tokens) in the <see cref="ExpireRequestTokenAndStoreNewAccessToken"/>
		/// method.
		/// </remarks>
		void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response);

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		/// <remarks>
		/// <para>
		/// Any scope of granted privileges associated with the request token from the
		/// original call to <see cref="StoreNewRequestToken"/> should be carried over
		/// to the new Access Token.
		/// </para>
		/// <para>
		/// To associate a user account with the new access token, 
		/// <see cref="System.Web.HttpContext.User">HttpContext.Current.User</see> may be
		/// useful in an ASP.NET web application within the implementation of this method.
		/// Alternatively you may store the access token here without associating with a user account,
		/// and wait until WebConsumer.ProcessUserAuthorization or
		/// DesktopConsumer.ProcessUserAuthorization return the access
		/// token to associate the access token with a user account at that point.
		/// </para>
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
