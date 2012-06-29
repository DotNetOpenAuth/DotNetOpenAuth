//-----------------------------------------------------------------------
// <copyright file="AuthenticationOnlyCookieOAuthTokenManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Text;
	using System.Web;
	using System.Web.Security;

	/// <summary>
	/// Stores OAuth tokens in the current request's cookie
	/// </summary>
	public class AuthenticationOnlyCookieOAuthTokenManager : IOAuthTokenManager {
		/// <summary>
		/// Key used for token cookie
		/// </summary>
		private const string TokenCookieKey = "OAuthTokenSecret";

		/// <summary>
		/// Primary request context.
		/// </summary>
		private readonly HttpContextBase primaryContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationOnlyCookieOAuthTokenManager"/> class.
		/// </summary>
		public AuthenticationOnlyCookieOAuthTokenManager() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationOnlyCookieOAuthTokenManager"/> class.
		/// </summary>
		/// <param name="context">The current request context.</param>
		public AuthenticationOnlyCookieOAuthTokenManager(HttpContextBase context) {
			this.primaryContext = context;
		}

		/// <summary>
		/// Gets the effective HttpContext object to use.
		/// </summary>
		private HttpContextBase Context {
			get {
				return this.primaryContext ?? new HttpContextWrapper(HttpContext.Current);
			}
		}

		/// <summary>
		/// Gets the token secret from the specified token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>
		/// The token's secret
		/// </returns>
		public string GetTokenSecret(string token) {
			HttpCookie cookie = this.Context.Request.Cookies[TokenCookieKey];
			if (cookie == null || string.IsNullOrEmpty(cookie.Values[token])) {
				return null;
			}
			byte[] cookieBytes = HttpServerUtility.UrlTokenDecode(cookie.Values[token]);
			byte[] clearBytes = MachineKeyUtil.Unprotect(cookieBytes, TokenCookieKey, "Token:" + token);

			string secret = Encoding.UTF8.GetString(clearBytes);
			return secret;
		}

		/// <summary>
		/// Replaces the request token with access token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="accessTokenSecret">The access token secret.</param>
		public void ReplaceRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret) {
			var cookie = new HttpCookie(TokenCookieKey) {
				Value = string.Empty,
				Expires = DateTime.UtcNow.AddDays(-5)
			};
			this.Context.Response.Cookies.Set(cookie);
		}

		/// <summary>
		/// Stores the request token together with its secret.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="requestTokenSecret">The request token secret.</param>
		public void StoreRequestToken(string requestToken, string requestTokenSecret) {
			var cookie = new HttpCookie(TokenCookieKey) {
				HttpOnly = true
			};

			if (FormsAuthentication.RequireSSL) {
				cookie.Secure = true;
			}

			byte[] cookieBytes = Encoding.UTF8.GetBytes(requestTokenSecret);
			var secretBytes = MachineKeyUtil.Protect(cookieBytes, TokenCookieKey, "Token:" + requestToken);
			cookie.Values[requestToken] = HttpServerUtility.UrlTokenEncode(secretBytes);
			this.Context.Response.Cookies.Set(cookie);
		}
	}
}