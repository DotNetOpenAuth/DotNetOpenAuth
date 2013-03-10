//-----------------------------------------------------------------------
// <copyright file="HttpResponseMessageWithOriginal.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Net;
	using System.Net.Http;

	using Validation;

	internal class HttpResponseMessageWithOriginal : HttpResponseMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpResponseMessageWithOriginal"/> class.
		/// </summary>
		/// <param name="originalMessage">The original message.</param>
		/// <param name="statusCode">The status code.</param>
		internal HttpResponseMessageWithOriginal(IMessage originalMessage, HttpStatusCode statusCode = HttpStatusCode.OK)
			: base(statusCode) {
			OriginalMessage = originalMessage;
			Requires.NotNull(originalMessage, "originalMessage");
		}

		/// <summary>
		/// Gets the original message.
		/// </summary>
		internal IMessage OriginalMessage { get; private set; }
	}
}
