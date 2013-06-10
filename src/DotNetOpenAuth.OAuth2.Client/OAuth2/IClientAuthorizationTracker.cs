//-----------------------------------------------------------------------
// <copyright file="IClientAuthorizationTracker.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using Validation;

	/// <summary>
	/// A token manager implemented by some clients to assist in tracking authorization state.
	/// </summary>
	public interface IClientAuthorizationTracker {
		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>The authorization state; may be <c>null</c> if no authorization state matches.</returns>
		IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState);
	}
}
