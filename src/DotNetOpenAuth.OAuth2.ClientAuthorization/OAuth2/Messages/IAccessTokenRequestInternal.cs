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
		/// Gets or sets the access token creation parameters.
		/// </summary>
		/// <remarks>
		/// This property's value is set by a binding element in the OAuth 2 channel.
		/// </remarks>
		AccessTokenParameters AccessTokenCreationParameters { get; set; }
	}
}
