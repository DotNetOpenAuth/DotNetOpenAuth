//-----------------------------------------------------------------------
// <copyright file="OpenAuthSecurityManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Web;
	using DotNetOpenAuth.AspNet.Clients;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Manage authenticating with an external OAuth or OpenID provider
	/// </summary>
	public class OpenAuthSecurityManager {
		#region Constants and Fields

		/// <summary>
		/// The provider query string name.
		/// </summary>
		private const string ProviderQueryStringName = "__provider__";

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
		public OpenAuthSecurityManager(HttpContextBase requestContext)
			: this(requestContext, provider: null, dataProvider: null) { }

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
			if (requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}

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
			} else {
				uri = this.requestContext.Request.GetPublicFacingUrl();
			}

			// attach the provider parameter so that we know which provider initiated 
			// the login when user is redirected back to this page
			uri = uri.AttachQueryStringParameter(ProviderQueryStringName, this.authenticationProvider.ProviderName);
			this.authenticationProvider.RequestAuthentication(this.requestContext, uri);
		}

		/// <summary>
		/// Checks if user is successfully authenticated when user is redirected back to this user.
		/// </summary>
		/// <returns>The result of the authentication.</returns>
		public AuthenticationResult VerifyAuthentication() {
			AuthenticationResult result = this.authenticationProvider.VerifyAuthentication(this.requestContext);
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

		/// <summary>
		/// Checks if user is successfully authenticated when user is redirected back to this user.
		/// </summary>
		/// <param name="returnUrl">The return Url which must match exactly the Url passed into RequestAuthentication() earlier.</param>
		/// <returns>
		/// The result of the authentication.
		/// </returns>
		public AuthenticationResult VerifyAuthentication(string returnUrl) {
			Requires.NotNullOrEmpty(returnUrl, "returnUrl");

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
			else {
				return this.VerifyAuthentication();
			}
		}

		#endregion
	}
}