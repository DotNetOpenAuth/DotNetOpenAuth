//-----------------------------------------------------------------------
// <copyright file="RequestAccessTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent by the Consumer to exchange a Request Token for an Access Token
	/// and Token Secret.
	/// </summary>
	internal class RequestAccessTokenMessage : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestAccessTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal RequestAccessTokenMessage(ServiceProviderEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Consumer Key.
		/// </summary>
		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string RequestToken { get; set; }
	}
}
