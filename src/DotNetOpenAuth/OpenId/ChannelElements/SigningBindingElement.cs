//-----------------------------------------------------------------------
// <copyright file="SigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Net.Security;
	using DotNetOpenAuth.Loggers;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

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
		/// The security settings at the Provider.
		/// Only defined when this element is instantiated to service a Provider.
		/// </summary>
		private readonly ProviderSecuritySettings opSecuritySettings;

		/// <summary>
		/// A logger specifically used for logging verbose text on everything about the signing process.
		/// </summary>
		private static ILog signingLogger = Logger.Create(typeof(SigningBindingElement));

		/// <summary>
		/// Initializes a new instance of the SigningBindingElement class for use by a Relying Party.
		/// </summary>
		/// <param name="associationStore">The association store used to look up the secrets needed for signing.  May be null for dumb Relying Parties.</param>
		internal SigningBindingElement(IAssociationStore<Uri> associationStore) {
			this.rpAssociations = associationStore;
		}

		/// <summary>
		/// Initializes a new instance of the SigningBindingElement class for use by a Provider.
		/// </summary>
		/// <param name="associationStore">The association store used to look up the secrets needed for signing.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal SigningBindingElement(IAssociationStore<AssociationRelyingPartyType> associationStore, ProviderSecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			this.opAssociations = associationStore;
			this.opSecuritySettings = securitySettings;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.TamperProtection"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.TamperProtection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		#endregion

		/// <summary>
		/// Gets a value indicating whether this binding element is on a Provider channel.
		/// </summary>
		private bool IsOnProvider {
			get { return this.opAssociations != null; }
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
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.DebugFormat("Signing {0} message.", message.GetType().Name);
				Association association = this.GetAssociation(signedMessage);
				signedMessage.AssociationHandle = association.Handle;
				signedMessage.SignedParameterOrder = this.GetSignedParameterOrder(signedMessage);
				signedMessage.Signature = GetSignature(signedMessage, association);
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

				EnsureParametersRequiringSignatureAreSigned(signedMessage);

				Association association = this.GetSpecificAssociation(signedMessage);
				if (association != null) {
					string signature = GetSignature(signedMessage, association);
					if (!string.Equals(signedMessage.Signature, signature, StringComparison.Ordinal)) {
						Logger.Error("Signature verification failed.");
						throw new InvalidSignatureException(message);
					}
				} else {
					ErrorUtilities.VerifyInternal(this.Channel != null, "Cannot verify private association signature because we don't have a channel.");

					// We did not recognize the association the provider used to sign the message.
					// Ask the provider to check the signature then.
					var indirectSignedResponse = (IndirectSignedResponse)signedMessage;
					var checkSignatureRequest = new CheckAuthenticationRequest(indirectSignedResponse);
					var checkSignatureResponse = this.Channel.Request<CheckAuthenticationResponse>(checkSignatureRequest);
					if (!checkSignatureResponse.IsValid) {
						Logger.Error("Provider reports signature verification failed.");
						throw new InvalidSignatureException(message);
					}

					// If the OP confirms that a handle should be invalidated as well, do that.
					if (!string.IsNullOrEmpty(checkSignatureResponse.InvalidateHandle)) {
						if (this.rpAssociations != null) {
							this.rpAssociations.RemoveAssociation(indirectSignedResponse.ProviderEndpoint, checkSignatureResponse.InvalidateHandle);
						}
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Ensures that all message parameters that must be signed are in fact included
		/// in the signature.
		/// </summary>
		/// <param name="signedMessage">The signed message.</param>
		private static void EnsureParametersRequiringSignatureAreSigned(ITamperResistantOpenIdMessage signedMessage) {
			// Verify that the signed parameter order includes the mandated fields.
			// We do this in such a way that derived classes that add mandated fields automatically
			// get included in the list of checked parameters.
			Protocol protocol = Protocol.Lookup(signedMessage.Version);
			var partsRequiringProtection = from part in MessageDescription.Get(signedMessage.GetType(), signedMessage.Version).Mapping.Values
			                               where part.RequiredProtection != ProtectionLevel.None
			                               select part.Name;
			ErrorUtilities.VerifyInternal(partsRequiringProtection.All(name => name.StartsWith(protocol.openid.Prefix, StringComparison.Ordinal)), "Signing only works when the parameters start with the 'openid.' prefix.");
			string[] signedParts = signedMessage.SignedParameterOrder.Split(',');
			var unsignedParts = from partName in partsRequiringProtection
			                    where !signedParts.Contains(partName.Substring(protocol.openid.Prefix.Length))
			                    select partName;
			ErrorUtilities.VerifyProtocol(!unsignedParts.Any(), OpenIdStrings.SignatureDoesNotIncludeMandatoryParts, string.Join(", ", unsignedParts.ToArray()));
		}

		/// <summary>
		/// Calculates the signature for a given message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <param name="association">The association to use to sign the message.</param>
		/// <returns>The calculated signature of the method.</returns>
		private static string GetSignature(ITamperResistantOpenIdMessage signedMessage, Association association) {
			ErrorUtilities.VerifyArgumentNotNull(signedMessage, "signedMessage");
			ErrorUtilities.VerifyNonZeroLength(signedMessage.SignedParameterOrder, "signedMessage.SignedParameterOrder");
			ErrorUtilities.VerifyArgumentNotNull(association, "association");

			// Prepare the parts to sign, taking care to replace an openid.mode value
			// of check_authentication with its original id_res so the signature matches.
			MessageDictionary dictionary = new MessageDictionary(signedMessage);
			var parametersToSign = from name in signedMessage.SignedParameterOrder.Split(',')
								   let prefixedName = Protocol.V20.openid.Prefix + name
								   select new KeyValuePair<string, string>(name, dictionary[prefixedName]);

			byte[] dataToSign = KeyValueFormEncoding.GetBytes(parametersToSign);
			string signature = Convert.ToBase64String(association.Sign(dataToSign));

			if (signingLogger.IsDebugEnabled) {
				signingLogger.DebugFormat(
					CultureInfo.InvariantCulture,
					"Signing these message parts: {0}{1}{0}Base64 representation of signed data: {2}{0}Signature: {3}",
					Environment.NewLine,
					parametersToSign.ToStringDeferred(),
					Convert.ToBase64String(dataToSign),
					signature);
			}

			return signature;
		}

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

			Protocol protocol = Protocol.Lookup(signedMessage.Version);

			MessageDescription description = MessageDescription.Get(signedMessage.GetType(), signedMessage.Version);
			var signedParts = from part in description.Mapping.Values
			                  where (part.RequiredProtection & System.Net.Security.ProtectionLevel.Sign) != 0
			                        && part.GetValue(signedMessage) != null
			                  select part.Name;
			string prefix = Protocol.V20.openid.Prefix;
			ErrorUtilities.VerifyInternal(signedParts.All(name => name.StartsWith(prefix, StringComparison.Ordinal)), "All signed message parts must start with 'openid.'.");

			if (this.opSecuritySettings.SignOutgoingExtensions) {
				// Tack on any ExtraData parameters that start with 'openid.'.
				List<string> extraSignedParameters = new List<string>(signedMessage.ExtraData.Count);
				foreach (string key in signedMessage.ExtraData.Keys) {
					if (key.StartsWith(protocol.openid.Prefix, StringComparison.Ordinal)) {
						extraSignedParameters.Add(key);
					} else {
						Logger.DebugFormat("The extra parameter '{0}' will not be signed because it does not start with 'openid.'.", key);
					}
				}
				signedParts = signedParts.Concat(extraSignedParameters);
			}

			int skipLength = prefix.Length;
			string signedFields = string.Join(",", signedParts.Select(name => name.Substring(skipLength)).ToArray());
			return signedFields;
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>The association to use to sign or verify the message.</returns>
		private Association GetAssociation(ITamperResistantOpenIdMessage signedMessage) {
			if (this.IsOnProvider) {
				// We're on a Provider to either sign (smart/dumb) or verify a dumb signature.
				return this.GetSpecificAssociation(signedMessage) ?? this.GetDumbAssociationForSigning();
			} else {
				// We're on a Relying Party verifying a signature.
				IDirectedProtocolMessage directedMessage = (IDirectedProtocolMessage)signedMessage;
				if (this.rpAssociations != null) {
					return this.rpAssociations.GetAssociation(directedMessage.Recipient, signedMessage.AssociationHandle);
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Gets a specific association referenced in a given message's association handle.
		/// </summary>
		/// <param name="signedMessage">The signed message whose association handle should be used to lookup the association to return.</param>
		/// <returns>The referenced association; or <c>null</c> if such an association cannot be found.</returns>
		/// <remarks>
		/// If the association handle set in the message does not match any valid association,
		/// the association handle property is cleared, and the 
		/// <see cref="ITamperResistantOpenIdMessage.InvalidateHandle"/> property is set to the
		/// handle that could not be found.
		/// </remarks>
		private Association GetSpecificAssociation(ITamperResistantOpenIdMessage signedMessage) {
			Association association = null;

			if (!string.IsNullOrEmpty(signedMessage.AssociationHandle)) {
				IndirectSignedResponse indirectSignedMessage = signedMessage as IndirectSignedResponse;
				if (this.IsOnProvider) {
					// Since we have an association handle, we're either signing with a smart association,
					// or verifying a dumb one.
					bool signing = string.IsNullOrEmpty(signedMessage.Signature);
					AssociationRelyingPartyType type = signing ? AssociationRelyingPartyType.Smart : AssociationRelyingPartyType.Dumb;
					association = this.opAssociations.GetAssociation(type, signedMessage.AssociationHandle);
					if (association == null) {
						// There was no valid association with the requested handle.
						// Let's tell the RP to forget about that association.
						signedMessage.InvalidateHandle = signedMessage.AssociationHandle;
						signedMessage.AssociationHandle = null;
					}
				} else if (this.rpAssociations != null) { // if on a smart RP
					Uri providerEndpoint = indirectSignedMessage.ProviderEndpoint;
					association = this.rpAssociations.GetAssociation(providerEndpoint, signedMessage.AssociationHandle);
				}
			}

			return association;
		}

		/// <summary>
		/// Gets a private Provider association used for signing messages in "dumb" mode.
		/// </summary>
		/// <returns>An existing or newly created association.</returns>
		private Association GetDumbAssociationForSigning() {
			// If no assoc_handle was given or it was invalid, the only thing 
			// left to do is sign a message using a 'dumb' mode association.
			Protocol protocol = Protocol.Default;
			Association association = this.opAssociations.GetAssociation(AssociationRelyingPartyType.Dumb, this.opSecuritySettings);
			if (association == null) {
				association = HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.HMAC_SHA256, AssociationRelyingPartyType.Dumb, this.opSecuritySettings);
				this.opAssociations.StoreAssociation(AssociationRelyingPartyType.Dumb, association);
			}

			return association;
		}
	}
}
