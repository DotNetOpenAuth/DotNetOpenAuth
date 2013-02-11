//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementChain.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A tamper protection applying binding element that can use any of several given
	/// binding elements to apply the protection.
	/// </summary>
	internal class SigningBindingElementChain : ITamperProtectionChannelBindingElement {
		/// <summary>
		/// The various signing binding elements that may be applicable to a message in preferred use order.
		/// </summary>
		private readonly ITamperProtectionChannelBindingElement[] signers;

		/// <summary>
		/// Initializes a new instance of the <see cref="SigningBindingElementChain"/> class.
		/// </summary>
		/// <param name="signers">
		/// The signing binding elements that may be used for some outgoing message,
		/// in preferred use order.
		/// </param>
		internal SigningBindingElementChain(ITamperProtectionChannelBindingElement[] signers) {
			Requires.NotNullOrEmpty(signers, "signers");
			Requires.NullOrNotNullElements(signers, "signers");
			Requires.That(signers.Select(s => s.Protection).Distinct().Count() == 1, "signers", OAuthStrings.SigningElementsMustShareSameProtection);

			this.signers = signers;
		}

		#region ITamperProtectionChannelBindingElement Properties

		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// May be null for Consumers (who never have to verify signatures).
		/// </summary>
		public Action<ITamperResistantOAuthMessage> SignatureCallback {
			get {
				return this.signers[0].SignatureCallback;
			}

			set {
				foreach (ITamperProtectionChannelBindingElement signer in this.signers) {
					signer.SignatureCallback = value;
				}
			}
		}

		#endregion

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		public MessageProtections Protection {
			get { return this.signers[0].Protection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel {
			get {
				return this.signers[0].Channel;
			}

			set {
				foreach (var signer in this.signers) {
					signer.Channel = value;
				}
			}
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		public async Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			foreach (IChannelBindingElement signer in this.signers) {
				ErrorUtilities.VerifyInternal(signer.Channel != null, "A binding element's Channel property is unexpectedly null.");
				MessageProtections? result = await signer.ProcessOutgoingMessageAsync(message, cancellationToken);
				if (result.HasValue) {
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		public async Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			foreach (IChannelBindingElement signer in this.signers) {
				ErrorUtilities.VerifyInternal(signer.Channel != null, "A binding element's Channel property is unexpectedly null.");
				MessageProtections? result = await signer.ProcessIncomingMessageAsync(message, cancellationToken);
				if (result.HasValue) {
					return result;
				}
			}

			return null;
		}

		#endregion

		#region ITamperProtectionChannelBindingElement Methods

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		ITamperProtectionChannelBindingElement ITamperProtectionChannelBindingElement.Clone() {
			return new SigningBindingElementChain(this.signers.Select(el => (ITamperProtectionChannelBindingElement)el.Clone()).ToArray());
		}

		#endregion
	}
}
