//-----------------------------------------------------------------------
// <copyright file="OpenAuthAuthenticationTicketHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Diagnostics;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.AspNet.Resources;

	/// <summary>
	/// Helper methods for setting and retrieving a custom forms authentication ticket for delegation protocols.
	/// </summary>
	internal static class OpenAuthAuthenticationTicketHelper {
		#region Constants and Fields

		/// <summary>
		/// The open auth cookie token.
		/// </summary>
		private const string OpenAuthCookieToken = "OpenAuth";

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// The is valid authentication ticket.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <returns>
		/// The is valid authentication ticket.
		/// </returns>
		public static bool IsValidAuthenticationTicket(HttpContextBase context) {
			HttpCookie cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
			if (cookie == null) {
				return false;
			}

			string encryptedCookieData = cookie.Value;
			if (string.IsNullOrEmpty(encryptedCookieData)) {
				return false;
			}

			try {
				FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(encryptedCookieData);
				return authTicket != null && !authTicket.Expired && authTicket.UserData == OpenAuthCookieToken;
			} catch (ArgumentException) {
				return false;
			}
		}

		/// <summary>
		/// The set authentication ticket.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <param name="userName">
		/// The user name.
		/// </param>
		/// <param name="createPersistentCookie">
		/// The create persistent cookie.
		/// </param>
		/// <exception cref="HttpException">
		/// </exception>
		public static void SetAuthenticationTicket(HttpContextBase context, string userName, bool createPersistentCookie) {
			if (!context.Request.IsSecureConnection && FormsAuthentication.RequireSSL) {
				throw new HttpException(WebResources.ConnectionNotSecure);
			}

			HttpCookie cookie = GetAuthCookie(userName, createPersistentCookie);
			context.Response.Cookies.Add(cookie);
		}

		#endregion

		#region Methods

		/// <summary>
		/// The get auth cookie.
		/// </summary>
		/// <param name="userName">
		/// The user name.
		/// </param>
		/// <param name="createPersistentCookie">
		/// The create persistent cookie.
		/// </param>
		/// <returns>
		/// </returns>
		/// <exception cref="HttpException">
		/// </exception>
		private static HttpCookie GetAuthCookie(string userName, bool createPersistentCookie) {
			Debug.Assert(!string.IsNullOrEmpty(userName));

			var ticket = new FormsAuthenticationTicket(
				/* version */
				2, 
				userName, 
				DateTime.Now, 
				DateTime.Now.Add(FormsAuthentication.Timeout), 
				createPersistentCookie, 
				OpenAuthCookieToken, 
				FormsAuthentication.FormsCookiePath);

			string encryptedTicket = FormsAuthentication.Encrypt(ticket);
			if (encryptedTicket == null || encryptedTicket.Length < 1) {
				throw new HttpException(WebResources.FailedToEncryptTicket);
			}

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
			{
      HttpOnly = true, Path = FormsAuthentication.FormsCookiePath, Secure = FormsAuthentication.RequireSSL 
   };

			if (FormsAuthentication.CookieDomain != null) {
				cookie.Domain = FormsAuthentication.CookieDomain;
			}

			if (ticket.IsPersistent) {
				cookie.Expires = ticket.Expiration;
			}

			return cookie;
		}

		#endregion
	}
}
