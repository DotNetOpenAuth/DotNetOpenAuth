//-----------------------------------------------------------------------
// <copyright file="RequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a token.
	/// </summary>
	public class RequestTokenMessage : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal RequestTokenMessage(MessageReceivingEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}
	}
}
