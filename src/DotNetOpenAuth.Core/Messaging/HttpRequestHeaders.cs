// -----------------------------------------------------------------------
// <copyright file="HttpRequestHeaders.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	internal static class HttpRequestHeaders {
		/// <summary>
		/// The Authorization header, which specifies the credentials that the client presents in order to authenticate itself to the server.
		/// </summary>
		internal const string Authorization = "Authorization";

		/// <summary>
		/// The Content-Type header, which specifies the MIME type of the accompanying body data.
		/// </summary>
		internal const string ContentType = "Content-Type";
	}
}
