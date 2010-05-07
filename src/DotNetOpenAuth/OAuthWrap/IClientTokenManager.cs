//-----------------------------------------------------------------------
// <copyright file="IClientTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Diagnostics.Contracts;

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
		private IClientTokenManagerContract() {
		}

		#region IClientTokenManager Members

		IAuthorizationState IClientTokenManager.GetAuthorizationState(Uri callbackUrl, string clientState) {
			Contract.Requires<ArgumentNullException>(callbackUrl != null);
			throw new NotImplementedException();
		}

		#endregion
	}

	/// <summary>
	/// Provides access to a persistent object that tracks the state of an authorization.
	/// </summary>
	public interface IAuthorizationState {
		/// <summary>
		/// Gets or sets the callback URL used to obtain authorization.
		/// </summary>
		/// <value>The callback URL.</value>
		Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the long-lived token used to renew the short-lived <see cref="AccessToken"/>.
		/// </summary>
		/// <value>The refresh token.</value>
		string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		/// <value>The access token secret.</value>
		string AccessTokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the access token UTC expiration date.
		/// </summary>
		DateTime? AccessTokenExpirationUtc { get; set; }

		/// <summary>
		/// Gets or sets the scope the token is (to be) authorized for.
		/// </summary>
		/// <value>The scope.</value>
		string Scope { get; set; }

		/// <summary>
		/// Deletes this authorization, including access token and refresh token where applicable.
		/// </summary>
		/// <remarks>
		/// This method is invoked when an authorization attempt fails, is rejected, is revoked, or
		/// expires and cannot be renewed.
		/// </remarks>
		void Delete();

		/// <summary>
		/// Saves any changes made to this authorization object's properties.
		/// </summary>
		/// <remarks>
		/// This method is invoked after DotNetOpenAuth changes any property.
		/// </remarks>
		void SaveChanges();
	}
}
