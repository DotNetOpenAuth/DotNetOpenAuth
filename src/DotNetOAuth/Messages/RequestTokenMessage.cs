//-----------------------------------------------------------------------
// <copyright file="RequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a token.
	/// </summary>
	internal class RequestTokenMessage : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal RequestTokenMessage(ServiceProviderEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }
	}
}
