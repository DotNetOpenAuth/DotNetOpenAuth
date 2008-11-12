//-----------------------------------------------------------------------
// <copyright file="SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// Signs and verifies authentication assertions.
	/// </summary>
	/// <typeparam name="TKey">
	/// <see cref="System.Uri"/> for consumers (to distinguish associations across servers) or
	/// <see cref="AssociationRelyingPartyType"/> for providers (to distinguish dumb and smart client associations).
	/// </typeparam>
	internal class SigningBindingElement<TKey> : IChannelBindingElement {
		/// <summary>
		/// The association store used to look up the secrets needed for signing.
		/// </summary>
		private IAssociationStore<TKey> associations;

		/// <summary>
		/// Initializes a new instance of the SigningBindingElement class.
		/// </summary>
		/// <param name="associations">The association store used to look up the secrets needed for signing.</param>
		internal SigningBindingElement(IAssociationStore<TKey> associations) {
			ErrorUtilities.VerifyArgumentNotNull(associations, "associations");

			this.associations = associations;
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.TamperProtection"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.TamperProtection; }
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
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.DebugFormat("Signing {0} message.", message.GetType().Name);
				if (string.IsNullOrEmpty(signedMessage.AssociationHandle)) {
					// TODO: code here
					////signedMessage.AssociationHandle = 
				}
				signedMessage.SignedParameterOrder = this.GetSignedParameterOrder(signedMessage);
				signedMessage.Signature = this.GetSignature(signedMessage);
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
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.DebugFormat("Verifying incoming {0} message signature of: {1}", message.GetType().Name, signedMessage.Signature);

				string signature = this.GetSignature(signedMessage);
				if (!string.Equals(signedMessage.Signature, signature, StringComparison.Ordinal)) {
					Logger.Error("Signature verification failed.");
					throw new InvalidSignatureException(message);
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Gets the value to use for the openid.signed parameter.
		/// </summary>
		/// <param name="signedMessage">The signable message.</param>
		/// <returns>
		/// A comma-delimited list of parameter names, omitting the 'openid.' prefix, that determines
		/// the inclusion and order of message parts that will be signed.
		/// </returns>
		private string GetSignedParameterOrder(ITamperResistantOpenIdMessage signedMessage) {
			ErrorUtilities.VerifyArgumentNotNull(signedMessage, "signedMessage");

			MessageDescription description = MessageDescription.Get(signedMessage.GetType());
			var signedParts = from part in description.Mapping.Values
							where (part.RequiredProtection & System.Net.Security.ProtectionLevel.Sign) != 0
							select part.Name;
			string prefix = Protocol.V20.openid.Prefix;
			Debug.Assert(signedParts.All(name => name.StartsWith(prefix, StringComparison.Ordinal)), "All signed message parts must start with 'openid.'.");
			int skipLength = prefix.Length;
			string signedFields = string.Join(",", signedParts.Select(name => name.Substring(skipLength)).ToArray());
			return signedFields;
		}

		/// <summary>
		/// Calculates the signature for a given message.
		/// </summary>
		/// <param name="signedMessage">The message to sign.</param>
		/// <returns>The calculated signature of the method.</returns>
		private string GetSignature(ITamperResistantOpenIdMessage signedMessage) {
			ErrorUtilities.VerifyArgumentNotNull(signedMessage, "signedMessage");
			ErrorUtilities.VerifyNonZeroLength(signedMessage.SignedParameterOrder, "signedMessage.SignedParameterOrder");

			MessageDictionary dictionary = new MessageDictionary(signedMessage);
			var parametersToSign = from name in signedMessage.SignedParameterOrder.Split(',')
								   let prefixedName = Protocol.V20.openid.Prefix + name
								   select new KeyValuePair<string, string>(prefixedName, dictionary[prefixedName]);

			KeyValueFormEncoding keyValueForm = new KeyValueFormEncoding();
			byte[] dataToSign = keyValueForm.GetBytes(parametersToSign);

			Association association = null; // TODO: fetch the association to use
			string signature = Convert.ToBase64String(association.Sign(dataToSign));
			return signature;
		}
	}
}
