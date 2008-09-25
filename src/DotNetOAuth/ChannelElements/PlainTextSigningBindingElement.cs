//-----------------------------------------------------------------------
// <copyright file="PlainTextSigningBindingElement.cs" company="Andrew Arnott">
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
	internal class PlainTextSigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="PlainTextSigningBindingElement"/> class
		/// for use by Consumers.
		/// </summary>
		internal PlainTextSigningBindingElement()
			: this(null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlainTextSigningBindingElement"/> class.
		/// </summary>
		/// <param name="signatureVerificationCallback">
		/// The delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// May be null for Consumers (who never have to verify signatures).
		/// </param>
		internal PlainTextSigningBindingElement(Action<ITamperResistantOAuthMessage> signatureVerificationCallback)
			: base("PLAINTEXT", signatureVerificationCallback) {
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message according to OAuth 1.0 section 9.4.1.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			return Uri.EscapeDataString(GetConsumerAndTokenSecretString(message));
		}
	}
}
