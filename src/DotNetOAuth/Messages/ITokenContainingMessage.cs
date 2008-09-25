//-----------------------------------------------------------------------
// <copyright file="ITokenContainingMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	interface ITokenContainingMessage {
		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string Token { get; set; }
	}
}
