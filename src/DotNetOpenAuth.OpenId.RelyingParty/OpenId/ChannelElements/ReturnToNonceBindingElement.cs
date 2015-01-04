//-----------------------------------------------------------------------
// <copyright file="ReturnToNonceBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// This binding element adds a nonce to a Relying Party's outgoing 
	/// authentication request when working against an OpenID 1.0 Provider
	/// in order to protect against replay attacks or on all authentication
	/// requests to distinguish solicited from unsolicited assertions.
	/// </summary>
	/// <remarks>
	/// <para>This nonce goes beyond the OpenID 1.x spec, but adds to security.
	/// Since this library's Provider implementation also provides special nonce
	/// protection for 1.0 messages, this security feature overlaps with that one.
	/// This means that if an RP from this library were talking to an OP from this
	/// library, but the Identifier being authenticated advertised the OP as a 1.x
	/// OP, then both RP and OP might try to use a nonce for protecting the assertion.
	/// There's no problem with that--it will still all work out.  And it would be a 
	/// very rare combination of elements anyway.
	/// </para>
	/// <para>
	/// This binding element deactivates itself for OpenID 2.0 (or later) messages 
	/// since they are automatically protected in the protocol by the Provider's
	/// openid.response_nonce parameter.  The exception to this is when
	/// <see cref="RelyingPartySecuritySettings.RejectUnsolicitedAssertions"/> is
	/// set to <c>true</c>, which will not only add a request nonce to every outgoing
	/// authentication request but also require that it be present in positive
	/// assertions, effectively disabling unsolicited assertions.
	/// </para>
	/// <para>In the messaging stack, this binding element looks like an ordinary
	/// transform-type of binding element rather than a protection element,
	/// due to its required order in the channel stack and that it exists
	/// only on the RP side and only on some messages.</para>
	/// </remarks>
	internal class ReturnToNonceBindingElement : IChannelBindingElement {
		/// <summary>
		/// A reusable, precompleted task that can be returned many times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> NullTask = Task.FromResult<MessageProtections?>(null);

		/// <summary>
		/// A reusable, precompleted task that can be returned many times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> ReplayProtectionTask =
			Task.FromResult<MessageProtections?>(MessageProtections.ReplayProtection);

		/// <summary>
		/// The context within which return_to nonces must be unique -- they all go into the same bucket.
		/// </summary>
		private const string ReturnToNonceContext = "https://localhost/dnoa/return_to_nonce";

		/// <summary>
		/// The length of the generated nonce's random part.
		/// </summary>
		private const int NonceByteLength = 128 / 8; // 128-bit nonce

		/// <summary>
		/// The nonce store that will allow us to recall which nonces we've seen before.
		/// </summary>
		private INonceStore nonceStore;

		/// <summary>
		/// The security settings at the RP.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private Channel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnToNonceBindingElement"/> class.
		/// </summary>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings of the RP.</param>
		internal ReturnToNonceBindingElement(INonceStore nonceStore, RelyingPartySecuritySettings securitySettings) {
			Requires.NotNull(nonceStore, "nonceStore");
			Requires.NotNull(securitySettings, "securitySettings");

			this.nonceStore = nonceStore;
			this.securitySettings = securitySettings;
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			// We only add a nonce to some auth requests.
			SignedResponseRequest request = message as SignedResponseRequest;
			if (this.UseRequestNonce(request)) {
				request.AddReturnToArguments(Protocol.ReturnToNonceParameter, CustomNonce.NewNonce().Serialize());
				request.SignReturnTo = true; // a nonce without a signature is completely pointless

				return ReplayProtectionTask;
			}

			return NullTask;
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
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			IndirectSignedResponse response = message as IndirectSignedResponse;
			if (this.UseRequestNonce(response)) {
				if (!response.ReturnToParametersSignatureValidated) {
					Logger.OpenId.Error("Incoming message is expected to have a nonce, but the return_to parameter is not signed.");
				}

				string nonceValue = response.GetReturnToArgument(Protocol.ReturnToNonceParameter);
				ErrorUtilities.VerifyProtocol(
					nonceValue != null && response.ReturnToParametersSignatureValidated,
					this.securitySettings.RejectUnsolicitedAssertions ? OpenIdStrings.UnsolicitedAssertionsNotAllowed : OpenIdStrings.UnsolicitedAssertionsNotAllowedFrom1xOPs);

				CustomNonce nonce = CustomNonce.Deserialize(nonceValue);
				DateTime expirationDate = nonce.CreationDateUtc + MaximumMessageAge;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, message);
				}

				IReplayProtectedProtocolMessage replayResponse = response;
				if (!this.nonceStore.StoreNonce(ReturnToNonceContext, nonce.RandomPartAsString, nonce.CreationDateUtc)) {
					Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", replayResponse.Nonce, replayResponse.UtcCreationDate);
					throw new ReplayedMessageException(message);
				}

				return ReplayProtectionTask;
			}

			return NullTask;
		}

		#endregion

		/// <summary>
		/// Determines whether a request nonce should be applied the request
		/// or should be expected in the response.
		/// </summary>
		/// <param name="message">The authentication request or the positive assertion response.</param>
		/// <returns>
		/// <c>true</c> if the message exchanged with an OpenID 1.x provider
		/// or if unsolicited assertions should be rejected at the RP; otherwise <c>false</c>.
		/// </returns>
		private bool UseRequestNonce(IMessage message) {
			return message != null && (this.securitySettings.RejectUnsolicitedAssertions ||
				(message.Version.Major < 2 && this.securitySettings.ProtectDownlevelReplayAttacks));
		}

		/// <summary>
		/// A special DotNetOpenAuth-only nonce used by the RP when talking to 1.0 OPs in order
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
				Requires.NotNullOrEmpty(value, "value");

				byte[] nonce = MessagingUtilities.FromBase64WebSafeString(value);
				Assumes.True(nonce != null);
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
				string base64Nonce = MessagingUtilities.ConvertToBase64WebSafeString(nonce);
				return base64Nonce;
			}
		}
	}
}
