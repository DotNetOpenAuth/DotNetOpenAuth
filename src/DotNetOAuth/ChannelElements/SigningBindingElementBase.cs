//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	internal abstract class SigningBindingElementBase : IChannelBindingElement {
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
				this.Sign(signedMessage);
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
		/// <param name="httpMethod">
		/// The HTTP method to be used in sending the request.
		/// </param>
		/// <param name="additionalParameters">
		/// Parameters outside the OAuth message that are appended to the query string 
		/// or included in a POST entity where the content-type is application/x-www-form-urlencoded.
		/// </param>
		/// <returns>The signature base string.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 9.1.
		/// </remarks>
		protected static string ConstructSignatureBaseString(ITamperResistantOAuthMessage message, string httpMethod, IDictionary<string, string> additionalParameters) {
			if (String.IsNullOrEmpty(httpMethod)) {
				throw new ArgumentNullException("httpMethod");
			}

			List<string> signatureBaseStringElements = new List<string>(3);

			signatureBaseStringElements.Add(httpMethod.ToUpperInvariant());

			UriBuilder endpoint = new UriBuilder(message.Recipient);
			endpoint.Query = null;
			endpoint.Fragment = null;
			signatureBaseStringElements.Add(endpoint.Uri.AbsoluteUri);

			// TODO: figure out whether parameters passed by other means in the same HttpWebRequest
			// must also be signed.
			var encodedDictionary = OAuthChannel.GetEncodedParameters(message);
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
		/// Applies a signature to the message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		protected abstract void Sign(ITamperResistantOAuthMessage message);

		/// <summary>
		/// Validates the signature on a message.
		/// Does NOT throw an exception on failing signature verification.
		/// </summary>
		/// <param name="message">The message with a signature to verify.</param>
		/// <returns>True if the signature is valid.  False otherwise.</returns>
		protected abstract bool IsSignatureValid(ITamperResistantOAuthMessage message);

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
