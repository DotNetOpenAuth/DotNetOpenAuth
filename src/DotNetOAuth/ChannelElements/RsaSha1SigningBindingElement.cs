//-----------------------------------------------------------------------
// <copyright file="RsaSha1SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public class RsaSha1SigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class.
		/// </summary>
		internal RsaSha1SigningBindingElement()
			: base("RSA-SHA1") {
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message per OAuth 1.0 section 9.3.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			AsymmetricAlgorithm provider = new RSACryptoServiceProvider();
			AsymmetricSignatureFormatter hasher = new RSAPKCS1SignatureFormatter(provider);
			hasher.SetHashAlgorithm("SHA1");
			byte[] digest = hasher.CreateSignature(Encoding.ASCII.GetBytes(ConstructSignatureBaseString(message)));
			return Convert.ToBase64String(digest);
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		protected override ITamperProtectionChannelBindingElement Clone() {
			return new RsaSha1SigningBindingElement();
		}
	}
}
