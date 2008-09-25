//-----------------------------------------------------------------------
// <copyright file="ITokenContainingMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	/// <summary>
	/// An interface implemented by all OAuth messages that have a request or access token property.
	/// </summary>
	internal interface ITokenContainingMessage {
		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string Token { get; set; }
	}
}
