//-----------------------------------------------------------------------
// <copyright file="OAuthTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Code {
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
				return Global.DataContext.IssuedToken.First(t => t.Token == token).TokenSecret;
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
				consumer = Global.DataContext.Consumer.First(c => c.ConsumerKey == request.ConsumerKey);
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException();
			}

			var token = new IssuedRequestToken {
				Callback = request.Callback,
				Consumer = consumer,
				CreatedOn = DateTime.Now,
				Token = response.Token,
				TokenSecret = response.TokenSecret,
			};
			Global.DataContext.AddToIssuedToken(token);
			Global.DataContext.SaveChanges();
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			var requestTokenEntity = Global.DataContext.IssuedToken.OfType<IssuedRequestToken>().First(
				t => t.Consumer.ConsumerKey == consumerKey && t.Token == requestToken);
			Global.DataContext.DeleteObject(requestTokenEntity);

			var accessTokenEntity = new IssuedAccessToken {
				Token = accessToken,
				TokenSecret = accessTokenSecret,
				ExpirationDate = null, // currently, our access tokens don't expire
				CreatedOn = DateTime.Now,
			};

			Global.DataContext.AddToIssuedToken(accessTokenEntity);
			Global.DataContext.SaveChanges();
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>
		/// Request or Access token, or invalid if the token is not recognized.
		/// </returns>
		public TokenType GetTokenType(string token) {
			IssuedToken tok = Global.DataContext.IssuedToken.FirstOrDefault(t => t.Token == token);
			if (tok == null) {
				return TokenType.InvalidToken;
			} else {
				return tok is IssuedAccessToken ? TokenType.AccessToken : TokenType.RequestToken;
			}
		}

		#endregion
	}
}
