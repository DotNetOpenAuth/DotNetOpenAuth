//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	internal abstract class SigningBindingElementBase : IChannelBindingElement {
		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the message protection provided by this binding element.
		/// </summary>
		public MessageProtection Protection {
			get { return MessageProtection.TamperProtection; }
		}

		/// <summary>
		/// Signs the outgoing message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>True if the message was signed.  False otherwise.</returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				this.Sign(signedMessage);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Verifies the signature on an incoming message.
		/// </summary>
		/// <param name="message">The message whose signature should be verified.</param>
		/// <returns>True if the signature was verified.  False if the message had no signature.</returns>
		/// <exception cref="InvalidSignatureException">Thrown if the signature is invalid.</exception>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				if (!this.IsSignatureValid(signedMessage)) {
					throw new InvalidSignatureException(message);
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Applies a signature to the message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		protected abstract void Sign(ITamperResistantOAuthMessage message);

		/// <summary>
		/// Validates the signature on a message.
		/// Does NOT throw an exception on failing signature verification.
		/// </summary>
		/// <param name="message">The message with a signature to verify.</param>
		/// <returns>True if the signature is valid.  False otherwise.</returns>
		protected abstract bool IsSignatureValid(ITamperResistantOAuthMessage message);
	}
}
