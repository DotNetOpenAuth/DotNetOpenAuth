//-----------------------------------------------------------------------
// <copyright file="RsaSha1SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public class RsaSha1SigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class
		/// for use by Consumers.
		/// </summary>
		/// <param name="signingCertificate">The certificate used to sign outgoing messages.</param>
		public RsaSha1SigningBindingElement(X509Certificate2 signingCertificate)
			: this() {
			if (signingCertificate == null) {
				throw new ArgumentNullException("signingCertificate");
			}

			this.SigningCertificate = signingCertificate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class
		/// for use by Service Providers.
		/// </summary>
		public RsaSha1SigningBindingElement()
			: base("RSA-SHA1") {
		}

		/// <summary>
		/// Gets or sets the certificate used to sign outgoing messages.
		/// </summary>
		public X509Certificate2 SigningCertificate { get; set; }

		/// <summary>
		/// Gets or sets the consumer certificate provider.
		/// </summary>
		public IConsumerCertificateProvider ConsumerCertificateProvider { get; set; }

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message per OAuth 1.0 section 9.3.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			if (this.SigningCertificate == null) {
				throw new InvalidOperationException(OAuthStrings.X509CertificateNotProvidedForSigning);
			}

			string signatureBaseString = ConstructSignatureBaseString(message);
			byte[] data = Encoding.ASCII.GetBytes(signatureBaseString);
			var provider = (RSACryptoServiceProvider)this.SigningCertificate.PublicKey.Key;
			byte[] binarySignature = provider.SignData(data, "SHA1");
			string base64Signature = Convert.ToBase64String(binarySignature);
			return base64Signature;
		}

		/// <summary>
		/// Determines whether the signature on some message is valid.
		/// </summary>
		/// <param name="message">The message to check the signature on.</param>
		/// <returns>
		/// 	<c>true</c> if the signature on the message is valid; otherwise, <c>false</c>.
		/// </returns>
		protected override bool IsSignatureValid(ITamperResistantOAuthMessage message) {
			if (this.ConsumerCertificateProvider == null) {
				throw new InvalidOperationException(OAuthStrings.ConsumerCertificateProviderNotAvailable);
			}

			string signatureBaseString = ConstructSignatureBaseString(message);
			byte[] data = Encoding.ASCII.GetBytes(signatureBaseString);

			byte[] carriedSignature = Convert.FromBase64String(message.Signature);

			X509Certificate2 cert = this.ConsumerCertificateProvider.GetCertificate(message);
			if (cert == null) {
				Logger.WarnFormat("Incoming message from consumer '{0}' could not be matched with an appropriate X.509 certificate for signature verification.", message.ConsumerKey);
				return false;
			}

			var provider = (RSACryptoServiceProvider)cert.PublicKey.Key;
			bool valid = provider.VerifyData(data, "SHA1", carriedSignature);
			return valid;
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		protected override ITamperProtectionChannelBindingElement Clone() {
			return new RsaSha1SigningBindingElement() {
				ConsumerCertificateProvider = this.ConsumerCertificateProvider,
				SigningCertificate = this.SigningCertificate,
			};
		}
	}
}
