//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public abstract class SigningBindingElementBase : ITamperProtectionChannelBindingElement {
		/// <summary>
		/// The signature method this binding element uses.
		/// </summary>
		private string signatureMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="SigningBindingElementBase"/> class.
		/// </summary>
		/// <param name="signatureMethod">The OAuth signature method that the binding element uses.</param>
		internal SigningBindingElementBase(string signatureMethod) {
			this.signatureMethod = signatureMethod;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the message protection provided by this binding element.
		/// </summary>
		public MessageProtections Protection {
			get { return MessageProtections.TamperProtection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		#endregion

		#region ITamperProtectionChannelBindingElement members

		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// </summary>
		public Action<ITamperResistantOAuthMessage> SignatureCallback { get; set; }

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		ITamperProtectionChannelBindingElement ITamperProtectionChannelBindingElement.Clone() {
			ITamperProtectionChannelBindingElement clone = this.Clone();
			clone.SignatureCallback = this.SignatureCallback;
			return clone;
		}

		#endregion

		#region IChannelBindingElement Methods

		/// <summary>
		/// Signs the outgoing message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>True if the message was signed.  False otherwise.</returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null && this.IsMessageApplicable(signedMessage)) {
				if (this.SignatureCallback != null) {
					this.SignatureCallback(signedMessage);
				} else {
					Logger.Warn("Signing required, but callback delegate was not provided to provide additional data for signing.");
				}

				signedMessage.SignatureMethod = this.signatureMethod;
				Logger.DebugFormat("Signing {0} message using {1}.", message.GetType().Name, this.signatureMethod);
				signedMessage.Signature = this.GetSignature(signedMessage);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Verifies the signature on an incoming message.
		/// </summary>
		/// <param name="message">The message whose signature should be verified.</param>
		/// <returns>True if the signature was verified.  False if the message had no signature.</returns>
		/// <exception cref="InvalidSignatureException">Thrown if the signature is invalid.</exception>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null && this.IsMessageApplicable(signedMessage)) {
				Logger.DebugFormat("Verifying incoming {0} message signature of: {1}", message.GetType().Name, signedMessage.Signature);

				if (!string.Equals(signedMessage.SignatureMethod, this.signatureMethod, StringComparison.Ordinal)) {
					Logger.WarnFormat("Expected signature method '{0}' but received message with a signature method of '{1}'.", this.signatureMethod, signedMessage.SignatureMethod);
					return false;
				}

				if (this.SignatureCallback != null) {
					this.SignatureCallback(signedMessage);
				} else {
					Logger.Warn("Signature verification required, but callback delegate was not provided to provide additional data for signature verification.");
				}

				if (!this.IsSignatureValid(signedMessage)) {
					Logger.Error("Signature verification failed.");
					throw new InvalidSignatureException(message);
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Constructs the OAuth Signature Base String and returns the result.
		/// </summary>
		/// <param name="message">The message to derive the signature base string from.</param>
		/// <returns>The signature base string.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 9.1.
		/// </remarks>
		protected static string ConstructSignatureBaseString(ITamperResistantOAuthMessage message) {
			if (String.IsNullOrEmpty(message.HttpMethod)) {
				throw new ArgumentException(
					string.Format(
					CultureInfo.CurrentCulture,
					MessagingStrings.ArgumentPropertyMissing,
					typeof(ITamperResistantOAuthMessage).Name,
					"HttpMethod"),
					"message");
			}

			List<string> signatureBaseStringElements = new List<string>(3);

			signatureBaseStringElements.Add(message.HttpMethod.ToUpperInvariant());

			UriBuilder endpoint = new UriBuilder(message.Recipient);
			endpoint.Query = null;
			endpoint.Fragment = null;
			signatureBaseStringElements.Add(endpoint.Uri.AbsoluteUri);

			var encodedDictionary = OAuthChannel.GetUriEscapedParameters(message);
			encodedDictionary.Remove("oauth_signature");
			var sortedKeyValueList = new List<KeyValuePair<string, string>>(encodedDictionary);
			sortedKeyValueList.Sort(SignatureBaseStringParameterComparer);
			StringBuilder paramBuilder = new StringBuilder();
			foreach (var pair in sortedKeyValueList) {
				if (paramBuilder.Length > 0) {
					paramBuilder.Append("&");
				}

				paramBuilder.Append(pair.Key);
				paramBuilder.Append('=');
				paramBuilder.Append(pair.Value);
			}

			signatureBaseStringElements.Add(paramBuilder.ToString());

			StringBuilder signatureBaseString = new StringBuilder();
			foreach (string element in signatureBaseStringElements) {
				if (signatureBaseString.Length > 0) {
					signatureBaseString.Append("&");
				}

				signatureBaseString.Append(Uri.EscapeDataString(element));
			}

			Logger.DebugFormat("Constructed signature base string: {0}", signatureBaseString);
			return signatureBaseString.ToString();
		}

		/// <summary>
		/// Gets the ConsumerSecret&amp;TokenSecret" string, allowing either property to be empty or null.
		/// </summary>
		/// <param name="message">The message to extract the secrets from.</param>
		/// <returns>The concatenated string.</returns>
		protected static string GetConsumerAndTokenSecretString(ITamperResistantOAuthMessage message) {
			StringBuilder builder = new StringBuilder();
			if (!string.IsNullOrEmpty(message.ConsumerSecret)) {
				builder.Append(Uri.EscapeDataString(message.ConsumerSecret));
			}
			builder.Append("&");
			if (!string.IsNullOrEmpty(message.TokenSecret)) {
				builder.Append(Uri.EscapeDataString(message.TokenSecret));
			}
			return builder.ToString();
		}

		/// <summary>
		/// Determines whether the signature on some message is valid.
		/// </summary>
		/// <param name="message">The message to check the signature on.</param>
		/// <returns>
		/// 	<c>true</c> if the signature on the message is valid; otherwise, <c>false</c>.
		/// </returns>
		protected virtual bool IsSignatureValid(ITamperResistantOAuthMessage message) {
			string signature = this.GetSignature(message);
			return message.Signature == signature;
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		/// <remarks>
		/// Implementations of this method need not clone the SignatureVerificationCallback member, as the
		/// <see cref="SigningBindingElementBase"/> class does this.
		/// </remarks>
		protected abstract ITamperProtectionChannelBindingElement Clone();

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		protected abstract string GetSignature(ITamperResistantOAuthMessage message);

		/// <summary>
		/// Checks whether this binding element applies to this message.
		/// </summary>
		/// <param name="message">The message that needs to be signed.</param>
		/// <returns>True if this binding element can be used to sign the message.  False otherwise.</returns>
		protected virtual bool IsMessageApplicable(ITamperResistantOAuthMessage message) {
			return string.IsNullOrEmpty(message.SignatureMethod) || message.SignatureMethod == this.signatureMethod;
		}

		/// <summary>
		/// Sorts parameters according to OAuth signature base string rules.
		/// </summary>
		/// <param name="left">The first parameter to compare.</param>
		/// <param name="right">The second parameter to compare.</param>
		/// <returns>Negative, zero or positive.</returns>
		private static int SignatureBaseStringParameterComparer(KeyValuePair<string, string> left, KeyValuePair<string, string> right) {
			int result = string.CompareOrdinal(left.Key, right.Key);
			if (result != 0) {
				return result;
			}

			return string.CompareOrdinal(left.Value, right.Value);
		}
	}
}
