//-----------------------------------------------------------------------
// <copyright file="DatabaseTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.ServiceModel;

	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public class DatabaseTokenManager : IServiceProviderTokenManager {
		internal OperationContext OperationContext { get; set; }

		#region IServiceProviderTokenManager

		public IConsumerDescription GetConsumer(string consumerKey) {
			this.ApplyOperationContext();
			var consumerRow = Global.DataContext.OAuthConsumers.SingleOrDefault(
				consumerCandidate => consumerCandidate.ConsumerKey == consumerKey);
			if (consumerRow == null) {
				throw new KeyNotFoundException();
			}

			return consumerRow;
		}

		public IServiceProviderRequestToken GetRequestToken(string token) {
			try {
				this.ApplyOperationContext();
				return Global.DataContext.OAuthTokens.First(t => t.Token == token && t.State != TokenAuthorizationState.AccessToken);
			} catch (InvalidOperationException ex) {
				throw new KeyNotFoundException("Unrecognized token", ex);
			}
		}

		public IServiceProviderAccessToken GetAccessToken(string token) {
			this.ApplyOperationContext();
			try {
				return Global.DataContext.OAuthTokens.First(t => t.Token == token && t.State == TokenAuthorizationState.AccessToken);
			} catch (InvalidOperationException ex) {
				throw new KeyNotFoundException("Unrecognized token", ex);
			}
		}

		public void UpdateToken(IServiceProviderRequestToken token) {
			// Nothing to do here, since we're using Linq To SQL, and
			// We call LinqToSql's SubmitChanges method via our Global.Application_EndRequest method.
			// This is a good pattern because we only save changes if the request didn't end up somehow failing.
			// But if you DO want to save changes at this point, you could do it like so:
			////Global.DataContext.SubmitChanges();
		}

		#endregion

		#region ITokenManager Members

		public string GetTokenSecret(string token) {
			this.ApplyOperationContext();
			var tokenRow = Global.DataContext.OAuthTokens.SingleOrDefault(
				tokenCandidate => tokenCandidate.Token == token);
			if (tokenRow == null) {
				throw new ArgumentException();
			}

			return tokenRow.TokenSecret;
		}

		public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
			this.ApplyOperationContext();
			RequestScopedTokenMessage scopedRequest = (RequestScopedTokenMessage)request;
			var consumer = Global.DataContext.OAuthConsumers.Single(consumerRow => consumerRow.ConsumerKey == request.ConsumerKey);
			string scope = scopedRequest.Scope;
			OAuthToken newToken = new OAuthToken {
				OAuthConsumer = consumer,
				Token = response.Token,
				TokenSecret = response.TokenSecret,
				IssueDate = DateTime.UtcNow,
				Scope = scope,
			};

			Global.DataContext.OAuthTokens.InsertOnSubmit(newToken);
			Global.DataContext.SubmitChanges();
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
			this.ApplyOperationContext();
			var tokenFound = Global.DataContext.OAuthTokens.SingleOrDefault(
				token => token.Token == requestToken &&
				token.State == TokenAuthorizationState.AuthorizedRequestToken);
			return tokenFound != null;
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.ApplyOperationContext();

			var data = Global.DataContext;
			var consumerRow = data.OAuthConsumers.Single(consumer => consumer.ConsumerKey == consumerKey);
			var tokenRow = data.OAuthTokens.Single(token => token.Token == requestToken && token.OAuthConsumer == consumerRow);
			Debug.Assert(tokenRow.State == TokenAuthorizationState.AuthorizedRequestToken, "The token should be authorized already!");

			// Update the existing row to be an access token.
			tokenRow.IssueDate = DateTime.UtcNow;
			tokenRow.State = TokenAuthorizationState.AccessToken;
			tokenRow.Token = accessToken;
			tokenRow.TokenSecret = accessTokenSecret;
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>Request or Access token, or invalid if the token is not recognized.</returns>
		public TokenType GetTokenType(string token) {
			this.ApplyOperationContext();
			var tokenRow = Global.DataContext.OAuthTokens.SingleOrDefault(tokenCandidate => tokenCandidate.Token == token);
			if (tokenRow == null) {
				return TokenType.InvalidToken;
			} else if (tokenRow.State == TokenAuthorizationState.AccessToken) {
				return TokenType.AccessToken;
			} else {
				return TokenType.RequestToken;
			}
		}

		#endregion

		public void AuthorizeRequestToken(string requestToken, User user) {
			if (requestToken == null) {
				throw new ArgumentNullException("requestToken");
			}
			if (user == null) {
				throw new ArgumentNullException("user");
			}

			this.ApplyOperationContext();
			var tokenRow = Global.DataContext.OAuthTokens.SingleOrDefault(
				tokenCandidate => tokenCandidate.Token == requestToken &&
				tokenCandidate.State == TokenAuthorizationState.UnauthorizedRequestToken);
			if (tokenRow == null) {
				throw new ArgumentException();
			}

			tokenRow.State = TokenAuthorizationState.AuthorizedRequestToken;
			tokenRow.User = user;
		}

		public OAuthConsumer GetConsumerForToken(string token) {
			if (string.IsNullOrEmpty(token)) {
				throw new ArgumentNullException("requestToken");
			}

			this.ApplyOperationContext();
			var tokenRow = Global.DataContext.OAuthTokens.SingleOrDefault(
				tokenCandidate => tokenCandidate.Token == token);
			if (tokenRow == null) {
				throw new ArgumentException();
			}

			return tokenRow.OAuthConsumer;
		}

		private void ApplyOperationContext() {
			if (this.OperationContext != null && OperationContext.Current == null) {
				OperationContext.Current = this.OperationContext;
			}
		}
	}
}