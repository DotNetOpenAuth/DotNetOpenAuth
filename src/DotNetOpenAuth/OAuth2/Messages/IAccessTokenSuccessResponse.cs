//-----------------------------------------------------------------------
// <copyright file="IAccessTokenSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A response message from the authorization server to the client that contains an access token
	/// and possibly a refresh token
	/// </summary>
	internal interface IAccessTokenSuccessResponse {
		/// <summary>
		/// Gets the access token.
		/// </summary>
		/// <value>The access token.</value>
		string AccessToken { get; }

		/// <summary>
		/// Gets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		string RefreshToken { get; }

		/// <summary>
		/// Gets the lifetime.
		/// </summary>
		/// <value>The lifetime.</value>
		TimeSpan? Lifetime { get; }

		/// <summary>
		/// Gets the scope.
		/// </summary>
		/// <value>The scope.</value>
		string Scope { get; }
	}
}
