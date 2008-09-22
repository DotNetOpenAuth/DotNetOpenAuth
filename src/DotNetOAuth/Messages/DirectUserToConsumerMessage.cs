//-----------------------------------------------------------------------
// <copyright file="DirectUserToConsumerMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A message used to redirect the user from a Service Provider to a Consumer's web site.
	/// </summary>
	internal class DirectUserToConsumerMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectUserToConsumerMessage"/> class.
		/// </summary>
		/// <param name="consumer">The URI of the Consumer endpoint to send this message to.</param>
		internal DirectUserToConsumerMessage(Uri consumer)
			: base(MessageProtection.None, MessageTransport.Indirect, consumer) {
		}

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)] // TODO: graph in spec says this is optional, but in text is suggests it is required.
		public string RequestToken { get; set; }
	}
}
