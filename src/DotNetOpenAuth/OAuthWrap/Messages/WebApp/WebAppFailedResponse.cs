//-----------------------------------------------------------------------
// <copyright file="WebAppFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The message the Authorization Server MAY use to send the user back to the Client
	/// following the user's denial to grant Consumer with authorization of 
	/// access to requested resources.
	/// </summary>
	internal class WebAppFailedResponse : MessageBase, IMessageWithClientState {
		/// <summary>
		/// A constant parameter that indicates the user refused to grant the requested authorization.
		/// </summary>
		[MessagePart(Protocol.wrap_error_reason, IsRequired = true)]
		private const string ErrorReason = Protocol.user_denied;

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppFailedResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The recipient of the message.</param>
		/// <param name="version">The version.</param>
		internal WebAppFailedResponse(Uri clientCallback, Version version) :
			base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Gets or sets the state of the client that was supplied to the Authorization Server.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with the authorization request.
		/// </value>
		/// <remarks>
		/// If this value is present, the Authorization Server MUST return it to the Client's callback URL.
		/// </remarks>
		[MessagePart(Protocol.wrap_client_state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }
	}
}
