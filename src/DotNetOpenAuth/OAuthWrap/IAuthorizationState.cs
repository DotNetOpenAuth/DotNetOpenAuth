//-----------------------------------------------------------------------
// <copyright file="IAuthorizationState.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;

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
		/// Gets or sets the type of the access token secret.
		/// </summary>
		/// <value>The type of the access token secret.</value>
		string AccessTokenSecretType { get; set; }

		/// <summary>
		/// Gets or sets the access token issue date UTC.
		/// </summary>
		/// <value>The access token issue date UTC.</value>
		DateTime? AccessTokenIssueDateUtc { get; set; }

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