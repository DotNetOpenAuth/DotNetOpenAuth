//-----------------------------------------------------------------------
// <copyright file="OAuth2Client.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Web;

	/// <summary>
	/// Represents the base class for OAuth 2.0 clients
	/// </summary>
	public abstract class OAuth2Client : IAuthenticationClient {
		#region Constants and Fields

		/// <summary>
		/// The provider name.
		/// </summary>
		private readonly string providerName;

		/// <summary>
		/// The return url.
		/// </summary>
		private Uri returnUrl;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2Client"/> class with the specified provider name.
		/// </summary>
		/// <param name="providerName">
		/// Name of the provider. 
		/// </param>
		protected OAuth2Client(string providerName) {
			Requires.NotNull(providerName, "providerName");
			this.providerName = providerName;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the provider which provides authentication service.
		/// </summary>
		public string ProviderName {
			get {
				return this.providerName;
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Attempts to authenticate users by forwarding them to an external website, and upon succcess or failure, redirect users back to the specified url.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <param name="returnUrl">
		/// The return url after users have completed authenticating against external website. 
		/// </param>
		public virtual void RequestAuthentication(HttpContextBase context, Uri returnUrl) {
			Requires.NotNull(context, "context");
			Requires.NotNull(returnUrl, "returnUrl");

			this.returnUrl = returnUrl;

			string redirectUrl = this.GetServiceLoginUrl(returnUrl).AbsoluteUri;
			context.Response.Redirect(redirectUrl, endResponse: true);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult"/> containing authentication result. 
		/// </returns>
		public virtual AuthenticationResult VerifyAuthentication(HttpContextBase context) {
			Requires.NotNull(context, "context");

			string code = context.Request.QueryString["code"];
			if (string.IsNullOrEmpty(code)) {
				return AuthenticationResult.Failed;
			}

			string accessToken = this.QueryAccessToken(this.returnUrl, code);
			if (accessToken == null) {
				return AuthenticationResult.Failed;
			}

			IDictionary<string, string> userData = this.GetUserData(accessToken);
			if (userData == null) {
				return AuthenticationResult.Failed;
			}

			string id = userData["id"];
			string name;

			// Some oAuth providers do not return value for the 'username' attribute. 
			// In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
			if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
				name = id;
			}

			// add the access token to the user data dictionary just in case page developers want to use it
			userData["accesstoken"] = accessToken;

			return new AuthenticationResult(
				isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the full url pointing to the login page for this client. The url should include the specified return url so that when the login completes, user is redirected back to that url.
		/// </summary>
		/// <param name="returnUrl">
		/// The return URL. 
		/// </param>
		/// <returns>
		/// An absolute URL. 
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", 
			Justification = "Login is used more consistently in ASP.Net")]
		protected abstract Uri GetServiceLoginUrl(Uri returnUrl);

		/// <summary>
		/// Given the access token, gets the logged-in user's data. The returned dictionary must include two keys 'id', and 'username'.
		/// </summary>
		/// <param name="accessToken">
		/// The access token of the current user. 
		/// </param>
		/// <returns>
		/// A dictionary contains key-value pairs of user data 
		/// </returns>
		protected abstract IDictionary<string, string> GetUserData(string accessToken);

		/// <summary>
		/// Queries the access token from the specified authorization code.
		/// </summary>
		/// <param name="returnUrl">
		/// The return URL. 
		/// </param>
		/// <param name="authorizationCode">
		/// The authorization code. 
		/// </param>
		/// <returns>
		/// The access token 
		/// </returns>
		protected abstract string QueryAccessToken(Uri returnUrl, string authorizationCode);
		#endregion
	}
}
