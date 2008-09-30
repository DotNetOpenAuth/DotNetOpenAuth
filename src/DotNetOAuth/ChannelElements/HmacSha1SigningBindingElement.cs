//-----------------------------------------------------------------------
// <copyright file="HmacSha1SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public class HmacSha1SigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="HmacSha1SigningBindingElement"/> class
		/// </summary>
		public HmacSha1SigningBindingElement()
			: base("HMAC-SHA1") {
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message per OAuth 1.0 section 9.2.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			string key = GetConsumerAndTokenSecretString(message);
			HashAlgorithm hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));
			string baseString = ConstructSignatureBaseString(message);
			Logger.DebugFormat("Signing message with signature base string: {0}", baseString);
			byte[] digest = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
			return Convert.ToBase64String(digest);
		}
	}
}
