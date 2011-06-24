//-----------------------------------------------------------------------
// <copyright file="AccessTokenBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Serializes access tokens inside an outgoing message.
	/// </summary>
	internal class AccessTokenBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenBindingElement"/> class.
		/// </summary>
		internal AccessTokenBindingElement() {
		}

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <value>Always <c>MessageProtections.None</c></value>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public override MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			var response = message as EndUserAuthorizationSuccessAccessTokenResponse;
			if (response != null) {
				var directResponse = (IDirectResponseProtocolMessage)response;
				var request = (IAccessTokenRequest)directResponse.OriginatingRequest;
				IAuthorizationCarryingRequest tokenCarryingResponse = response;
				tokenCarryingResponse.AuthorizationDescription = new AccessToken(request.ClientIdentifier, response.Scope, response.AuthorizingUsername, response.Lifetime);

				return MessageProtections.None;
			}

			return null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			return null;
		}
	}
}
