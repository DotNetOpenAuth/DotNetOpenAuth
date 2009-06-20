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
	public class RsaSha1SigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// The name of the hash algorithm to use.
		/// </summary>
		private const string HashAlgorithmName = "RSA-SHA1";

		/// <summary>
		/// The token manager for the service provider.
		/// </summary>
		private IServiceProviderTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class
		/// for use by Consumers.
		/// </summary>
		/// <param name="signingCertificate">The certificate used to sign outgoing messages.</param>
		public RsaSha1SigningBindingElement(X509Certificate2 signingCertificate)
			: base(HashAlgorithmName) {
			Contract.Requires(signingCertificate != null);
			ErrorUtilities.VerifyArgumentNotNull(signingCertificate, "signingCertificate");

			this.SigningCertificate = signingCertificate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class
		/// for use by Service Providers.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		public RsaSha1SigningBindingElement(IServiceProviderTokenManager tokenManager)
			: base(HashAlgorithmName) {
			Contract.Requires(tokenManager != null);
			ErrorUtilities.VerifyArgumentNotNull(tokenManager, "tokenManager");

			this.tokenManager = tokenManager;
		}

		/// <summary>
		/// Gets or sets the certificate used to sign outgoing messages.  Used only by Consumers.
		/// </summary>
		public X509Certificate2 SigningCertificate { get; set; }

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message per OAuth 1.0 section 9.3.
		/// </remarks>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyOperation(this.SigningCertificate != null, OAuthStrings.X509CertificateNotProvidedForSigning);

			string signatureBaseString = ConstructSignatureBaseString(message, this.Channel.MessageDescriptions.GetAccessor(message));
			byte[] data = Encoding.ASCII.GetBytes(signatureBaseString);
			var provider = (RSACryptoServiceProvider)this.SigningCertificate.PrivateKey;
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
			ErrorUtilities.VerifyInternal(this.tokenManager != null, "No token manager available for fetching Consumer public certificates.");

			string signatureBaseString = ConstructSignatureBaseString(message, this.Channel.MessageDescriptions.GetAccessor(message));
			byte[] data = Encoding.ASCII.GetBytes(signatureBaseString);

			byte[] carriedSignature = Convert.FromBase64String(message.Signature);

			X509Certificate2 cert = this.tokenManager.GetConsumer(message.ConsumerKey).Certificate;
			if (cert == null) {
				Logger.Signatures.WarnFormat("Incoming message from consumer '{0}' could not be matched with an appropriate X.509 certificate for signature verification.", message.ConsumerKey);
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
			if (this.tokenManager != null) {
				return new RsaSha1SigningBindingElement(this.tokenManager);
			} else {
				return new RsaSha1SigningBindingElement(this.SigningCertificate);
			}
		}
	}
}
