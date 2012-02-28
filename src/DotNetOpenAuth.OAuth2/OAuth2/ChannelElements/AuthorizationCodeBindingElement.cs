﻿//-----------------------------------------------------------------------
// <copyright file="AuthorizationCodeBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Messages;
	using Messaging;
	using Messaging.Bindings;

	/// <summary>
	/// A binding element for OAuth 2.0 authorization servers that create/verify
	/// issued authorization codes as part of obtaining access/refresh tokens.
	/// </summary>
	internal class AuthorizationCodeBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationCodeBindingElement"/> class.
		/// </summary>
		internal AuthorizationCodeBindingElement() {
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
		/// Gets the maximum message age from the standard expiration binding element.
		/// </summary>
		/// <value>This interval need not account for clock skew because it is only compared within a single authorization server or farm of servers.</value>
		internal static TimeSpan MaximumMessageAge {
			get { return Configuration.DotNetOpenAuthSection.Messaging.MaximumMessageLifetimeNoSkew; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			var response = message as EndUserAuthorizationSuccessAuthCodeResponse;
			if (response != null) {
				var directResponse = (IDirectResponseProtocolMessage)response;
				var request = (EndUserAuthorizationRequest)directResponse.OriginatingRequest;
				IAuthorizationCodeCarryingRequest tokenCarryingResponse = response;
				tokenCarryingResponse.AuthorizationDescription = new AuthorizationCode(request.ClientIdentifier, request.Callback, response.Scope, response.AuthorizingUsername);

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
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var request = message as AccessTokenAuthorizationCodeRequest;
			if (request != null) {
				IAuthorizationCarryingRequest tokenRequest = request;
				((AuthorizationCode)tokenRequest.AuthorizationDescription).VerifyCallback(request.Callback);

				return MessageProtections.None;
			}

			return null;
		}
	}
}
