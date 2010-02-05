//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationInUserAgentGrantedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// The message sent by the Authorization Server to the Consumer via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Consumer where they started their experience.
	/// </summary>
	internal class UserAuthorizationInUserAgentGrantedResponse : MessageBase, IDirectedProtocolMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationInUserAgentGrantedResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The protocol version.</param>
		internal UserAuthorizationInUserAgentGrantedResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Consumer for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.wrap_verification_code, IsRequired = true, AllowEmpty = true)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets some state as provided by the client in the authorization request.
		/// </summary>
		/// <value>An opaque value defined by the client.</value>
		/// <remarks>
		/// REQUIRED if the Client sent the value in the <see cref="UserAuthorizationRequestInUserAgentRequest"/>.
		/// </remarks>
		[MessagePart(Protocol.wrap_client_state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }
	}
}
