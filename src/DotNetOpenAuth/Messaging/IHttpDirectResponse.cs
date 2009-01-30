//-----------------------------------------------------------------------
// <copyright file="IHttpDirectResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// Gets the HTTP status code that the direct respones should be sent with.
		/// </summary>
		HttpStatusCode HttpStatusCode { get; }
	}
}
