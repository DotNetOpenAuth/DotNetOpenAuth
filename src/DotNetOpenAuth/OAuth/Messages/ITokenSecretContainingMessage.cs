//-----------------------------------------------------------------------
// <copyright file="ITokenSecretContainingMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	/// <summary>
	/// An interface implemented by all OAuth messages that have a request or access token and secret properties.
	/// </summary>
	public interface ITokenSecretContainingMessage : ITokenContainingMessage {
		/// <summary>
		/// Gets or sets the Request or Access Token secret.
		/// </summary>
		string TokenSecret { get; set; }
	}
}
