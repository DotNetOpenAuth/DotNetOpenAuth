//-----------------------------------------------------------------------
// <copyright file="IAccessTokenIssuingResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A message sent from the Authorization Server to the client carrying an access token.
	/// </summary>
	internal interface IAccessTokenIssuingResponse : IAccessTokenCarryingRequest {
		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		TimeSpan? Lifetime { get; set; }
	}
}
