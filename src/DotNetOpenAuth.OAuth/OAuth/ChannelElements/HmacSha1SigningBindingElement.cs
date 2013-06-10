//-----------------------------------------------------------------------
// <copyright file="HmacSha1SigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;

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
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			string key = GetConsumerAndTokenSecretString(message);
			using (var hasher = HmacAlgorithms.Create(HmacAlgorithms.HmacSha1, Encoding.ASCII.GetBytes(key))) {
				string baseString = ConstructSignatureBaseString(message, this.Channel.MessageDescriptions.GetAccessor(message));
				byte[] digest = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
				return Convert.ToBase64String(digest);
			}
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		protected override ITamperProtectionChannelBindingElement Clone() {
			return new HmacSha1SigningBindingElement();
		}
	}
}
