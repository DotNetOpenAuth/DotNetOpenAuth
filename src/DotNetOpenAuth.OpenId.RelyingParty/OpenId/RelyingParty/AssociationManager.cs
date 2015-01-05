//-----------------------------------------------------------------------
// <copyright file="AssociationManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Security;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Manages the establishment, storage and retrieval of associations at the relying party.
	/// </summary>
	internal class AssociationManager {
		/// <summary>
		/// The storage to use for saving and retrieving associations.  May be null.
		/// </summary>
		private readonly IRelyingPartyAssociationStore associationStore;

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private Channel channel;

		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationManager"/> class.
		/// </summary>
		/// <param name="channel">The channel the relying party is using.</param>
		/// <param name="associationStore">The association store.  May be null for dumb mode relying parties.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal AssociationManager(Channel channel, IRelyingPartyAssociationStore associationStore, RelyingPartySecuritySettings securitySettings) {
			Requires.NotNull(channel, "channel");
			Requires.NotNull(securitySettings, "securitySettings");

			this.channel = channel;
			this.associationStore = associationStore;
			this.securitySettings = securitySettings;
		}

		/// <summary>
		/// Gets or sets the channel to use for establishing associations.
		/// </summary>
		/// <value>The channel.</value>
		internal Channel Channel {
			get {
				return this.channel;
			}

			set {
				Requires.NotNull(value, "value");
				this.channel = value;
			}
		}

		/// <summary>
		/// Gets or sets the security settings to apply in choosing association types to support.
		/// </summary>
		internal RelyingPartySecuritySettings SecuritySettings {
			get {
				return this.securitySettings;
			}

			set {
				Requires.NotNull(value, "value");
				this.securitySettings = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has an association store.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the relying party can act in 'smart' mode;
		/// 	<c>false</c> if the relying party must always act in 'dumb' mode.
		/// </value>
		internal bool HasAssociationStore {
			get { return this.associationStore != null; }
		}

		/// <summary>
		/// Gets the storage to use for saving and retrieving associations.  May be null.
		/// </summary>
		internal IRelyingPartyAssociationStore AssociationStoreTestHook {
			get { return this.associationStore; }
		}

		/// <summary>
		/// Gets an association between this Relying Party and a given Provider
		/// if it already exists in the association store.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>The association if one exists and has useful life remaining.  Otherwise <c>null</c>.</returns>
		internal Association GetExistingAssociation(IProviderEndpoint provider) {
			Requires.NotNull(provider, "provider");

			// If the RP has no application store for associations, there's no point in creating one.
			if (this.associationStore == null) {
				return null;
			}

			Association association = this.associationStore.GetAssociation(provider.Uri, this.SecuritySettings);

			// If the returned association does not fulfill security requirements, ignore it.
			if (association != null && !this.SecuritySettings.IsAssociationInPermittedRange(association)) {
				association = null;
			}

			if (association != null && !association.HasUsefulLifeRemaining) {
				association = null;
			}

			return association;
		}

		/// <summary>
		/// Gets an existing association with the specified Provider, or attempts to create
		/// a new association of one does not already exist.
		/// </summary>
		/// <param name="provider">The provider to get an association for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The existing or new association; <c>null</c> if none existed and one could not be created.</returns>
		internal async Task<Association> GetOrCreateAssociationAsync(IProviderEndpoint provider, CancellationToken cancellationToken) {
			return this.GetExistingAssociation(provider) ?? await this.CreateNewAssociationAsync(provider, cancellationToken);
		}

		/// <summary>
		/// Creates a new association with a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The newly created association, or null if no association can be created with
		/// the given Provider given the current security settings.
		/// </returns>
		/// <remarks>
		/// A new association is created and returned even if one already exists in the
		/// association store.
		/// Any new association is automatically added to the <see cref="associationStore" />.
		/// </remarks>
		private async Task<Association> CreateNewAssociationAsync(IProviderEndpoint provider, CancellationToken cancellationToken) {
			Requires.NotNull(provider, "provider");

			// If there is no association store, there is no point in creating an association.
			if (this.associationStore == null) {
				return null;
			}

			try {
				var associateRequest = AssociateRequestRelyingParty.Create(this.securitySettings, provider);

				const int RenegotiateRetries = 1;
				return await this.CreateNewAssociationAsync(provider, associateRequest, RenegotiateRetries, cancellationToken);
			} catch (VerificationException ex) {
				// See Trac ticket #163.  In partial trust host environments, the
				// Diffie-Hellman implementation we're using for HTTP OP endpoints
				// sometimes causes the CLR to throw:
				// "VerificationException: Operation could destabilize the runtime."
				// Just give up and use dumb mode in this case.
				Logger.OpenId.ErrorFormat("VerificationException occurred while trying to create an association with {0}.  {1}", provider.Uri, ex);
				return null;
			}
		}

		/// <summary>
		/// Creates a new association with a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <param name="associateRequest">The associate request.  May be <c>null</c>, which will always result in a <c>null</c> return value..</param>
		/// <param name="retriesRemaining">The number of times to try the associate request again if the Provider suggests it.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The newly created association, or null if no association can be created with
		/// the given Provider given the current security settings.
		/// </returns>
		/// <exception cref="ProtocolException">Create if an error occurs while creating the new association.</exception>
		private async Task<Association> CreateNewAssociationAsync(IProviderEndpoint provider, AssociateRequest associateRequest, int retriesRemaining, CancellationToken cancellationToken) {
			Requires.NotNull(provider, "provider");

			if (associateRequest == null || retriesRemaining < 0) {
				// this can happen if security requirements and protocol conflict
				// to where there are no association types to choose from.
				return null;
			}

			Exception exception = null;
			try {
				var associateResponse = await this.channel.RequestAsync(associateRequest, cancellationToken);
				var associateSuccessfulResponse = associateResponse as IAssociateSuccessfulResponseRelyingParty;
				var associateUnsuccessfulResponse = associateResponse as AssociateUnsuccessfulResponse;
				if (associateSuccessfulResponse != null) {
					Association association = associateSuccessfulResponse.CreateAssociationAtRelyingParty(associateRequest);
					this.associationStore.StoreAssociation(provider.Uri, association);
					return association;
				} else if (associateUnsuccessfulResponse != null) {
					if (string.IsNullOrEmpty(associateUnsuccessfulResponse.AssociationType)) {
						Logger.OpenId.Debug("Provider rejected an association request and gave no suggestion as to an alternative association type.  Giving up.");
						return null;
					}

					if (!this.securitySettings.IsAssociationInPermittedRange(Protocol.Lookup(provider.Version), associateUnsuccessfulResponse.AssociationType)) {
						Logger.OpenId.DebugFormat("Provider rejected an association request and suggested '{0}' as an association to try, which this Relying Party does not support.  Giving up.", associateUnsuccessfulResponse.AssociationType);
						return null;
					}

					if (retriesRemaining <= 0) {
						Logger.OpenId.Debug("Unable to agree on an association type with the Provider in the allowed number of retries.  Giving up.");
						return null;
					}

					// Make sure the Provider isn't suggesting an incompatible pair of association/session types.
					Protocol protocol = Protocol.Lookup(provider.Version);
					ErrorUtilities.VerifyProtocol(
						HmacShaAssociation.IsDHSessionCompatible(protocol, associateUnsuccessfulResponse.AssociationType, associateUnsuccessfulResponse.SessionType),
						OpenIdStrings.IncompatibleAssociationAndSessionTypes,
						associateUnsuccessfulResponse.AssociationType,
						associateUnsuccessfulResponse.SessionType);

					associateRequest = AssociateRequestRelyingParty.Create(this.securitySettings, provider, associateUnsuccessfulResponse.AssociationType, associateUnsuccessfulResponse.SessionType);
					return await this.CreateNewAssociationAsync(provider, associateRequest, retriesRemaining - 1, cancellationToken);
				} else {
					throw new ProtocolException(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}
			} catch (ProtocolException ex) {
				exception = ex;
			}

			Assumes.NotNull(exception);

			// If the association failed because the remote server can't handle Expect: 100 Continue headers,
			// then our web request handler should have already accomodated for future calls.  Go ahead and
			// immediately make one of those future calls now to try to get the association to succeed.
			if (UntrustedWebRequestHandler.IsExceptionFrom417ExpectationFailed(exception)) {
				return await this.CreateNewAssociationAsync(provider, associateRequest, retriesRemaining - 1, cancellationToken);
			}

			// Since having associations with OPs is not totally critical, we'll log and eat
			// the exception so that auth may continue in dumb mode.
			Logger.OpenId.ErrorFormat("An error occurred while trying to create an association with {0}.  {1}", provider.Uri, exception);
			return null;
		}
	}
}
