//-----------------------------------------------------------------------
// <copyright file="IAccessTokenRequestInternal.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Implemented by all message types whose response may contain an access token.
	/// </summary>
	public interface IAccessTokenRequestInternal : IAccessTokenRequest {
		/// <summary>
		/// Gets or sets the result of calling the authorization server host's access token creation method.
		/// </summary>
		IAccessTokenResult AccessTokenResult { get; set; }
	}
}
