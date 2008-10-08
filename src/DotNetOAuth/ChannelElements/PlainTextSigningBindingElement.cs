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
	public class PlainTextSigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="PlainTextSigningBindingElement"/> class.
		/// </summary>
		public PlainTextSigningBindingElement()
			: base("PLAINTEXT") {
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
			return GetConsumerAndTokenSecretString(message);
		}

		/// <summary>
		/// Checks whether this binding element applies to this message.
		/// </summary>
		/// <param name="message">The message that needs to be signed.</param>
		/// <returns>True if this binding element can be used to sign the message.  False otherwise.</returns>
		protected override bool IsMessageApplicable(ITamperResistantOAuthMessage message) {
			return string.Equals(message.Recipient.Scheme, "https", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		protected override object Clone() {
			return new PlainTextSigningBindingElement();
		}
	}
}
