//-----------------------------------------------------------------------
// <copyright file="ProviderSigningBindingElement.cs" company="Outercurve Foundation">
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
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// The signing binding element for OpenID Providers.
	/// </summary>
	internal class ProviderSigningBindingElement : SigningBindingElement {
		/// <summary>
		/// The association store used by Providers to look up the secrets needed for signing.
		/// </summary>
		private readonly IProviderAssociationStore opAssociations;

		/// <summary>
		/// The security settings at the Provider.
		/// Only defined when this element is instantiated to service a Provider.
		/// </summary>
		private readonly ProviderSecuritySettings opSecuritySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderSigningBindingElement"/> class.
		/// </summary>
		/// <param name="associationStore">The association store used to look up the secrets needed for signing.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal ProviderSigningBindingElement(IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(associationStore, "associationStore");
			Requires.NotNull(securitySettings, "securitySettings");

			this.opAssociations = associationStore;
			this.opSecuritySettings = securitySettings;
		}

		/// <summary>
		/// Gets a value indicating whether this binding element is on a Provider channel.
		/// </summary>
		protected override bool IsOnProvider {
			get { return true; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		public override async Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var result = await base.ProcessOutgoingMessageAsync(message, cancellationToken);
			if (result != null) {
				return result;
			}

			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.Bindings.DebugFormat("Signing {0} message.", message.GetType().Name);
				Association association = this.GetAssociation(signedMessage);
				signedMessage.AssociationHandle = association.Handle;
				signedMessage.SignedParameterOrder = this.GetSignedParameterOrder(signedMessage);
				signedMessage.Signature = this.GetSignature(signedMessage, association);
				return MessageProtections.TamperProtection;
			}

			return null;
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>
		/// The association to use to sign or verify the message.
		/// </returns>
		protected override Association GetAssociation(ITamperResistantOpenIdMessage signedMessage) {
			// We're on a Provider to either sign (smart/dumb) or verify a dumb signature.
			bool signing = string.IsNullOrEmpty(signedMessage.Signature);

			if (signing) {
				// If the RP has no replay protection, coerce use of a private association 
				// instead of a shared one (if security settings indicate)
				// to protect the authenticating user from replay attacks.
				bool forcePrivateAssociation = this.opSecuritySettings.ProtectDownlevelReplayAttacks
					&& IsRelyingPartyVulnerableToReplays(null, (IndirectSignedResponse)signedMessage);

				if (forcePrivateAssociation) {
					if (!string.IsNullOrEmpty(signedMessage.AssociationHandle)) {
						Logger.Signatures.Info("An OpenID 1.x authentication request with a shared association handle will be responded to with a private association in order to provide OP-side replay protection.");
					}

					return this.GetDumbAssociationForSigning();
				} else {
					return this.GetSpecificAssociation(signedMessage) ?? this.GetDumbAssociationForSigning();
				}
			} else {
				return this.GetSpecificAssociation(signedMessage);
			}
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

				// Since we have an association handle, we're either signing with a smart association,
				// or verifying a dumb one.
				bool signing = string.IsNullOrEmpty(signedMessage.Signature);
				bool isPrivateAssociation = !signing;
				association = this.opAssociations.Deserialize(signedMessage, isPrivateAssociation, signedMessage.AssociationHandle);
				if (association == null) {
					// There was no valid association with the requested handle.
					// Let's tell the RP to forget about that association.
					signedMessage.InvalidateHandle = signedMessage.AssociationHandle;
					signedMessage.AssociationHandle = null;
				}
			}

			return association;
		}

		/// <summary>
		/// Gets a private Provider association used for signing messages in "dumb" mode.
		/// </summary>
		/// <returns>An existing or newly created association.</returns>
		protected override Association GetDumbAssociationForSigning() {
			// If no assoc_handle was given or it was invalid, the only thing 
			// left to do is sign a message using a 'dumb' mode association.
			Protocol protocol = Protocol.Default;
			Association association = HmacShaAssociationProvider.Create(protocol, protocol.Args.SignatureAlgorithm.HMAC_SHA256, AssociationRelyingPartyType.Dumb, this.opAssociations, this.opSecuritySettings);
			return association;
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
		protected override Task<MessageProtections> VerifySignatureByUnrecognizedHandleAsync(IProtocolMessage message, ITamperResistantOpenIdMessage signedMessage, MessageProtections protectionsApplied, CancellationToken cancellationToken) {
			// If we're on the Provider, then the RP sent us a check_auth with a signature
			// we don't have an association for.  (It may have expired, or it may be a faulty RP).
			var tcs = new TaskCompletionSource<MessageProtections>();
			tcs.SetException(new InvalidSignatureException(message));
			return tcs.Task;
		}

		/// <summary>
		/// Determines whether the relying party sending an authentication request is
		/// vulnerable to replay attacks.
		/// </summary>
		/// <param name="request">The request message from the Relying Party.  Useful, but may be null for conservative estimate results.</param>
		/// <param name="response">The response message to be signed.</param>
		/// <returns>
		/// 	<c>true</c> if the relying party is vulnerable; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsRelyingPartyVulnerableToReplays(SignedResponseRequest request, IndirectSignedResponse response) {
			Requires.NotNull(response, "response");

			// OpenID 2.0 includes replay protection as part of the protocol.
			if (response.Version.Major >= 2) {
				return false;
			}

			// This library's RP may be on the remote end, and may be using 1.x merely because
			// discovery on the Claimed Identifier suggested this was a 1.x OP.  
			// Since this library's RP has a built-in request_nonce parameter for replay
			// protection, we'll allow for that.
			var returnToArgs = HttpUtility.ParseQueryString(response.ReturnTo.Query);
			if (!string.IsNullOrEmpty(returnToArgs[Protocol.ReturnToNonceParameter])) {
				return false;
			}

			// If the OP endpoint _AND_ RP return_to URL uses HTTPS then no one
			// can steal and replay the positive assertion.
			// We can only ascertain this if the request message was handed to us
			// so we know what our own OP endpoint is.  If we don't have a request
			// message, then we'll default to assuming it's insecure.
			if (request != null) {
				if (request.Recipient.IsTransportSecure() && response.Recipient.IsTransportSecure()) {
					return false;
				}
			}

			// Nothing left to protect against replays.  RP is vulnerable.
			return true;
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
			RequiresEx.ValidState(this.Channel != null);
			Requires.NotNull(signedMessage, "signedMessage");

			Protocol protocol = Protocol.Lookup(signedMessage.Version);

			MessageDescription description = this.Channel.MessageDescriptions.Get(signedMessage);
			var signedParts =
				from part in description.Mapping.Values
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
						Logger.Signatures.DebugFormat("The extra parameter '{0}' will not be signed because it does not start with 'openid.'.", key);
					}
				}
				signedParts = signedParts.Concat(extraSignedParameters);
			}

			int skipLength = prefix.Length;
			string signedFields = string.Join(",", signedParts.Select(name => name.Substring(skipLength)).ToArray());
			return signedFields;
		}
	}
}
