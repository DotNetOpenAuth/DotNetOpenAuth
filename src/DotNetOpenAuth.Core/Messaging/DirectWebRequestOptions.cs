//-----------------------------------------------------------------------
// <copyright file="DirectWebRequestOptions.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Net;

	/// <summary>
	/// A set of flags that can control the behavior of an individual web request.
	/// </summary>
	[Flags]
	public enum DirectWebRequestOptions {
		/// <summary>
		/// Indicates that default <see cref="HttpWebRequest"/> behavior is required.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Indicates that any response from the remote server, even those
		/// with HTTP status codes that indicate errors, should not result
		/// in a thrown exception.
		/// </summary>
		/// <remarks>
		/// Even with this flag set, <see cref="ProtocolException"/> should
		/// be thrown when an HTTP protocol error occurs (i.e. timeouts).
		/// </remarks>
		AcceptAllHttpResponses = 0x1,

		/// <summary>
		/// Indicates that the HTTP request must be completed entirely 
		/// using SSL (including any redirects).
		/// </summary>
		RequireSsl = 0x2,
	}
}
