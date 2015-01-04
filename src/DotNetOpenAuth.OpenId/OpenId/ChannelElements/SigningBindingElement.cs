//-----------------------------------------------------------------------
// <copyright file="SigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Net.Security;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Signs and verifies authentication assertions.
	/// </summary>
	internal abstract class SigningBindingElement : IChannelBindingElement {
		/// <summary>
		/// A reusable pre-completed task that may be returned multiple times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> NullTask = Task.FromResult<MessageProtections?>(null);

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
		protected virtual bool IsOnProvider {
			get { return false; }
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
		public virtual Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
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
		public async Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.Bindings.DebugFormat("Verifying incoming {0} message signature of: {1}", message.GetType().Name, signedMessage.Signature);
				MessageProtections protectionsApplied = MessageProtections.TamperProtection;

				this.EnsureParametersRequiringSignatureAreSigned(signedMessage);

				Association association = this.GetSpecificAssociation(signedMessage);
				if (association != null) {
					string signature = this.GetSignature(signedMessage, association);
					if (!MessagingUtilities.EqualsConstantTime(signedMessage.Signature, signature)) {
						Logger.Bindings.Error("Signature verification failed.");
						throw new InvalidSignatureException(message);
					}
				} else {
					ErrorUtilities.VerifyInternal(this.Channel != null, "Cannot verify private association signature because we don't have a channel.");

					protectionsApplied = await this.VerifySignatureByUnrecognizedHandleAsync(message, signedMessage, protectionsApplied, cancellationToken);
				}

				return protectionsApplied;
			}

			return null;
		}

		/// <summary>
		/// Verifies the signature by unrecognized handle.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="signedMessage">The signed message.</param>
		/// <param name="protectionsApplied">The protections applied.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The applied protections.</returns>
		protected abstract Task<MessageProtections> VerifySignatureByUnrecognizedHandleAsync(IProtocolMessage message, ITamperResistantOpenIdMessage signedMessage, MessageProtections protectionsApplied, CancellationToken cancellationToken);

		#endregion

		/// <summary>
		/// Calculates the signature for a given message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <param name="association">The association to use to sign the message.</param>
		/// <returns>The calculated signature of the method.</returns>
		protected string GetSignature(ITamperResistantOpenIdMessage signedMessage, Association association) {
			Requires.NotNull(signedMessage, "signedMessage");
			Requires.That(!string.IsNullOrEmpty(signedMessage.SignedParameterOrder), "signedMessage", "SignedParameterOrder must not be null or empty.");
			Requires.NotNull(association, "association");

			// Prepare the parts to sign, taking care to replace an openid.mode value
			// of check_authentication with its original id_res so the signature matches.
			MessageDictionary dictionary = this.Channel.MessageDescriptions.GetAccessor(signedMessage);
			var parametersToSign = from name in signedMessage.SignedParameterOrder.Split(',')
			                       let prefixedName = Protocol.V20.openid.Prefix + name
			                       select new KeyValuePair<string, string>(name, dictionary.GetValueOrThrow(prefixedName, signedMessage));

			byte[] dataToSign = KeyValueFormEncoding.GetBytes(parametersToSign);
			string signature = Convert.ToBase64String(association.Sign(dataToSign));

			if (Logger.Signatures.IsDebugEnabled()) {
				Logger.Signatures.DebugFormat(
					"Signing these message parts: {0}{1}{0}Base64 representation of signed data: {2}{0}Signature: {3}",
					Environment.NewLine,
					parametersToSign.ToStringDeferred(),
					Convert.ToBase64String(dataToSign),
					signature);
			}

			return signature;
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>The association to use to sign or verify the message.</returns>
		protected abstract Association GetAssociation(ITamperResistantOpenIdMessage signedMessage);

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
		protected abstract Association GetSpecificAssociation(ITamperResistantOpenIdMessage signedMessage);

		/// <summary>
		/// Gets a private Provider association used for signing messages in "dumb" mode.
		/// </summary>
		/// <returns>An existing or newly created association.</returns>
		protected virtual Association GetDumbAssociationForSigning() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Ensures that all message parameters that must be signed are in fact included
		/// in the signature.
		/// </summary>
		/// <param name="signedMessage">The signed message.</param>
		private void EnsureParametersRequiringSignatureAreSigned(ITamperResistantOpenIdMessage signedMessage) {
			// Verify that the signed parameter order includes the mandated fields.
			// We do this in such a way that derived classes that add mandated fields automatically
			// get included in the list of checked parameters.
			Protocol protocol = Protocol.Lookup(signedMessage.Version);
			var partsRequiringProtection = from part in this.Channel.MessageDescriptions.Get(signedMessage).Mapping.Values
										   where part.RequiredProtection != ProtectionLevel.None
										   where part.IsRequired || part.IsNondefaultValueSet(signedMessage)
										   select part.Name;
			ErrorUtilities.VerifyInternal(partsRequiringProtection.All(name => name.StartsWith(protocol.openid.Prefix, StringComparison.Ordinal)), "Signing only works when the parameters start with the 'openid.' prefix.");
			string[] signedParts = signedMessage.SignedParameterOrder.Split(',');
			var unsignedParts = from partName in partsRequiringProtection
								where !signedParts.Contains(partName.Substring(protocol.openid.Prefix.Length))
								select partName;
			ErrorUtilities.VerifyProtocol(!unsignedParts.Any(), OpenIdStrings.SignatureDoesNotIncludeMandatoryParts, string.Join(", ", unsignedParts.ToArray()));
		}
	}
}
