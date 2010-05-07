//-----------------------------------------------------------------------
// <copyright file="WebAppSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Client where they started their experience.
	/// </summary>
	internal class WebAppSuccessResponse : MessageBase, IMessageWithClientState {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The protocol version.</param>
		internal WebAppSuccessResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Gets or sets some state as provided by the client in the authorization request.
		/// </summary>
		/// <value>An opaque value defined by the client.</value>
		/// <remarks>
		/// REQUIRED if the Client sent the value in the <see cref="WebAppRequest"/>.
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		public string ClientState { get; set; }

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Consumer for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = true)]
		internal string VerificationCode { get; set; }
	}
}
