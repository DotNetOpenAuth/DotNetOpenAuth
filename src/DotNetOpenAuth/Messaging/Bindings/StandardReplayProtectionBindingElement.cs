//-----------------------------------------------------------------------
// <copyright file="StandardReplayProtectionBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Diagnostics;

	/// <summary>
	/// A binding element that checks/verifies a nonce message part.
	/// </summary>
	internal class StandardReplayProtectionBindingElement : IChannelBindingElement {
		/// <summary>
		/// These are the characters that may be chosen from when forming a random nonce.
		/// </summary>
		private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		/// <summary>
		/// The persistent store for nonces received.
		/// </summary>
		private INonceStore nonceStore;

		/// <summary>
		/// The length of generated nonces.
		/// </summary>
		private int nonceLength = 8;

		/// <summary>
		/// A random number generator.
		/// </summary>
		private Random generator = new Random();

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardReplayProtectionBindingElement"/> class.
		/// </summary>
		/// <param name="nonceStore">The store where nonces will be persisted and checked.</param>
		internal StandardReplayProtectionBindingElement(INonceStore nonceStore)
			: this(nonceStore, false) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardReplayProtectionBindingElement"/> class.
		/// </summary>
		/// <param name="nonceStore">The store where nonces will be persisted and checked.</param>
		/// <param name="allowEmptyNonces">A value indicating whether zero-length nonces will be allowed.</param>
		internal StandardReplayProtectionBindingElement(INonceStore nonceStore, bool allowEmptyNonces) {
			ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");

			this.nonceStore = nonceStore;
			this.AllowZeroLengthNonce = allowEmptyNonces;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the protection that this binding element provides messages.
		/// </summary>
		public MessageProtections Protection {
			get { return MessageProtections.ReplayProtection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		#endregion

		/// <summary>
		/// Gets or sets the strength of the nonce, which is measured by the number of
		/// nonces that could theoretically be generated.
		/// </summary>
		/// <remarks>
		/// The strength of the nonce is equal to the number of characters that might appear
		/// in the nonce to the power of the length of the nonce.
		/// </remarks>
		internal double NonceStrength {
			get {
				return Math.Pow(AllowedCharacters.Length, this.nonceLength);
			}

			set {
				value = Math.Max(value, AllowedCharacters.Length);
				this.nonceLength = (int)Math.Log(value, AllowedCharacters.Length);
				Debug.Assert(this.nonceLength > 0, "Nonce length calculated to be below 1!");
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether empty nonces are allowed.
		/// </summary>
		/// <value>Default is <c>false</c>.</value>
		internal bool AllowZeroLengthNonce { get; set; }

		#region IChannelBindingElement Methods

		/// <summary>
		/// Applies a nonce to the message.
		/// </summary>
		/// <param name="message">The message to apply replay protection to.</param>
		/// <returns>True if the message protection was applied.  False otherwise.</returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			IReplayProtectedProtocolMessage nonceMessage = message as IReplayProtectedProtocolMessage;
			if (nonceMessage != null) {
				nonceMessage.Nonce = this.GenerateUniqueFragment();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Verifies that the nonce in an incoming message has not been seen before.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		/// <returns>
		/// True if the message nonce passed replay detection checks.
		/// False if the message did not have a nonce that could be checked at all.
		/// </returns>
		/// <exception cref="ReplayedMessageException">Thrown when the nonce check revealed a replayed message.</exception>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			IReplayProtectedProtocolMessage nonceMessage = message as IReplayProtectedProtocolMessage;
			if (nonceMessage != null && nonceMessage.Nonce != null) {
				ErrorUtilities.VerifyProtocol(nonceMessage.Nonce.Length > 0 || this.AllowZeroLengthNonce, MessagingStrings.InvalidNonceReceived);

				if (!this.nonceStore.StoreNonce(nonceMessage.Nonce, nonceMessage.UtcCreationDate)) {
					throw new ReplayedMessageException(message);
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Generates a string of random characters for use as a nonce.
		/// </summary>
		/// <returns>The nonce string.</returns>
		private string GenerateUniqueFragment() {
			char[] nonce = new char[this.nonceLength];
			for (int i = 0; i < nonce.Length; i++) {
				nonce[i] = AllowedCharacters[this.generator.Next(AllowedCharacters.Length)];
			}
			return new string(nonce);
		}
	}
}
