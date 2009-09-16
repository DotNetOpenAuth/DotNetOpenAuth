//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationInUserAgentDeniedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleApiAuth.Messages {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The message the Token Issuer MAY use to send the user back to the Consumer
	/// following the user's denial to grant Consumer with authorization of 
	/// access to requested resources.
	/// </summary>
	public class UserAuthorizationInUserAgentDeniedResponse : MessageBase, IDirectedProtocolMessage {
		/// <summary>
		/// A constant parameter that indicates the user refused to grant the requested authorization.
		/// </summary>
		[MessagePart(Protocol.sa_error_reason, IsRequired = true)]
		private const string ErrorReason = Protocol.sa_error_reason_denied;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationInUserAgentDeniedResponse"/> class.
		/// </summary>
		/// <param name="consumerCallback">The recipient of the message.</param>
		/// <param name="version">The version.</param>
		internal UserAuthorizationInUserAgentDeniedResponse(Uri consumerCallback, Version version) :
			base(version, MessageTransport.Indirect, consumerCallback) {
		}

		/// <summary>
		/// Gets or sets the state of the consumer.
		/// </summary>
		/// <value>
		/// An opaque value that Consumers can use to maintain state associated with this request.
		/// </value>
		/// <remarks>
		/// If this value is present, the Token Issuer MUST return it to the Consumer's callback URL.
		/// </remarks>
		[MessagePart(Protocol.sa_consumer_state, IsRequired = false, AllowEmpty = true)]
		public string ConsumerState { get; set; }
	}
}
