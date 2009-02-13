//-----------------------------------------------------------------------
// <copyright file="AssociationManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Manages the establishment, storage and retrieval of associations at the relying party.
	/// </summary>
	internal class AssociationManager {
		/// <summary>
		/// The storage to use for saving and retrieving associations.  May be null.
		/// </summary>
		private readonly IAssociationStore<Uri> associationStore;

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
		internal AssociationManager(Channel channel, IAssociationStore<Uri> associationStore, RelyingPartySecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(channel, "channel");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

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
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
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
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
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
		/// Gets an association between this Relying Party and a given Provider
		/// if it already exists in the association store.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>The association if one exists and has useful life remaining.  Otherwise <c>null</c>.</returns>
		internal Association GetExistingAssociation(ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			// If the RP has no application store for associations, there's no point in creating one.
			if (this.associationStore == null) {
				return null;
			}

			Association association = this.associationStore.GetAssociation(provider.Endpoint, this.SecuritySettings);

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
		/// <returns>The existing or new association; <c>null</c> if none existed and one could not be created.</returns>
		internal Association GetOrCreateAssociation(ProviderEndpointDescription provider) {
			return this.GetExistingAssociation(provider) ?? this.CreateNewAssociation(provider);
		}

		/// <summary>
		/// Creates a new association with a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>
		/// The newly created association, or null if no association can be created with
		/// the given Provider given the current security settings.
		/// </returns>
		/// <remarks>
		/// A new association is created and returned even if one already exists in the
		/// association store.
		/// Any new association is automatically added to the <see cref="associationStore"/>.
		/// </remarks>
		private Association CreateNewAssociation(ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			// If there is no association store, there is no point in creating an association.
			if (this.associationStore == null) {
				return null;
			}

			var associateRequest = AssociateRequest.Create(this.securitySettings, provider);

			const int RenegotiateRetries = 1;
			return this.CreateNewAssociation(provider, associateRequest, RenegotiateRetries);
		}

		/// <summary>
		/// Creates a new association with a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <param name="associateRequest">The associate request.  May be <c>null</c>, which will always result in a <c>null</c> return value..</param>
		/// <param name="retriesRemaining">The number of times to try the associate request again if the Provider suggests it.</param>
		/// <returns>
		/// The newly created association, or null if no association can be created with
		/// the given Provider given the current security settings.
		/// </returns>
		private Association CreateNewAssociation(ProviderEndpointDescription provider, AssociateRequest associateRequest, int retriesRemaining) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			if (associateRequest == null || retriesRemaining < 0) {
				// this can happen if security requirements and protocol conflict
				// to where there are no association types to choose from.
				return null;
			}

			try {
				var associateResponse = this.channel.Request(associateRequest);
				var associateSuccessfulResponse = associateResponse as AssociateSuccessfulResponse;
				var associateUnsuccessfulResponse = associateResponse as AssociateUnsuccessfulResponse;
				if (associateSuccessfulResponse != null) {
					Association association = associateSuccessfulResponse.CreateAssociation(associateRequest, null);
					this.associationStore.StoreAssociation(provider.Endpoint, association);
					return association;
				} else if (associateUnsuccessfulResponse != null) {
					if (string.IsNullOrEmpty(associateUnsuccessfulResponse.AssociationType)) {
						Logger.Debug("Provider rejected an association request and gave no suggestion as to an alternative association type.  Giving up.");
						return null;
					}

					if (!this.securitySettings.IsAssociationInPermittedRange(Protocol.Lookup(provider.ProtocolVersion), associateUnsuccessfulResponse.AssociationType)) {
						Logger.DebugFormat("Provider rejected an association request and suggested '{0}' as an association to try, which this Relying Party does not support.  Giving up.", associateUnsuccessfulResponse.AssociationType);
						return null;
					}

					if (retriesRemaining <= 0) {
						Logger.Debug("Unable to agree on an association type with the Provider in the allowed number of retries.  Giving up.");
						return null;
					}

					// Make sure the Provider isn't suggesting an incompatible pair of association/session types.
					Protocol protocol = Protocol.Lookup(provider.ProtocolVersion);
					ErrorUtilities.VerifyProtocol(
						HmacShaAssociation.IsDHSessionCompatible(protocol, associateUnsuccessfulResponse.AssociationType, associateUnsuccessfulResponse.SessionType),
						OpenIdStrings.IncompatibleAssociationAndSessionTypes,
						associateUnsuccessfulResponse.AssociationType,
						associateUnsuccessfulResponse.SessionType);

					associateRequest = AssociateRequest.Create(this.securitySettings, provider, associateUnsuccessfulResponse.AssociationType, associateUnsuccessfulResponse.SessionType);
					return this.CreateNewAssociation(provider, associateRequest, retriesRemaining - 1);
				} else {
					throw new ProtocolException(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}
			} catch (ProtocolException ex) {
				// Since having associations with OPs is not totally critical, we'll log and eat
				// the exception so that auth may continue in dumb mode.
				Logger.ErrorFormat("An error occurred while trying to create an association with {0}.  {1}", provider.Endpoint, ex);
				return null;
			}
		}
	}
}
