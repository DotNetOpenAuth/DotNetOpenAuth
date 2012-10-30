//-----------------------------------------------------------------------
// <copyright file="IAccessTokenResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Describes the parameters to be fed into creating a response to an access token request.
	/// </summary>
	public interface IAccessTokenResult {
		/// <summary>
		/// Gets or sets a value indicating whether to provide the client with a refresh token, when applicable.
		/// </summary>
		/// <value>The default value is <c>true</c>.</value>
		/// <remarks>>
		/// The refresh token will never be provided when this value is false.
		/// The refresh token <em>may</em> be provided when this value is true.
		/// </remarks>
		bool AllowRefreshToken { get; set; }

		/// <summary>
		/// Gets the access token.
		/// </summary>
		AccessToken AccessToken { get; }
	}
}
