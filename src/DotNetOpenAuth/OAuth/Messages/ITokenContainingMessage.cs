//-----------------------------------------------------------------------
// <copyright file="ITokenContainingMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	/// <summary>
	/// An interface implemented by all OAuth messages that have a request or access token property.
	/// </summary>
	public interface ITokenContainingMessage {
		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string Token { get; set; }
	}
}
