//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementChain.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A tamper protection applying binding element that can use any of several given
	/// binding elements to apply the protection.
	/// </summary>
	internal class SigningBindingElementChain : ITamperProtectionChannelBindingElement {
		/// <summary>
		/// The various signing binding elements that may be applicable to a message in preferred use order.
		/// </summary>
		private ITamperProtectionChannelBindingElement[] signers;

		/// <summary>
		/// Initializes a new instance of the <see cref="SigningBindingElementChain"/> class.
		/// </summary>
		/// <param name="signers">
		/// The signing binding elements that may be used for some outgoing message,
		/// in preferred use order.
		/// </param>
		internal SigningBindingElementChain(ITamperProtectionChannelBindingElement[] signers) {
			if (signers == null) {
				throw new ArgumentNullException("signers");
			}
			if (signers.Length == 0) {
				throw new ArgumentException(MessagingStrings.SequenceContainsNoElements, "signers");
			}
			if (signers.Contains(null)) {
				throw new ArgumentException(MessagingStrings.SequenceContainsNullElement, "signers");
			}
			MessageProtection protection = signers[0].Protection;
			if (signers.Any(element => element.Protection != protection)) {
				throw new ArgumentException(Strings.SigningElementsMustShareSameProtection, "signers");
			}

			this.signers = signers;
		}

		#region ITamperProtectionChannelBindingElement Members

		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// May be null for Consumers (who never have to verify signatures).
		/// </summary>
		public Action<ITamperResistantOAuthMessage> SignatureVerificationCallback {
			get {
				return this.signers[0].SignatureVerificationCallback;
			}

			set {
				foreach (ITamperProtectionChannelBindingElement signer in this.signers) {
					signer.SignatureVerificationCallback = value;
				}
			}
		}

		#endregion

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		public MessageProtection Protection {
			get { return this.signers[0].Protection; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			foreach (IChannelBindingElement signer in this.signers) {
				if (signer.PrepareMessageForSending(message)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			foreach (IChannelBindingElement signer in this.signers) {
				if (signer.PrepareMessageForReceiving(message)) {
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
