//-----------------------------------------------------------------------
// <copyright file="OAuthTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using System.Security.Cryptography.X509Certificates;
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// The token manager this web site uses in its roles both as
	/// a consumer and as a service provider.
	/// </summary>
	public class OAuthTokenManager : IConsumerTokenManager, IServiceProviderTokenManager {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthTokenManager"/> class
		/// for use as a Consumer.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		private OAuthTokenManager(string consumerKey, string consumerSecret) {
			if (String.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}
			if (consumerSecret == null) {
				throw new ArgumentNullException("consumerSecret");
			}

			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthTokenManager"/> class.
		/// </summary>
		private OAuthTokenManager() {
		}

		#region IConsumerTokenManager Members

		/// <summary>
		/// Gets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		public string ConsumerKey { get; private set; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		public string ConsumerSecret { get; private set; }

		#endregion

		#region IServiceProviderTokenManager Members

		/// <summary>
		/// Gets the Consumer description for a given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>
		/// A description of the consumer.  Never null.
		/// </returns>
		/// <exception cref="KeyNotFoundException">Thrown if the consumer key cannot be found.</exception>
		public IConsumerDescription GetConsumer(string consumerKey) {
			try {
				return Global.DataContext.Consumer.First(c => c.ConsumerKey == consumerKey);
			} catch (InvalidOperationException) {
				throw new KeyNotFoundException();
			}
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
			return Global.DataContext.IssuedToken.Any(
				t => t.Token == requestToken && !t.IsAccessToken && t.User != null);
		}

		/// <summary>
		/// Gets details on the named request token.
		/// </summary>
		/// <param name="token">The request token.</param>
		/// <returns>A description of the token.  Never null.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the token cannot be found.</exception>
		/// <remarks>
		/// It is acceptable for implementations to find the token, see that it has expired,
		/// delete it from the database and then throw <see cref="KeyNotFoundException"/>,
		/// or alternatively it can return the expired token anyway and the OAuth channel will
		/// log and throw the appropriate error.
		/// </remarks>
		public IServiceProviderRequestToken GetRequestToken(string token) {
			try {
				return Global.DataContext.IssuedToken.First(tok => !tok.IsAccessToken && tok.Token == token);
			} catch (InvalidOperationException) {
				throw new KeyNotFoundException();
			}
		}

		/// <summary>
		/// Gets details on the named access token.
		/// </summary>
		/// <param name="token">The access token.</param>
		/// <returns>A description of the token.  Never null.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the token cannot be found.</exception>
		/// <remarks>
		/// It is acceptable for implementations to find the token, see that it has expired,
		/// delete it from the database and then throw <see cref="KeyNotFoundException"/>,
		/// or alternatively it can return the expired token anyway and the OAuth channel will
		/// log and throw the appropriate error.
		/// </remarks>
		public IServiceProviderAccessToken GetAccessToken(string token) {
			try {
				return Global.DataContext.IssuedToken.First(tok => tok.IsAccessToken && tok.Token == token);
			} catch (InvalidOperationException) {
				throw new KeyNotFoundException();
			}
		}

		/// <summary>
		/// Persists any changes made to the token.
		/// </summary>
		/// <param name="token">The token whose properties have been changed.</param>
		/// <remarks>
		/// This library will invoke this method after making a set
		/// of changes to the token as part of a web request to give the host
		/// the opportunity to persist those changes to a database.
		/// Depending on the object persistence framework the host site uses,
		/// this method MAY not need to do anything (if changes made to the token
		/// will automatically be saved without any extra handling).
		/// </remarks>
		public void UpdateToken(IServiceProviderRequestToken token) {
			Global.DataContext.SaveChanges();
		}

		#endregion

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

			var token = new IssuedToken {
				Callback = request.Callback,
				Consumer = consumer,
				CreatedOn = DateTime.Now,
				ExpirationDate = DateTime.Now.AddHours(1),
				Token = response.Token,
				TokenSecret = response.TokenSecret,
			};
			Global.DataContext.AddToIssuedToken(token);
			Global.DataContext.SaveChanges();
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			var token = Global.DataContext.IssuedToken.First(
				t => t.Consumer.ConsumerKey == consumerKey && !t.IsAccessToken && t.Token == requestToken);

			// Repurpose this request token to be our access token.
			token.Token = accessToken;
			token.TokenSecret = accessTokenSecret;
			token.ExpirationDate = null; // currently, our access tokens don't expire
			token.IsAccessToken = true;
			token.VerificationCode = null;
			token.CreatedOn = DateTime.Now;
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
				return tok.IsAccessToken ? TokenType.AccessToken : TokenType.RequestToken;
			}
		}

		#endregion

		/// <summary>
		/// Creates a token manager for use when this web site acts as a consumer of
		/// another OAuth service provider.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		/// <returns>The token manager.</returns>
		internal static IConsumerTokenManager CreateConsumer(string consumerKey, string consumerSecret) {
			if (String.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}
			if (consumerSecret == null) {
				throw new ArgumentNullException("consumerSecret");
			}

			return new OAuthTokenManager(consumerKey, consumerSecret);
		}

		/// <summary>
		/// Creates a token manager suitable for this web site acting as an OAuth service provider.
		/// </summary>
		/// <returns>The token manager.</returns>
		internal static IServiceProviderTokenManager CreateServiceProvider() {
			return new OAuthTokenManager();
		}
	}
}
