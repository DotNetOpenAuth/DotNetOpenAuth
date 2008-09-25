//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	internal abstract class SigningBindingElementBase : IChannelBindingElement {
		/// <summary>
		/// The signature method this binding element uses.
		/// </summary>
		private string signatureMethod;

		/// <summary>
		/// The delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// </summary>
		private readonly Action<ITamperResistantOAuthMessage> incomingMessageSignatureVerificationCallback;

		/// <summary>
		/// Initializes a new instance of the <see cref="SigningBindingElementBase"/> class.
		/// </summary>
		/// <param name="signatureMethod">The OAuth signature method that the binding element uses.</param>
		/// <param name="signatureVerificationCallback">
		/// The delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// May be null for Consumers (who never have to verify signatures).
		/// </param>
		internal SigningBindingElementBase(string signatureMethod, Action<ITamperResistantOAuthMessage> signatureVerificationCallback) {
			if (String.IsNullOrEmpty(signatureMethod)) {
				throw new ArgumentNullException("signatureMethod");
			}

			this.signatureMethod = signatureMethod;
			this.incomingMessageSignatureVerificationCallback = signatureVerificationCallback;
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the message protection provided by this binding element.
		/// </summary>
		public MessageProtection Protection {
			get { return MessageProtection.TamperProtection; }
		}

		/// <summary>
		/// Signs the outgoing message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>True if the message was signed.  False otherwise.</returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				signedMessage.SignatureMethod = this.signatureMethod;
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
			if (signedMessage != null) {
				if (!string.Equals(signedMessage.SignatureMethod, this.signatureMethod, StringComparison.Ordinal)) {
					Logger.ErrorFormat("Expected signature method '{0}' but received message with a signature method of '{1}'.", this.signatureMethod, signedMessage.SignatureMethod);
					throw new InvalidSignatureException(message);
				}

				if (this.incomingMessageSignatureVerificationCallback != null) {
					this.incomingMessageSignatureVerificationCallback(signedMessage);
				} else {
					throw new InvalidOperationException(MessagingStrings.SignatureVerificationCallbackMissing);
				}

				string signature = this.GetSignature(signedMessage);
				if (signedMessage.Signature != signature) {
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

			var encodedDictionary = OAuthChannel.GetEncodedParameters(message);
			OAuthChannel.EncodeParameters(message.AdditionalParametersInHttpRequest, encodedDictionary);
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

			return signatureBaseString.ToString();
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		protected abstract string GetSignature(ITamperResistantOAuthMessage message);

		/// <summary>
		/// Gets the ConsumerSecret&amp;TokenSecret" string, allowing either property to be empty or null.
		/// </summary>
		/// <param name="message">The message to extract the secrets from.</param>
		/// <returns>The concatenated string.</returns>
		protected string GetConsumerAndTokenSecretString(ITamperResistantOAuthMessage message) {
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
