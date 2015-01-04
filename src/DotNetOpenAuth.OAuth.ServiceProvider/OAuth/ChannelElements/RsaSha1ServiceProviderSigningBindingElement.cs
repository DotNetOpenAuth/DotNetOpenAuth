//-----------------------------------------------------------------------
// <copyright file="RsaSha1ServiceProviderSigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha", Justification = "Acronym")]
	public class RsaSha1ServiceProviderSigningBindingElement : RsaSha1SigningBindingElement {
		/// <summary>
		/// The token manager for the service provider.
		/// </summary>
		private IServiceProviderTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1ServiceProviderSigningBindingElement"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		public RsaSha1ServiceProviderSigningBindingElement(IServiceProviderTokenManager tokenManager) {
			Requires.NotNull(tokenManager, "tokenManager");

			this.tokenManager = tokenManager;
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
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>
		/// The signature for the message.
		/// </returns>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>
		/// A new instance of the binding element.
		/// </returns>
		protected override ITamperProtectionChannelBindingElement Clone() {
			return new RsaSha1ServiceProviderSigningBindingElement(this.tokenManager);
		}
	}
}
