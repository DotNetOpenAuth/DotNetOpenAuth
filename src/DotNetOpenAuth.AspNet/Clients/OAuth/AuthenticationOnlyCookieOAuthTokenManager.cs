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
		protected const string TokenCookieKey = "OAuthTokenSecret";

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
		protected HttpContextBase Context {
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
		public virtual string GetTokenSecret(string token) {
			HttpCookie cookie = this.Context.Request.Cookies[TokenCookieKey];
			if (cookie == null || string.IsNullOrEmpty(cookie.Values[token])) {
				return null;
			}

			string secret = DecodeAndUnprotectToken(token, cookie.Values[token]);
			return secret;
		}

		/// <summary>
		/// Replaces the request token with access token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="accessTokenSecret">The access token secret.</param>
		public virtual void ReplaceRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret) {
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
		public virtual void StoreRequestToken(string requestToken, string requestTokenSecret) {
			var cookie = new HttpCookie(TokenCookieKey) {
				HttpOnly = true
			};

			if (FormsAuthentication.RequireSSL) {
				cookie.Secure = true;
			}

			var encryptedToken = ProtectAndEncodeToken(requestToken, requestTokenSecret);
			cookie.Values[requestToken] = encryptedToken;

			this.Context.Response.Cookies.Set(cookie);
		}

		/// <summary>
		/// Protect and url-encode the specified token secret.
		/// </summary>
		/// <param name="token">The token to be used as a key.</param>
		/// <param name="tokenSecret">The token secret to be protected</param>
		/// <returns>The encrypted and protected string.</returns>
		protected static string ProtectAndEncodeToken(string token, string tokenSecret)
		{
			byte[] cookieBytes = Encoding.UTF8.GetBytes(tokenSecret);
			var secretBytes = MachineKeyUtil.Protect(cookieBytes, TokenCookieKey, "Token:" + token);
			return HttpServerUtility.UrlTokenEncode(secretBytes);
		}

		/// <summary>
		/// Url-decode and unprotect the specified encrypted token string.
		/// </summary>
		/// <param name="token">The token to be used as a key.</param>
		/// <param name="encryptedToken">The encrypted token to be decrypted</param>
		/// <returns>The original token secret</returns>
		protected static string DecodeAndUnprotectToken(string token, string encryptedToken)
		{
			byte[] cookieBytes = HttpServerUtility.UrlTokenDecode(encryptedToken);
			byte[] clearBytes = MachineKeyUtil.Unprotect(cookieBytes, TokenCookieKey, "Token:" + token);
			return Encoding.UTF8.GetString(clearBytes);
		}
	}
}