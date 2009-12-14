//-----------------------------------------------------------------------
// <copyright file="OAuthTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
	using System.Web;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The token manager this web site uses in its roles both as
	/// a consumer and as a service provider.
	/// </summary>
	public class OAuthTokenManager : ITokenManager {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthTokenManager"/> class.
		/// </summary>
		protected OAuthTokenManager() {
		}

		#region ITokenManager Members

		/// <summary>
		/// Gets the Token Secret given a request or access token.
		/// </summary>
		/// <param name="token">The request or access token.</param>
		/// <returns>
		/// The secret associated with the given token.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown if the secret cannot be found for the given token.</exception>
		public string GetTokenSecret(string token) {
			try {
				return Database.DataContext.IssuedTokens.First(t => t.Token == token).TokenSecret;
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException();
			}
		}

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
		public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
			Consumer consumer;
			try {
				consumer = Database.DataContext.Consumers.First(c => c.ConsumerKey == request.ConsumerKey);
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException();
			}

			var token = new IssuedRequestToken {
				Callback = request.Callback,
				Consumer = consumer,
				Token = response.Token,
				TokenSecret = response.TokenSecret,
			};
			string scope;
			if (request.ExtraData.TryGetValue("scope", out scope)) {
				token.Scope = scope;
			}
			Database.DataContext.AddToIssuedTokens(token);
			Database.DataContext.SaveChanges();
		}

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		/// <remarks>
		/// 	<para>
		/// Any scope of granted privileges associated with the request token from the
		/// original call to <see cref="StoreNewRequestToken"/> should be carried over
		/// to the new Access Token.
		/// </para>
		/// 	<para>
		/// To associate a user account with the new access token,
		/// <see cref="System.Web.HttpContext.User">HttpContext.Current.User</see> may be
		/// useful in an ASP.NET web application within the implementation of this method.
		/// Alternatively you may store the access token here without associating with a user account,
		/// and wait until <see cref="WebConsumer.ProcessUserAuthorization()"/> or
		/// <see cref="DesktopConsumer.ProcessUserAuthorization(string, string)"/> return the access
		/// token to associate the access token with a user account at that point.
		/// </para>
		/// </remarks>
		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			var requestTokenEntity = Database.DataContext.IssuedTokens.OfType<IssuedRequestToken>()
				.Include("User")
				.First(t => t.Consumer.ConsumerKey == consumerKey && t.Token == requestToken);

			var accessTokenEntity = new IssuedAccessToken {
				Token = accessToken,
				TokenSecret = accessTokenSecret,
				ExpirationDateUtc = null, // currently, our access tokens don't expire
				User = requestTokenEntity.User,
				Scope = requestTokenEntity.Scope,
				Consumer = requestTokenEntity.Consumer,
			};

			Database.DataContext.DeleteObject(requestTokenEntity);
			Database.DataContext.AddToIssuedTokens(accessTokenEntity);
			Database.DataContext.SaveChanges();
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>
		/// Request or Access token, or invalid if the token is not recognized.
		/// </returns>
		public TokenType GetTokenType(string token) {
			IssuedToken tok = Database.DataContext.IssuedTokens.FirstOrDefault(t => t.Token == token);
			if (tok == null) {
				return TokenType.InvalidToken;
			} else {
				return tok is IssuedAccessToken ? TokenType.AccessToken : TokenType.RequestToken;
			}
		}

		#endregion
	}
}
