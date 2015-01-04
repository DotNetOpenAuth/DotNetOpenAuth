//-----------------------------------------------------------------------
// <copyright file="PlaintextSigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public class PlaintextSigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="PlaintextSigningBindingElement"/> class.
		/// </summary>
		public PlaintextSigningBindingElement()
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
			if (string.Equals(message.Recipient.Scheme, "https", StringComparison.OrdinalIgnoreCase)) {
				return true;
			} else {
				Logger.Bindings.DebugFormat("The {0} element will not sign this message because the URI scheme is not https.", this.GetType().Name);
				return false;
			}
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		protected override ITamperProtectionChannelBindingElement Clone() {
			return new PlaintextSigningBindingElement();
		}
	}
}
