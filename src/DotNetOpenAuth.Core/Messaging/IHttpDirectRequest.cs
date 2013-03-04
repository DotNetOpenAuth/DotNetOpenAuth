//-----------------------------------------------------------------------
// <copyright file="IHttpDirectRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Net;

	/// <summary>
	/// An interface that allows direct request messages to capture the details of the HTTP request they arrived on.
	/// </summary>
	public interface IHttpDirectRequest : IMessage {
		/// <summary>
		/// Gets the HTTP headers of the request.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		System.Net.Http.Headers.HttpRequestHeaders Headers { get; }
	}
}
