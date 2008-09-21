//-----------------------------------------------------------------------
// <copyright file="MessageScheme.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	/// <summary>
	/// The methods available for the Consumer to send direct messages to the Service Provider.
	/// </summary>
	/// <remarks>
	/// See 1.0 spec section 5.2.
	/// </remarks>
	internal enum MessageScheme {
		/// <summary>
		/// In the HTTP Authorization header as defined in OAuth HTTP Authorization Scheme (OAuth HTTP Authorization Scheme).
		/// </summary>
		AuthorizationHeaderRequest,

		/// <summary>
		/// As the HTTP POST request body with a  content-type  of application/x-www-form-urlencoded.
		/// </summary>
		PostRequest,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		GetRequest,

		/// <summary>
		/// Response parameters are sent by the Service Provider to return Tokens and other information to the Consumer in the HTTP response body. The parameter names and values are first encoded as per Parameter Encoding (Parameter Encoding), and concatenated with the ‘&amp;’ character (ASCII code 38) as defined in [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) Section 2.1.
		/// </summary>
		QueryStyleResponse,
	}
}
