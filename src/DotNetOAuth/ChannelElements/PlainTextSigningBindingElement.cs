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
		/// Initializes a new instance of the <see cref="PlainTextSigningBindingElement"/> class.
		/// </summary>
		internal PlainTextSigningBindingElement()
			: base("PLAINTEXT") {
		}

		/// <summary>
		/// Applies a signature to the message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		protected override void Sign(ITamperResistantOAuthMessage message) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates the signature on a message.
		/// Does NOT throw an exception on failing signature verification.
		/// </summary>
		/// <param name="message">The message with a signature to verify.</param>
		/// <returns>True if the signature is valid.  False otherwise.</returns>
		protected override bool IsSignatureValid(ITamperResistantOAuthMessage message) {
			throw new NotImplementedException();
		}
	}
}
