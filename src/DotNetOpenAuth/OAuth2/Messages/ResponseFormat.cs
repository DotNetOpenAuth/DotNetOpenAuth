//-----------------------------------------------------------------------
// <copyright file="ResponseFormat.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	/// <summary>
	/// The various formats a client can request a response to come in from an authorization server.
	/// </summary>
	public enum ResponseFormat {
		/// <summary>
		/// The response should be JSON encoded.
		/// </summary>
		Json,

		/// <summary>
		/// The response should be XML encoded.
		/// </summary>
		Xml,

		/// <summary>
		/// The response should be URL encoded.
		/// </summary>
		Form,
	}
}
