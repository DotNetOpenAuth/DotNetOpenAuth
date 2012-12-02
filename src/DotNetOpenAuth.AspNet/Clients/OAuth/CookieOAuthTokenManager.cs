//-----------------------------------------------------------------------
// <copyright file="CookieOAuthTokenManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System.Web;
	using System.Web.Security;

	/// <summary>
	/// Stores OAuth tokens in the current request's cookie.
	/// </summary>
	/// <remarks>
	/// This class is different from the <see cref="AuthenticationOnlyCookieOAuthTokenManager"/> in that 
	/// it also stores the access token after the authentication has succeeded.
	/// </remarks>
	public class CookieOAuthTokenManager : AuthenticationOnlyCookieOAuthTokenManager {
		/// <summary>
		/// Initializes a new instance of the <see cref="CookieOAuthTokenManager"/> class.
		/// </summary>
		public CookieOAuthTokenManager() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CookieOAuthTokenManager"/> class.
		/// </summary>
		/// <param name="context">The current request context.</param>
		public CookieOAuthTokenManager(HttpContextBase context)
			: base(context) {
		}

		/// <summary>
		/// Gets the token secret from the specified token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>
		/// The token's secret
		/// </returns>
		public override string GetTokenSecret(string token) {
			string secret = base.GetTokenSecret(token);
			if (secret != null) {
				return secret;
			}

			// The base class checks for cookies in the Request object. 
			// Here we check in the Response object as well because we 
			// may have set it earlier in the request life cycle.
			HttpCookie cookie = this.Context.Response.Cookies[TokenCookieKey];
			if (cookie == null || string.IsNullOrEmpty(cookie.Values[token])) {
				return null;
			}

			secret = DecodeAndUnprotectToken(token, cookie.Values[token]);
			return secret;
		}

		/// <summary>
		/// Replaces the request token with access token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="accessTokenSecret">The access token secret.</param>
		public override void ReplaceRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret) {
			var cookie = new HttpCookie(TokenCookieKey) {
				HttpOnly = true
			};

			if (FormsAuthentication.RequireSSL) {
				cookie.Secure = true;
			}

			var encryptedToken = ProtectAndEncodeToken(accessToken, accessTokenSecret);
			cookie.Values[accessToken] = encryptedToken;

			this.Context.Response.Cookies.Set(cookie);
		}
	}
}