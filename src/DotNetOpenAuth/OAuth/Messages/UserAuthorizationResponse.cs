//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message used to redirect the user from a Service Provider to a Consumer's web site.
	/// </summary>
	/// <remarks>
	/// The class is sealed because extra parameters are determined by the callback URI provided by the Consumer.
	/// </remarks>
	public sealed class UserAuthorizationResponse : MessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationResponse"/> class.
		/// </summary>
		/// <param name="consumer">The URI of the Consumer endpoint to send this message to.</param>
		internal UserAuthorizationResponse(Uri consumer)
			: base(MessageProtections.None, MessageTransport.Indirect, new MessageReceivingEndpoint(consumer, HttpDeliveryMethods.GetRequest)) {
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
		[MessagePart("oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }
	}
}
