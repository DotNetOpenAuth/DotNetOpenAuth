//-----------------------------------------------------------------------
// <copyright file="InMemoryTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;

	/// <summary>
	/// A simple in-memory token manager.  JUST FOR PURPOSES OF KEEPING THE SAMPLE SIMPLE.
	/// </summary>
	/// <remarks>
	/// This is merely a sample app.  A real web app SHOULD NEVER store a memory-only
	/// token manager in application.  It should be an IServiceProviderTokenManager
	/// implementation that is bound to a database.
	/// </remarks>
	public class InMemoryTokenManager : IServiceProviderTokenManager, IOpenIdOAuthTokenManager, ICombinedOpenIdProviderTokenManager {
		private Dictionary<string, InMemoryServiceProviderRequestToken> requestTokens = new Dictionary<string, InMemoryServiceProviderRequestToken>();
		private Dictionary<string, InMemoryServiceProviderAccessToken> accessTokens = new Dictionary<string, InMemoryServiceProviderAccessToken>();

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTokenManager"/> class.
		/// </summary>
		internal InMemoryTokenManager() {
		}

		#region IServiceProviderTokenManager Members

		public IConsumerDescription GetConsumer(string consumerKey) {
			return new InMemoryConsumerDescription {
				Key = consumerKey,
				Secret = "some crazy secret",
			};
		}

		public IServiceProviderRequestToken GetRequestToken(string token) {
			return this.requestTokens[token];
		}

		public IServiceProviderAccessToken GetAccessToken(string token) {
			throw new NotImplementedException();
		}

		public void UpdateToken(IServiceProviderRequestToken token) {
			// Nothing to do here, since there's no database in this sample.
		}

		#endregion

		#region ITokenManager Members

		public string GetTokenSecret(string token) {
			if (this.requestTokens.ContainsKey(token)) {
				return this.requestTokens[token].Secret;
			} else {
				return this.accessTokens[token].Secret;
			}
		}

		public void StoreNewRequestToken(DotNetOpenAuth.OAuth.Messages.UnauthorizedTokenRequest request, DotNetOpenAuth.OAuth.Messages.ITokenSecretContainingMessage response) {
			throw new NotImplementedException();
		}

		public bool IsRequestTokenAuthorized(string requestToken) {
			// In OpenID+OAuth scenarios, request tokens are always authorized.
			return true;
		}

		public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			this.requestTokens.Remove(requestToken);
			this.accessTokens[accessToken] = new InMemoryServiceProviderAccessToken {
				Token = accessToken,
				Secret = accessTokenSecret,
			};
		}

		public TokenType GetTokenType(string token) {
			if (this.requestTokens.ContainsKey(token)) {
				return TokenType.RequestToken;
			} else if (this.accessTokens.ContainsKey(token)) {
				return TokenType.AccessToken;
			} else {
				return TokenType.InvalidToken;
			}
		}

		#endregion

		#region IOpenIdOAuthTokenManager Members

		public void StoreOpenIdAuthorizedRequestToken(string consumerKey, AuthorizationApprovedResponse authorization) {
			this.requestTokens[authorization.RequestToken] = new InMemoryServiceProviderRequestToken {
				Token = authorization.RequestToken,
				Scope = authorization.Scope,
				ConsumerVersion = authorization.Version,
			};
		}

		#endregion

		#region ICombinedOpenIdProviderTokenManager Members

		public string GetConsumerKey(DotNetOpenAuth.OpenId.Realm realm) {
			// We just use the realm as the consumer key, like Google does.
			return realm;
		}

		#endregion
	}
}
