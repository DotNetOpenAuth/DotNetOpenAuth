//-----------------------------------------------------------------------
// <copyright file="AccessTokenResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using Validation;

	/// <summary>
	/// Describes the parameters to be fed into creating a response to an access token request.
	/// </summary>
	public class AccessTokenResult : IAccessTokenResult {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenResult"/> class.
		/// </summary>
		/// <param name="accessToken">The access token to include in this result.</param>
		public AccessTokenResult(AuthorizationServerAccessToken accessToken) {
			Requires.NotNull(accessToken, "accessToken");
			this.AllowRefreshToken = true;
			this.AccessToken = accessToken;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to provide the client with a refresh token, when applicable.
		/// </summary>
		/// <value>The default value is <c>true</c>.</value>
		/// <remarks>>
		/// The refresh token will never be provided when this value is false.
		/// The refresh token <em>may</em> be provided when this value is true.
		/// </remarks>
		public bool AllowRefreshToken { get; set; }

		/// <summary>
		/// Gets the access token.
		/// </summary>
		public AuthorizationServerAccessToken AccessToken { get; private set; }

		/// <summary>
		/// Gets the access token.
		/// </summary>
		AccessToken IAccessTokenResult.AccessToken {
			get { return this.AccessToken; }
		}
	}
}
