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
	internal sealed class DirectUserToConsumerMessage : MessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectUserToConsumerMessage"/> class.
		/// </summary>
		/// <param name="consumer">The URI of the Consumer endpoint to send this message to.</param>
		/// <remarks>
		/// The class is sealed because extra parameters are determined by the callback URI provided by the Consumer.
		/// </remarks>
		internal DirectUserToConsumerMessage(Uri consumer)
			: base(MessageProtection.None, MessageTransport.Indirect, new MessageReceivingEndpoint(consumer, HttpDeliveryMethod.GetRequest)) {
		}

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
		}

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }
	}
}
