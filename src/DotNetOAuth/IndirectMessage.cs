//-----------------------------------------------------------------------
// <copyright file="IndirectMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System.Net;

	/// <summary>
	/// A protocol message that passes between Consumer and Service Provider
	/// via the user agent using a redirect or form POST submission.
	/// </summary>
	/// <remarks>
	/// An <see cref="IndirectMessage"/> instance describes the HTTP response that must
	/// be sent to the user agent to initiate the message transfer.
	/// </remarks>
	public class IndirectMessage {
		/// <summary>
		/// Gets the headers that must be included in the response to the user agent.
		/// </summary>
		/// <remarks>
		/// The headers in this collection are not meant to be a comprehensive list
		/// of exactly what should be sent, but are meant to augment whatever headers
		/// are generally included in a typical response.
		/// </remarks>
		public WebHeaderCollection Headers { get; internal set; }

		/// <summary>
		/// Gets the body of the HTTP response.
		/// </summary>
		public byte[] Body { get; internal set; }

		/// <summary>
		/// Gets the HTTP status code to use in the HTTP response.
		/// </summary>
		public HttpStatusCode Status { get; internal set; }

		/// <summary>
		/// Gets or sets a reference to the actual protocol message that
		/// is being sent via the user agent.
		/// </summary>
		internal IProtocolMessage OriginalMessage { get; set; }

		/// <summary>
		/// Automatically sends the appropriate response to the user agent.
		/// Requires a current HttpContext.
		/// </summary>
		public void Send() {
			throw new System.NotImplementedException();
		}
	}
}
