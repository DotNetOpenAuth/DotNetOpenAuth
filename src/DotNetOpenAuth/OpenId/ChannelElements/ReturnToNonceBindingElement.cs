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
		private static readonly string NonceParameter = "dnoi.request_nonce";
		private static readonly int NonceByteLength = 128 / 8; // 128-bit nonce

		private INonceStore nonceStore;

		internal ReturnToNonceBindingElement(INonceStore nonceStore) {
			ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");

			this.nonceStore = nonceStore;
		}

		#region IChannelBindingElement Members

		public Channel Channel { get; set; }

		public MessageProtections Protection {
			get { return MessageProtections.ReplayProtection; }
		}

		public bool PrepareMessageForSending(IProtocolMessage message) {
			// We only add a nonce to 1.x auth requests.
			SignedResponseRequest request = message as SignedResponseRequest;
			if (request != null && request.Version.Major < 2) {
				request.AddReturnToArguments(NonceParameter, CustomNonce.NewNonce().Serialize());

				return true;
			}

			return false;
		}

		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			IndirectSignedResponse response = message as IndirectSignedResponse;
			if (response != null && response.Version.Major < 2) {
				string nonceValue = response.GetReturnToArgument(NonceParameter);
				ErrorUtilities.VerifyProtocol(nonceValue != null, OpenIdStrings.UnsolicitedAssertionsNotAllowedFrom1xOPs);

				CustomNonce nonce = CustomNonce.Deserialize(nonceValue);
				ErrorUtilities.VerifyProtocol(this.nonceStore.StoreNonce(nonce.RandomPartAsString, nonce.CreationDateUtc), MessagingStrings.ReplayAttackDetected);
				return true;
			}

			return false;
		}

		#endregion

		private class CustomNonce {
			private byte[] randomPart;

			private CustomNonce(DateTime creationDate, byte[] randomPart) {
				this.CreationDateUtc = creationDate;
				this.randomPart = randomPart;
			}

			internal static CustomNonce NewNonce() {
				return new CustomNonce(DateTime.UtcNow, MessagingUtilities.GetCryptoRandomData(NonceByteLength));
			}

			internal DateTime CreationDateUtc { get; private set; }

			internal string RandomPartAsString {
				get { return Convert.ToBase64String(this.randomPart); }
			}

			internal string Serialize() {
				byte[] timestamp = BitConverter.GetBytes(this.CreationDateUtc.Ticks);
				byte[] nonce = new byte[timestamp.Length + this.randomPart.Length];
				timestamp.CopyTo(nonce, 0);
				this.randomPart.CopyTo(nonce, timestamp.Length);
				string base64Nonce = Convert.ToBase64String(nonce);
				return base64Nonce;
			}

			internal static CustomNonce Deserialize(string value) {
				ErrorUtilities.VerifyNonZeroLength(value, "value");

				byte[] nonce = Convert.FromBase64String(value);
				DateTime creationDateUtc = new DateTime(BitConverter.ToInt64(nonce, 0), DateTimeKind.Utc);
				byte[] randomPart = new byte[NonceByteLength];
				Array.Copy(nonce, sizeof(long), randomPart, 0, NonceByteLength);
				return new CustomNonce(creationDateUtc, randomPart);
			}
		}
	}
}
