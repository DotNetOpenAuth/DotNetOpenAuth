//-----------------------------------------------------------------------
// <copyright file="ReturnToNonceBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// This binding element adds a nonce to a Relying Party's outgoing 
	/// authentication request when working against an OpenID 1.0 Provider
	/// in order to protect against replay attacks.
	/// </summary>
	/// <remarks>
	/// <para>In the messaging stack, this binding element looks like an ordinary
	/// transform-type of binding element rather than a protection element,
	/// due to its required order in the channel stack and that it exists
	/// only on the RP side and only on 1.0 messages.</para>
	/// </remarks>
	internal class ReturnToNonceBindingElement : IChannelBindingElement {
		/// <summary>
		/// The parameter of the callback parameter we tack onto the return_to URL
		/// to store the replay-detection nonce.
		/// </summary>
		private const string NonceParameter = "dnoi.request_nonce";

		/// <summary>
		/// The length of the generated nonce's random part.
		/// </summary>
		private const int NonceByteLength = 128 / 8; // 128-bit nonce

		/// <summary>
		/// The nonce store that will allow us to recall which nonces we've seen before.
		/// </summary>
		private INonceStore nonceStore;

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private Channel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnToNonceBindingElement"/> class.
		/// </summary>
		/// <param name="nonceStore">The nonce store to use.</param>
		internal ReturnToNonceBindingElement(INonceStore nonceStore) {
			ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");

			this.nonceStore = nonceStore;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel {
			get {
				return this.channel;
			}

			set {
				if (this.channel == value) {
					return;
				}

				this.channel = value;
			}
		}

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		public MessageProtections Protection {
			get { return MessageProtections.ReplayProtection; }
		}

		#endregion

		/// <summary>
		/// Gets the maximum message age from the standard expiration binding element.
		/// </summary>
		private static TimeSpan MaximumMessageAge {
			get { return StandardExpirationBindingElement.MaximumMessageAge; }
		}

		#region IChannelBindingElement Methods

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			// We only add a nonce to 1.x auth requests.
			SignedResponseRequest request = message as SignedResponseRequest;
			if (request != null && request.Version.Major < 2) {
				request.AddReturnToArguments(NonceParameter, CustomNonce.NewNonce().Serialize());

				return true;
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
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			IndirectSignedResponse response = message as IndirectSignedResponse;
			if (response != null && response.Version.Major < 2) {
				string nonceValue = response.GetReturnToArgument(NonceParameter);
				ErrorUtilities.VerifyProtocol(nonceValue != null, OpenIdStrings.UnsolicitedAssertionsNotAllowedFrom1xOPs);

				CustomNonce nonce = CustomNonce.Deserialize(nonceValue);
				DateTime expirationDate = nonce.CreationDateUtc + MaximumMessageAge;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, message);
				}

				if (!this.nonceStore.StoreNonce(nonce.RandomPartAsString, nonce.CreationDateUtc)) {
					throw new ReplayedMessageException(message);
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// A special DotNetOpenId-only nonce used by the RP when talking to 1.0 OPs in order
		/// to protect against replay attacks.
		/// </summary>
		private class CustomNonce {
			/// <summary>
			/// The random bits generated for the nonce.
			/// </summary>
			private byte[] randomPart;

			/// <summary>
			/// Initializes a new instance of the <see cref="CustomNonce"/> class.
			/// </summary>
			/// <param name="creationDate">The creation date of the nonce.</param>
			/// <param name="randomPart">The random bits that help make the nonce unique.</param>
			private CustomNonce(DateTime creationDate, byte[] randomPart) {
				this.CreationDateUtc = creationDate;
				this.randomPart = randomPart;
			}

			/// <summary>
			/// Gets the creation date.
			/// </summary>
			internal DateTime CreationDateUtc { get; private set; }

			/// <summary>
			/// Gets the random part of the nonce as a base64 encoded string.
			/// </summary>
			internal string RandomPartAsString {
				get { return Convert.ToBase64String(this.randomPart); }
			}

			/// <summary>
			/// Creates a new nonce.
			/// </summary>
			/// <returns>The newly instantiated instance.</returns>
			internal static CustomNonce NewNonce() {
				return new CustomNonce(DateTime.UtcNow, MessagingUtilities.GetCryptoRandomData(NonceByteLength));
			}

			/// <summary>
			/// Deserializes a nonce from the return_to parameter.
			/// </summary>
			/// <param name="value">The base64-encoded value of the nonce.</param>
			/// <returns>The instantiated and initialized nonce.</returns>
			internal static CustomNonce Deserialize(string value) {
				ErrorUtilities.VerifyNonZeroLength(value, "value");

				byte[] nonce = Convert.FromBase64String(value);
				DateTime creationDateUtc = new DateTime(BitConverter.ToInt64(nonce, 0), DateTimeKind.Utc);
				byte[] randomPart = new byte[NonceByteLength];
				Array.Copy(nonce, sizeof(long), randomPart, 0, NonceByteLength);
				return new CustomNonce(creationDateUtc, randomPart);
			}

			/// <summary>
			/// Serializes the entire nonce for adding to the return_to URL.
			/// </summary>
			/// <returns>The base64-encoded string representing the nonce.</returns>
			internal string Serialize() {
				byte[] timestamp = BitConverter.GetBytes(this.CreationDateUtc.Ticks);
				byte[] nonce = new byte[timestamp.Length + this.randomPart.Length];
				timestamp.CopyTo(nonce, 0);
				this.randomPart.CopyTo(nonce, timestamp.Length);
				string base64Nonce = Convert.ToBase64String(nonce);
				return base64Nonce;
			}
		}
	}
}
