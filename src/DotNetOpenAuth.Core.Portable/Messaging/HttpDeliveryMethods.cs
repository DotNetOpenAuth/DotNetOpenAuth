//-----------------------------------------------------------------------
// <copyright file="HttpDeliveryMethods.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;

	/// <summary>
	/// The methods available for the local party to send messages to a remote party.
	/// </summary>
	/// <remarks>
	/// See OAuth 1.0 spec section 5.2.
	/// </remarks>
	[Flags]
	public enum HttpDeliveryMethods {
		/// <summary>
		/// No HTTP methods are allowed.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// In the HTTP Authorization header as defined in OAuth HTTP Authorization Scheme (OAuth HTTP Authorization Scheme).
		/// </summary>
		AuthorizationHeaderRequest = 0x1,

		/// <summary>
		/// As the HTTP POST request body with a content-type of application/x-www-form-urlencoded.
		/// </summary>
		PostRequest = 0x2,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		GetRequest = 0x4,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		PutRequest = 0x8,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		DeleteRequest = 0x10,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		HeadRequest = 0x20,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		PatchRequest = 0x40,

		/// <summary>
		/// Added to the URLs in the query part (as defined by [RFC3986] (Berners-Lee, T., “Uniform Resource Identifiers (URI): Generic Syntax,” .) section 3).
		/// </summary>
		OptionsRequest = 0x80,

		/// <summary>
		/// The flags that control HTTP verbs.
		/// </summary>
		HttpVerbMask = PostRequest | GetRequest | PutRequest | DeleteRequest | HeadRequest | PatchRequest | OptionsRequest,
	}
}
