//-----------------------------------------------------------------------
// <copyright file="CookieTemporaryCredentialStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Security;
	using Validation;

	/// <summary>
	/// Provides temporary credential storage by persisting them in a protected cookie on the
	/// user agent (i.e. browser).
	/// </summary>
	public class CookieTemporaryCredentialStorage : ITemporaryCredentialStorage {
		/// <summary>
		/// Key used for token cookie
		/// </summary>
		protected const string TokenCookieKey = "DNOAOAuth1TempCredential";

		/// <summary>
		/// Primary request context.
		/// </summary>
		private readonly HttpContextBase httpContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="CookieTemporaryCredentialStorage"/> class
		/// using <see cref="HttpContext.Current"/> as the source for the context to read and write cookies to.
		/// </summary>
		public CookieTemporaryCredentialStorage()
			: this(new HttpContextWrapper(HttpContext.Current)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CookieTemporaryCredentialStorage"/> class.
		/// </summary>
		/// <param name="httpContext">The HTTP context from and to which to access cookies.</param>
		public CookieTemporaryCredentialStorage(HttpContextBase httpContext) {
			Requires.NotNull(httpContext, "httpContext");
			this.httpContext = httpContext;
		}

		#region ITemporaryCredentialsStorage Members

		/// <summary>
		/// Saves the temporary credential.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="secret">The secret.</param>
		public void SaveTemporaryCredential(string identifier, string secret) {
			var cookie = new HttpCookie(TokenCookieKey) {
				HttpOnly = true
			};

			if (FormsAuthentication.RequireSSL) {
				cookie.Secure = true;
			}

			var encryptedToken = ProtectAndEncodeToken(identifier, secret);
			var escapedIdentifier = Uri.EscapeDataString(identifier);
			cookie.Values[escapedIdentifier] = encryptedToken;

			this.httpContext.Response.Cookies.Set(cookie);
		}

		/// <summary>
		/// Obtains the temporary credential identifier and secret, if available.
		/// </summary>
		/// <returns>
		/// An initialized key value pair if credentials are available; otherwise both key and value are <c>null</c>.
		/// </returns>
		public KeyValuePair<string, string> RetrieveTemporaryCredential() {
			HttpCookie cookie = this.httpContext.Request.Cookies[TokenCookieKey];
			if (cookie == null || cookie.Values.Count == 0) {
				return new KeyValuePair<string, string>();
			}

			string escapedIdentifier = cookie.Values.GetKey(0);
			string identifier = Uri.UnescapeDataString(escapedIdentifier);
			string secret = DecodeAndUnprotectToken(identifier, cookie.Values[escapedIdentifier]);
			return new KeyValuePair<string, string>(identifier, secret);
		}

		/// <summary>
		/// Clears the temporary credentials from storage.
		/// </summary>
		/// <remarks>
		/// DotNetOpenAuth calls this when the credentials are no longer needed.
		/// </remarks>
		public void ClearTemporaryCredential() {
			var cookie = new HttpCookie(TokenCookieKey) {
				Value = string.Empty,
				Expires = DateTime.UtcNow.AddDays(-5),
			};
			this.httpContext.Response.Cookies.Set(cookie);
		}

		#endregion

		/// <summary>
		/// Protect and url-encode the specified token secret.
		/// </summary>
		/// <param name="token">The token to be used as a key.</param>
		/// <param name="tokenSecret">The token secret to be protected</param>
		/// <returns>The encrypted and protected string.</returns>
		protected static string ProtectAndEncodeToken(string token, string tokenSecret) {
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
		protected static string DecodeAndUnprotectToken(string token, string encryptedToken) {
			byte[] cookieBytes = HttpServerUtility.UrlTokenDecode(encryptedToken);
			byte[] clearBytes = MachineKeyUtil.Unprotect(cookieBytes, TokenCookieKey, "Token:" + token);
			return Encoding.UTF8.GetString(clearBytes);
		}
	}
}
