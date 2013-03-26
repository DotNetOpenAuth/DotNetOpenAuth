//-----------------------------------------------------------------------
// <copyright file="HttpResponseMessageWithOriginal.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Net;
	using System.Net.Http;

	using Validation;

	/// <summary>
	/// An HttpResponseMessage that includes the original DNOA semantic message as a property.
	/// </summary>
	/// <remarks>
	/// This is used to assist in testing.
	/// </remarks>
	internal class HttpResponseMessageWithOriginal : HttpResponseMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpResponseMessageWithOriginal"/> class.
		/// </summary>
		/// <param name="originalMessage">The original message.</param>
		/// <param name="statusCode">The status code.</param>
		internal HttpResponseMessageWithOriginal(IMessage originalMessage, HttpStatusCode statusCode = HttpStatusCode.OK)
			: base(statusCode) {
			this.OriginalMessage = originalMessage;
			Requires.NotNull(originalMessage, "originalMessage");
		}

		/// <summary>
		/// Gets the original message.
		/// </summary>
		internal IMessage OriginalMessage { get; private set; }
	}
}
