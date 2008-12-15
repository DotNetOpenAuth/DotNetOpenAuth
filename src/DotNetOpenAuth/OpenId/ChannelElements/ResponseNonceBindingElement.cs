//-----------------------------------------------------------------------
// <copyright file="ResponseNonceBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Applies the expiration and replay detection elements that are behind the
	/// openid.response_nonce parameter.
	/// </summary>
	internal class ResponseNonceBindingElement : IChannelBindingElement {
		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseNonceBindingElement"/> class.
		/// </summary>
		internal ResponseNonceBindingElement() {
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.ReplayProtection"/> and <see cref="MessageProtections.Expiration"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.ReplayProtection | MessageProtections.Expiration; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				// TODO: code here
				return true;
			}

			return false;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				// TODO: code here
				return true;
			}

			return false;
		}

		#endregion
	}
}
