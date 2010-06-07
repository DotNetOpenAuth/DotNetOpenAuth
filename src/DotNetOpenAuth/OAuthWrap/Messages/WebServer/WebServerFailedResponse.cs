//-----------------------------------------------------------------------
// <copyright file="WebServerFailedResponse.cs" company="Andrew Arnott">
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
	internal class WebServerFailedResponse : MessageBase, IMessageWithClientState {
		/// <summary>
		/// A constant parameter that indicates the user refused to grant the requested authorization.
		/// </summary>
		[MessagePart(Protocol.error, IsRequired = true)]
		private const string ErrorReason = Protocol.user_denied;

		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerFailedResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The recipient of the message.</param>
		/// <param name="version">The version.</param>
		internal WebServerFailedResponse(Uri clientCallback, Version version) :
			base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerFailedResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="request">The request.</param>
		internal WebServerFailedResponse(Uri clientCallback, WebServerRequest request)
			: this(clientCallback, ((IMessage)request).Version) {
			Contract.Requires<ArgumentNullException>(clientCallback != null, "clientCallback");
			Contract.Requires<ArgumentNullException>(request != null, "request");
			((IMessageWithClientState)this).ClientState = ((IMessageWithClientState)request).ClientState;
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
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		public string ClientState { get; set; }
	}
}
