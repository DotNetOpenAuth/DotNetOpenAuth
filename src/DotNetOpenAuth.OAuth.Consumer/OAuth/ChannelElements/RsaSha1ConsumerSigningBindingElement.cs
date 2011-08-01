//-----------------------------------------------------------------------
// <copyright file="RsaSha1SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public class RsaSha1ConsumerSigningBindingElement : RsaSha1SigningBindingElement {
		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class.
		/// </summary>
		/// <param name="signingCertificate">The certificate used to sign outgoing messages.</param>
		public RsaSha1ConsumerSigningBindingElement(X509Certificate2 signingCertificate) {
			Contract.Requires<ArgumentNullException>(signingCertificate != null);

			this.SigningCertificate = signingCertificate;
		}

		/// <summary>
		/// Gets or sets the certificate used to sign outgoing messages.  Used only by Consumers.
		/// </summary>
		public X509Certificate2 SigningCertificate { get; set; }

		protected override bool IsSignatureValid(ITamperResistantOAuthMessage message) {
			throw new NotImplementedException();
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
			ErrorUtilities.VerifyOperation(this.SigningCertificate != null, OAuthStrings.X509CertificateNotProvidedForSigning);

			string signatureBaseString = ConstructSignatureBaseString(message, this.Channel.MessageDescriptions.GetAccessor(message));
			byte[] data = Encoding.ASCII.GetBytes(signatureBaseString);
			var provider = (RSACryptoServiceProvider)this.SigningCertificate.PrivateKey;
			byte[] binarySignature = provider.SignData(data, "SHA1");
			string base64Signature = Convert.ToBase64String(binarySignature);
			return base64Signature;
		}

		protected override ITamperProtectionChannelBindingElement Clone() {
			return new RsaSha1ConsumerSigningBindingElement(this.SigningCertificate);
		}
	}
}
