//-----------------------------------------------------------------------
// <copyright file="IClientAuthorizationTracker.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A token manager implemented by some clients to assist in tracking authorization state.
	/// </summary>
	[ContractClass(typeof(IClientAuthorizationTrackerContract))]
	public interface IClientAuthorizationTracker {
		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>The authorization state; may be <c>null</c> if no authorization state matches.</returns>
		IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState);
	}

	/// <summary>
	/// Contract class for the <see cref="IClientAuthorizationTracker"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IClientAuthorizationTracker))]
	internal abstract class IClientAuthorizationTrackerContract : IClientAuthorizationTracker {
		/// <summary>
		/// Prevents a default instance of the <see cref="IClientAuthorizationTrackerContract"/> class from being created.
		/// </summary>
		private IClientAuthorizationTrackerContract() {
		}

		#region IClientTokenManager Members

		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>
		/// The authorization state; may be <c>null</c> if no authorization state matches.
		/// </returns>
		IAuthorizationState IClientAuthorizationTracker.GetAuthorizationState(Uri callbackUrl, string clientState) {
			Requires.NotNull(callbackUrl, "callbackUrl");
			throw new NotImplementedException();
		}

		#endregion
	}
}
