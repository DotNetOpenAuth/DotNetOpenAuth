//-----------------------------------------------------------------------
// <copyright file="IAuthenticationClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Web;

	/// <summary>
	/// Represents a client which can authenticate users via an external website/provider.
	/// </summary>
	public interface IAuthenticationClient {
		/// <summary>
		/// Gets the name of the provider which provides authentication service.
		/// </summary>
		string ProviderName { get; }

		/// <summary>
		/// Attempts to authenticate users by forwarding them to an external website, and upon succcess or failure, redirect users back to the specified url.
		/// </summary>
		/// <param name="context">
		/// The context of the current request. 
		/// </param>
		/// <param name="returnUrl">
		/// The return url after users have completed authenticating against external website. 
		/// </param>
		void RequestAuthentication(HttpContextBase context, Uri returnUrl);

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">
		/// The context of the current request. 
		/// </param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult"/> containing authentication result. 
		/// </returns>
		AuthenticationResult VerifyAuthentication(HttpContextBase context);
	}
}
