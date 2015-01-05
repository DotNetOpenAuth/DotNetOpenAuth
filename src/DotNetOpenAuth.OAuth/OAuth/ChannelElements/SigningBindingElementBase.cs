//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		public Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null && this.IsMessageApplicable(signedMessage)) {
				if (this.SignatureCallback != null) {
					this.SignatureCallback(signedMessage);
				} else {
					Logger.Bindings.Warn("Signing required, but callback delegate was not provided to provide additional data for signing.");
				}

				signedMessage.SignatureMethod = this.signatureMethod;
				Logger.Bindings.DebugFormat("Signing {0} message using {1}.", message.GetType().Name, this.signatureMethod);
				signedMessage.Signature = this.GetSignature(signedMessage);
				return MessageProtectionTasks.TamperProtection;
			}

			return MessageProtectionTasks.Null;
		}

		/// <summary>
		/// Verifies the signature on an incoming message.
		/// </summary>
		/// <param name="message">The message whose signature should be verified.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="InvalidSignatureException">Thrown if the signature is invalid.</exception>
		public Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null && this.IsMessageApplicable(signedMessage)) {
				Logger.Bindings.DebugFormat("Verifying incoming {0} message signature of: {1}", message.GetType().Name, signedMessage.Signature);

				if (!string.Equals(signedMessage.SignatureMethod, this.signatureMethod, StringComparison.Ordinal)) {
					Logger.Bindings.WarnFormat("Expected signature method '{0}' but received message with a signature method of '{1}'.", this.signatureMethod, signedMessage.SignatureMethod);
					return MessageProtectionTasks.None;
				}

				if (this.SignatureCallback != null) {
					this.SignatureCallback(signedMessage);
				} else {
					Logger.Bindings.Warn("Signature verification required, but callback delegate was not provided to provide additional data for signature verification.");
				}

				if (!this.IsSignatureValid(signedMessage)) {
					Logger.Bindings.Error("Signature verification failed.");
					throw new InvalidSignatureException(message);
				}

				return MessageProtectionTasks.TamperProtection;
			}

			return MessageProtectionTasks.Null;
		}

		#endregion

		/// <summary>
		/// Constructs the OAuth Signature Base String and returns the result.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="messageDictionary">The message to derive the signature base string from.</param>
		/// <returns>The signature base string.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 9.1.
		/// </remarks>
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unavoidable")]
		internal static string ConstructSignatureBaseString(ITamperResistantOAuthMessage message, MessageDictionary messageDictionary) {
			Requires.NotNull(message, "message");
			Requires.NotNull(message.HttpMethod, "message.HttpMethod");
			Requires.NotNull(messageDictionary, "messageDictionary");
			ErrorUtilities.VerifyInternal(messageDictionary.Message == message, "Message references are not equal.");

			List<string> signatureBaseStringElements = new List<string>(3);

			signatureBaseStringElements.Add(message.HttpMethod.ToString().ToUpperInvariant());

			// For multipart POST messages, only include the message parts that are NOT
			// in the POST entity (those parts that may appear in an OAuth authorization header).
			var encodedDictionary = new Dictionary<string, string>();
			IEnumerable<KeyValuePair<string, string>> partsToInclude = Enumerable.Empty<KeyValuePair<string, string>>();
			var binaryMessage = message as IMessageWithBinaryData;
			if (binaryMessage != null && binaryMessage.SendAsMultipart) {
				HttpDeliveryMethods authHeaderInUseFlags = HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest;
				ErrorUtilities.VerifyProtocol((binaryMessage.HttpMethods & authHeaderInUseFlags) == authHeaderInUseFlags, OAuthStrings.MultipartPostMustBeUsedWithAuthHeader);

				// Include the declared keys in the signature as those will be signable.
				// Cache in local variable to avoid recalculating DeclaredKeys in the delegate.
				ICollection<string> declaredKeys = messageDictionary.DeclaredKeys;
				partsToInclude = messageDictionary.Where(pair => declaredKeys.Contains(pair.Key));
			} else {
				partsToInclude = messageDictionary;
			}

			// If this message was deserialized, include only those explicitly included message parts (excludes defaulted values)
			// in the signature.
			var originalPayloadMessage = (IMessageOriginalPayload)message;
			if (originalPayloadMessage.OriginalPayload != null) {
				partsToInclude = partsToInclude.Where(pair => originalPayloadMessage.OriginalPayload.ContainsKey(pair.Key));
			}

			foreach (var pair in OAuthChannel.GetUriEscapedParameters(partsToInclude)) {
				encodedDictionary[pair.Key] = pair.Value;
			}

			// An incoming message will already have included the query and form parameters
			// in the message dictionary, but an outgoing message COULD have SOME parameters
			// in the query that are not in the message dictionary because they were included
			// in the receiving endpoint (the original URL).
			// In an outgoing message, the POST entity can only contain parameters if they were
			// in the message dictionary, so no need to pull out any parameters from there.
			if (message.Recipient.Query != null) {
				NameValueCollection nvc = HttpUtility.ParseQueryString(message.Recipient.Query);
				foreach (string key in nvc) {
					string escapedKey = MessagingUtilities.EscapeUriDataStringRfc3986(key);
					string escapedValue = MessagingUtilities.EscapeUriDataStringRfc3986(nvc[key]);
					string existingValue;
					if (!encodedDictionary.TryGetValue(escapedKey, out existingValue)) {
						encodedDictionary.Add(escapedKey, escapedValue);
					} else {
						ErrorUtilities.VerifyInternal(escapedValue == existingValue, "Somehow we have conflicting values for the '{0}' parameter.", escapedKey);
					}
				}
			}
			encodedDictionary.Remove("oauth_signature");

			UriBuilder endpoint = new UriBuilder(message.Recipient);
			endpoint.Query = null;
			endpoint.Fragment = null;
			signatureBaseStringElements.Add(endpoint.Uri.AbsoluteUri);

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

				signatureBaseString.Append(MessagingUtilities.EscapeUriDataStringRfc3986(element));
			}

			Logger.Bindings.DebugFormat("Constructed signature base string: {0}", signatureBaseString);
			return signatureBaseString.ToString();
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		/// <remarks>
		/// This method signs the message per OAuth 1.0 section 9.2.
		/// </remarks>
		internal string GetSignatureTestHook(ITamperResistantOAuthMessage message) {
			return this.GetSignature(message);
		}

		/// <summary>
		/// Gets the "ConsumerSecret&amp;TokenSecret" string, allowing either property to be empty or null.
		/// </summary>
		/// <param name="message">The message to extract the secrets from.</param>
		/// <returns>The concatenated string.</returns>
		protected static string GetConsumerAndTokenSecretString(ITamperResistantOAuthMessage message) {
			StringBuilder builder = new StringBuilder();
			if (!string.IsNullOrEmpty(message.ConsumerSecret)) {
				builder.Append(MessagingUtilities.EscapeUriDataStringRfc3986(message.ConsumerSecret));
			}
			builder.Append("&");
			if (!string.IsNullOrEmpty(message.TokenSecret)) {
				builder.Append(MessagingUtilities.EscapeUriDataStringRfc3986(message.TokenSecret));
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
			Requires.NotNull(message, "message");

			string signature = this.GetSignature(message);
			return MessagingUtilities.EqualsConstantTime(message.Signature, signature);
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
