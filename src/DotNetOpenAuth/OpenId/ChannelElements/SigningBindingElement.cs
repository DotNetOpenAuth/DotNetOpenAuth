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
	internal class SigningBindingElement : IChannelBindingElement {
		/// <summary>
		/// The association store used by Relying Parties to look up the secrets needed for signing.
		/// </summary>
		private readonly IAssociationStore<Uri> rpAssociations;

		/// <summary>
		/// The association store used by Providers to look up the secrets needed for signing.
		/// </summary>
		private readonly IAssociationStore<AssociationRelyingPartyType> opAssociations;

		/// <summary>
		/// Initializes a new instance of the SigningBindingElement class for use by a Relying Party.
		/// </summary>
		/// <param name="associations">The association store used to look up the secrets needed for signing.</param>
		internal SigningBindingElement(IAssociationStore<Uri> associations) {
			ErrorUtilities.VerifyArgumentNotNull(associations, "associations");

			this.rpAssociations = associations;
		}

		/// <summary>
		/// Initializes a new instance of the SigningBindingElement class for use by a Provider.
		/// </summary>
		/// <param name="associations">The association store used to look up the secrets needed for signing.</param>
		internal SigningBindingElement(IAssociationStore<AssociationRelyingPartyType> associations) {
			ErrorUtilities.VerifyArgumentNotNull(associations, "associations");

			this.opAssociations = associations;
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
				signedMessage.SignedParameterOrder = GetSignedParameterOrder(signedMessage);
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
		private static string GetSignedParameterOrder(ITamperResistantOpenIdMessage signedMessage) {
			ErrorUtilities.VerifyArgumentNotNull(signedMessage, "signedMessage");

			MessageDescription description = MessageDescription.Get(signedMessage.GetType(), signedMessage.ProtocolVersion);
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
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>The calculated signature of the method.</returns>
		private string GetSignature(ITamperResistantOpenIdMessage signedMessage) {
			ErrorUtilities.VerifyArgumentNotNull(signedMessage, "signedMessage");
			ErrorUtilities.VerifyNonZeroLength(signedMessage.SignedParameterOrder, "signedMessage.SignedParameterOrder");

			// Prepare the parts to sign, taking care to replace an openid.mode value
			// of check_authentication with its original id_res so the signature matches.
			Protocol protocol = Protocol.Lookup(signedMessage.ProtocolVersion);
			MessageDictionary dictionary = new MessageDictionary(signedMessage);
			var parametersToSign = from name in signedMessage.SignedParameterOrder.Split(',')
								   let prefixedName = Protocol.V20.openid.Prefix + name
								   let alteredValue = name == protocol.openidnp.mode && dictionary[prefixedName] == protocol.Args.Mode.check_authentication ? protocol.Args.Mode.id_res : dictionary[prefixedName]
								   select new KeyValuePair<string, string>(prefixedName, alteredValue);

			byte[] dataToSign = KeyValueFormEncoding.GetBytes(parametersToSign);

			Association association = this.GetAssociation(signedMessage);
			string signature = Convert.ToBase64String(association.Sign(dataToSign));
			return signature;
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>The association to use to sign or verify the message.</returns>
		private Association GetAssociation(ITamperResistantOpenIdMessage signedMessage) {
			if (this.rpAssociations != null) {
				// We're on a Relying Party verifying a signature.
				IDirectedProtocolMessage directedMessage = (IDirectedProtocolMessage)signedMessage;
				return this.rpAssociations.GetAssociation(directedMessage.Recipient, signedMessage.AssociationHandle);
			} else {
				// We're on a Provider to either sign (smart/dumb) or verify a dumb signature.
				if (string.IsNullOrEmpty(signedMessage.AssociationHandle)) {
					// Without an assoc_handle, the only thing we could possibly be doing
					// is signing a message using a 'dumb' mode association.
					return this.opAssociations.GetAssociation(AssociationRelyingPartyType.Dumb);
				} else {
					// Since we have an association handle, we're either signing with a smart association,
					// or verifying a dumb one.
					bool signing = string.IsNullOrEmpty(signedMessage.Signature);
					AssociationRelyingPartyType type = signing ? AssociationRelyingPartyType.Smart : AssociationRelyingPartyType.Dumb;
					return this.opAssociations.GetAssociation(type, signedMessage.AssociationHandle);
				}
			}
		}
	}
}
