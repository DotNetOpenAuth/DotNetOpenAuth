//-----------------------------------------------------------------------
// <copyright file="RelyingPartySigningBindingElement.cs" company="Outercurve Foundation">
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

	/// <summary>
	/// The signing binding element for OpenID Relying Parties.
	/// </summary>
	internal class RelyingPartySigningBindingElement : SigningBindingElement {
		/// <summary>
		/// The association store used by Relying Parties to look up the secrets needed for signing.
		/// </summary>
		private readonly IRelyingPartyAssociationStore rpAssociations;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartySigningBindingElement"/> class.
		/// </summary>
		/// <param name="associationStore">The association store used to look up the secrets needed for signing.  May be null for dumb Relying Parties.</param>
		internal RelyingPartySigningBindingElement(IRelyingPartyAssociationStore associationStore) {
			this.rpAssociations = associationStore;
		}

		/// <summary>
		/// Gets a specific association referenced in a given message's association handle.
		/// </summary>
		/// <param name="signedMessage">The signed message whose association handle should be used to lookup the association to return.</param>
		/// <returns>
		/// The referenced association; or <c>null</c> if such an association cannot be found.
		/// </returns>
		protected override Association GetSpecificAssociation(ITamperResistantOpenIdMessage signedMessage) {
			Association association = null;

			if (!string.IsNullOrEmpty(signedMessage.AssociationHandle)) {
				IndirectSignedResponse indirectSignedMessage = signedMessage as IndirectSignedResponse;
				if (this.rpAssociations != null) { // if on a smart RP
					Uri providerEndpoint = indirectSignedMessage.ProviderEndpoint;
					association = this.rpAssociations.GetAssociation(providerEndpoint, signedMessage.AssociationHandle);
				}
			}

			return association;
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>
		/// The association to use to sign or verify the message.
		/// </returns>
		protected override Association GetAssociation(ITamperResistantOpenIdMessage signedMessage) {
			// We're on a Relying Party verifying a signature.
			IDirectedProtocolMessage directedMessage = (IDirectedProtocolMessage)signedMessage;
			if (this.rpAssociations != null) {
				return this.rpAssociations.GetAssociation(directedMessage.Recipient, signedMessage.AssociationHandle);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Verifies the signature by unrecognized handle.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="signedMessage">The signed message.</param>
		/// <param name="protectionsApplied">The protections applied.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The applied protections.
		/// </returns>
		protected override async Task<MessageProtections> VerifySignatureByUnrecognizedHandleAsync(IProtocolMessage message, ITamperResistantOpenIdMessage signedMessage, MessageProtections protectionsApplied, CancellationToken cancellationToken) {
			// We did not recognize the association the provider used to sign the message.
			// Ask the provider to check the signature then.
			var indirectSignedResponse = (IndirectSignedResponse)signedMessage;
			var checkSignatureRequest = new CheckAuthenticationRequest(indirectSignedResponse, this.Channel);
			var checkSignatureResponse = await this.Channel.RequestAsync<CheckAuthenticationResponse>(checkSignatureRequest, cancellationToken);
			if (!checkSignatureResponse.IsValid) {
				Logger.Bindings.Error("Provider reports signature verification failed.");
				throw new InvalidSignatureException(message);
			}

			// If the OP confirms that a handle should be invalidated as well, do that.
			if (!string.IsNullOrEmpty(checkSignatureResponse.InvalidateHandle)) {
				if (this.rpAssociations != null) {
					this.rpAssociations.RemoveAssociation(indirectSignedResponse.ProviderEndpoint, checkSignatureResponse.InvalidateHandle);
				}
			}

			// When we're in dumb mode we can't provide our own replay protection,
			// but for OpenID 2.0 Providers we can rely on them providing it as part
			// of signature verification.
			if (message.Version.Major >= 2) {
				protectionsApplied |= MessageProtections.ReplayProtection;
			}

			return protectionsApplied;
		}
	}
}
