//-----------------------------------------------------------------------
// <copyright file="OpenAuthSecurityManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.AspNet.Clients;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Manage authenticating with an external OAuth or OpenID provider
	/// </summary>
	public class OpenAuthSecurityManager {
		#region Constants and Fields

		/// <summary>
		/// Purposes string used for protecting the anti-XSRF token.
		/// </summary>
		private const string AntiXsrfPurposeString = "DotNetOpenAuth.AspNet.AntiXsrfToken.v1";

		/// <summary>
		/// The provider query string name.
		/// </summary>
		private const string ProviderQueryStringName = "__provider__";

		/// <summary>
		/// The query string name for session id.
		/// </summary>
		private const string SessionIdQueryStringName = "__sid__";

		/// <summary>
		/// The cookie name for session id.
		/// </summary>
		private const string SessionIdCookieName = "__csid__";

		/// <summary>
		/// The _authentication provider.
		/// </summary>
		private readonly IAuthenticationClient authenticationProvider;

		/// <summary>
		/// The _data provider.
		/// </summary>
		private readonly IOpenAuthDataProvider dataProvider;

		/// <summary>
		/// The _request context.
		/// </summary>
		private readonly HttpContextBase requestContext;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenAuthSecurityManager"/> class.
		/// </summary>
		/// <param name="requestContext">
		/// The request context. 
		/// </param>
		/// <param name="provider">
		/// The provider. 
		/// </param>
		/// <param name="dataProvider">
		/// The data provider. 
		/// </param>
		public OpenAuthSecurityManager(
			HttpContextBase requestContext, IAuthenticationClient provider, IOpenAuthDataProvider dataProvider) {
			Requires.NotNull(requestContext, "requestContext");
			Requires.NotNull(provider, "provider");
			Requires.NotNull(dataProvider, "dataProvider");

			this.requestContext = requestContext;
			this.dataProvider = dataProvider;
			this.authenticationProvider = provider;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether IsAuthenticatedWithOpenAuth.
		/// </summary>
		public bool IsAuthenticatedWithOpenAuth {
			get {
				return this.requestContext.Request.IsAuthenticated
					   && OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(this.requestContext);
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Gets the provider that is responding to an authentication request.
		/// </summary>
		/// <param name="context">
		/// The HTTP request context.
		/// </param>
		/// <returns>
		/// The provider name, if one is available.
		/// </returns>
		public static string GetProviderName(HttpContextBase context) {
			return context.Request.QueryString[ProviderQueryStringName];
		}

		/// <summary>
		/// Checks if the specified provider user id represents a valid account. If it does, log user in.
		/// </summary>
		/// <param name="providerUserId">
		/// The provider user id. 
		/// </param>
		/// <param name="createPersistentCookie">
		/// if set to <c>true</c> create persistent cookie. 
		/// </param>
		/// <returns>
		/// <c>true</c> if the login is successful. 
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login",
			Justification = "Login is used more consistently in ASP.Net")]
		public bool Login(string providerUserId, bool createPersistentCookie) {
			string userName = this.dataProvider.GetUserNameFromOpenAuth(
				this.authenticationProvider.ProviderName, providerUserId);
			if (string.IsNullOrEmpty(userName)) {
				return false;
			}

			OpenAuthAuthenticationTicketHelper.SetAuthenticationTicket(this.requestContext, userName, createPersistentCookie);
			return true;
		}

		/// <summary>
		/// Requests the specified provider to start the authentication by directing users to an external website
		/// </summary>
		/// <param name="returnUrl">
		/// The return url after user is authenticated. 
		/// </param>
		public void RequestAuthentication(string returnUrl) {
			// convert returnUrl to an absolute path
			Uri uri;
			if (!string.IsNullOrEmpty(returnUrl)) {
				uri = UriHelper.ConvertToAbsoluteUri(returnUrl, this.requestContext);
			}
			else {
				uri = this.requestContext.Request.GetPublicFacingUrl();
			}

			// attach the provider parameter so that we know which provider initiated 
			// the login when user is redirected back to this page
			uri = uri.AttachQueryStringParameter(ProviderQueryStringName, this.authenticationProvider.ProviderName);

			// Guard against XSRF attack by injecting session id into the redirect url and response cookie.
			// Upon returning from the external provider, we'll compare the session id value in the query 
			// string and the cookie. If they don't match, we'll reject the request.
			string sessionId = Guid.NewGuid().ToString("N");
			uri = uri.AttachQueryStringParameter(SessionIdQueryStringName, sessionId);

			// The cookie value will be the current username secured against the session id we just created.
			byte[] encryptedCookieBytes = MachineKeyUtil.Protect(Encoding.UTF8.GetBytes(GetUsername(this.requestContext)), AntiXsrfPurposeString, "Token: " + sessionId);

			var xsrfCookie = new HttpCookie(SessionIdCookieName, HttpServerUtility.UrlTokenEncode(encryptedCookieBytes)) {
				HttpOnly = true
			};
			if (FormsAuthentication.RequireSSL) {
				xsrfCookie.Secure = true;
			}
			this.requestContext.Response.Cookies.Add(xsrfCookie);

			// issue the redirect to the external auth provider
			this.authenticationProvider.RequestAuthentication(this.requestContext, uri);
		}

		/// <summary>
		/// Checks if user is successfully authenticated when user is redirected back to this user.
		/// </summary>
		/// <param name="returnUrl">The return Url which must match exactly the Url passed into RequestAuthentication() earlier.</param>
		/// <remarks>
		/// This returnUrl parameter only applies to OAuth2 providers. For other providers, it ignores the returnUrl parameter.
		/// </remarks>
		/// <returns>
		/// The result of the authentication.
		/// </returns>
		public AuthenticationResult VerifyAuthentication(string returnUrl) {
			// check for XSRF attack
			string sessionId;
			bool successful = this.ValidateRequestAgainstXsrfAttack(out sessionId);
			if (!successful) {
				return new AuthenticationResult(
							isSuccessful: false,
							provider: this.authenticationProvider.ProviderName,
							providerUserId: null,
							userName: null,
							extraData: null);
			}

			// Only OAuth2 requires the return url value for the verify authenticaiton step
			OAuth2Client oauth2Client = this.authenticationProvider as OAuth2Client;
			if (oauth2Client != null) {
				// convert returnUrl to an absolute path
				Uri uri;
				if (!string.IsNullOrEmpty(returnUrl)) {
					uri = UriHelper.ConvertToAbsoluteUri(returnUrl, this.requestContext);
				}
				else {
					uri = this.requestContext.Request.GetPublicFacingUrl();
				}

				// attach the provider parameter so that we know which provider initiated
				// the login when user is redirected back to this page
				uri = uri.AttachQueryStringParameter(ProviderQueryStringName, this.authenticationProvider.ProviderName);

				// When we called RequestAuthentication(), we put the sessionId in the returnUrl query string.
				// Hence, we need to put it in the VerifyAuthentication url again to please FB/Microsoft account providers.
				uri = uri.AttachQueryStringParameter(SessionIdQueryStringName, sessionId);

				try {
					AuthenticationResult result = oauth2Client.VerifyAuthentication(this.requestContext, uri);
					if (!result.IsSuccessful) {
						// if the result is a Failed result, creates a new Failed response which has providerName info.
						result = new AuthenticationResult(
							isSuccessful: false,
							provider: this.authenticationProvider.ProviderName,
							providerUserId: null,
							userName: null,
							extraData: null);
					}

					return result;
				}
				catch (HttpException exception) {
					return new AuthenticationResult(exception.GetBaseException(), this.authenticationProvider.ProviderName);
				}
			}
			else {
				return this.authenticationProvider.VerifyAuthentication(this.requestContext);
			}
		}

		/// <summary>
		/// Returns the username of the current logged-in user.
		/// </summary>
		/// <param name="context">The HTTP request context.</param>
		/// <returns>The username, or String.Empty if anonymous.</returns>
		private static string GetUsername(HttpContextBase context) {
			string username = null;
			if (context.User.Identity.IsAuthenticated) {
				username = context.User.Identity.Name;
			}
			return username ?? string.Empty;
		}

		/// <summary>
		/// Validates the request against XSRF attack.
		/// </summary>
		/// <param name="sessionId">The session id embedded in the query string.</param>
		/// <returns>
		///   <c>true</c> if the request is safe. Otherwise, <c>false</c>.
		/// </returns>
		private bool ValidateRequestAgainstXsrfAttack(out string sessionId) {
			sessionId = null;

			// get the session id query string parameter
			string queryStringSessionId = this.requestContext.Request.QueryString[SessionIdQueryStringName];

			// verify that the query string value is a valid guid
			Guid guid;
			if (!Guid.TryParse(queryStringSessionId, out guid)) {
				return false;
			}

			// the cookie value should be the current username secured against this guid
			var cookie = this.requestContext.Request.Cookies[SessionIdCookieName];
			if (cookie == null || string.IsNullOrEmpty(cookie.Value)) {
				return false;
			}

			// extract the username embedded within the cookie
			// if there is any error at all (crypto, malformed, etc.), fail gracefully
			string usernameInCookie = null;
			try {
				byte[] encryptedCookieBytes = HttpServerUtility.UrlTokenDecode(cookie.Value);
				byte[] decryptedCookieBytes = MachineKeyUtil.Unprotect(encryptedCookieBytes, AntiXsrfPurposeString, "Token: " + queryStringSessionId);
				usernameInCookie = Encoding.UTF8.GetString(decryptedCookieBytes);
			}
			catch {
				return false;
			}

			string currentUsername = GetUsername(this.requestContext);
			bool successful = string.Equals(currentUsername, usernameInCookie, StringComparison.OrdinalIgnoreCase);

			if (successful) {
				// be a good citizen, clean up cookie when the authentication succeeds
				var xsrfCookie = new HttpCookie(SessionIdCookieName, string.Empty) {
					HttpOnly = true,
					Expires = DateTime.Now.AddYears(-1)
				};
				this.requestContext.Response.Cookies.Set(xsrfCookie);
			}

			sessionId = queryStringSessionId;
			return successful;
		}

		#endregion
	}
}