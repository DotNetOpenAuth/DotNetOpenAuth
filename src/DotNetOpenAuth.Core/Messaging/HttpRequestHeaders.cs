//-----------------------------------------------------------------------
// <copyright file="HttpRequestHeaders.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Well known HTTP headers.
	/// </summary>
	internal static class HttpRequestHeaders {
		/// <summary>
		/// The Authorization header, which specifies the credentials that the client presents in order to authenticate itself to the server.
		/// </summary>
		internal const string Authorization = "Authorization";

		/// <summary>
		/// The WWW-Authenticate header, which is included in HTTP 401 Unauthorized responses to help the client know which authorization schemes are supported.
		/// </summary>
		internal const string WwwAuthenticate = "WWW-Authenticate";

		/// <summary>
		/// The Content-Type header, which specifies the MIME type of the accompanying body data.
		/// </summary>
		internal const string ContentType = "Content-Type";
	}
}
