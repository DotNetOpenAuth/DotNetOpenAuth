//-----------------------------------------------------------------------
// <copyright file="AuthServerAllFlowsBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2.Messages;
	using Messaging;

	/// <summary>
	/// A binding element that should be applied for authorization server channels regardless of which flows
	/// are supported.
	/// </summary>
	internal class AuthServerAllFlowsBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthServerAllFlowsBindingElement"/> class.
		/// </summary>
		internal AuthServerAllFlowsBindingElement() {
		}

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
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
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
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
			var authorizationRequest = message as EndUserAuthorizationRequest;
			if (authorizationRequest != null) {
				var client = this.AuthorizationServer.GetClientOrThrow(authorizationRequest.ClientIdentifier);
				ErrorUtilities.VerifyProtocol(authorizationRequest.Callback == null || client.IsCallbackAllowed(authorizationRequest.Callback), OAuthStrings.ClientCallbackDisallowed, authorizationRequest.Callback);
				ErrorUtilities.VerifyProtocol(authorizationRequest.Callback != null || client.DefaultCallback != null, OAuthStrings.NoCallback);

				return MessageProtections.None;
			}

			return null;
		}
	}
}
