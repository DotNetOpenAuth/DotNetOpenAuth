//-----------------------------------------------------------------------
// <copyright file="IHttpDirectResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Net;

	/// <summary>
	/// An interface that allows direct response messages to specify
	/// HTTP transport specific properties.
	/// </summary>
	public interface IHttpDirectResponse {
		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		HttpStatusCode HttpStatusCode { get; }

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection Headers { get; }
	}
}
