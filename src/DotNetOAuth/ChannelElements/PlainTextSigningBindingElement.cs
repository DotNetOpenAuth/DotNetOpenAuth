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
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message according to OAuth 1.0 section 9.4.1.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			StringBuilder builder = new StringBuilder();
			builder.Append(Uri.EscapeDataString(message.ConsumerSecret));
			builder.Append("&");
			builder.Append(Uri.EscapeDataString(message.TokenSecret));
			return Uri.EscapeDataString(builder.ToString());
		}
	}
}
