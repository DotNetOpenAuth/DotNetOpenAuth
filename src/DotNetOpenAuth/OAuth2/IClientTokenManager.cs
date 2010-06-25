//-----------------------------------------------------------------------
// <copyright file="IClientTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A token manager implemented by some clients to assist in tracking authorization state.
	/// </summary>
	[ContractClass(typeof(IClientTokenManagerContract))]
	public interface IClientTokenManager {
		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>The authorization state; may be <c>null</c> if no authorization state matches.</returns>
		IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState);
	}

	/// <summary>
	/// Contract class for the <see cref="IClientTokenManager"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IClientTokenManager))]
	internal abstract class IClientTokenManagerContract : IClientTokenManager {
		/// <summary>
		/// Prevents a default instance of the <see cref="IClientTokenManagerContract"/> class from being created.
		/// </summary>
		private IClientTokenManagerContract() {
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
		IAuthorizationState IClientTokenManager.GetAuthorizationState(Uri callbackUrl, string clientState) {
			Contract.Requires<ArgumentNullException>(callbackUrl != null);
			throw new NotImplementedException();
		}

		#endregion
	}
}
