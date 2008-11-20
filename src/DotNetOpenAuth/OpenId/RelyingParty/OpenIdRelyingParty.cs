//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	public sealed class OpenIdRelyingParty {
		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="associationStore">The association store.  If null, the relying party will always operate in "dumb mode".</param>
		public OpenIdRelyingParty(IAssociationStore<Uri> associationStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");

			this.Channel = new OpenIdChannel();
			this.AssociationStore = associationStore;
			this.SecuritySettings = RelyingPartySection.Configuration.SecuritySettings.CreateSecuritySettings();
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the security settings used by this Relying Party.
		/// </summary>
		public RelyingPartySecuritySettings SecuritySettings {
			get {
				return this.securitySettings;
			}

			internal set {
				if (value == null) {
					throw new ArgumentNullException("value");
				}

				this.securitySettings = value;
			}
		}

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IAssociationStore<Uri> AssociationStore { get; private set; }

		/// <summary>
		/// Gets an association between this Relying Party and a given Provider.
		/// A new association is created if necessary and possible.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>The association if one exists and/or could be created.  Null otherwise.</returns>
		internal Association GetAssociation(ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			var associateRequest = AssociateRequest.Create(this.SecuritySettings, provider);
			if (associateRequest == null) {
				return null;
			}

			var associateResponse = this.Channel.Request(associateRequest);
			var associateSuccessfulResponse = associateResponse as AssociateSuccessfulResponse;
			var associateUnsuccessfulResponse = associateResponse as AssociateUnsuccessfulResponse;
			if (associateSuccessfulResponse != null) {
				Association association = associateSuccessfulResponse.CreateAssociation(associateRequest);
				this.AssociationStore.StoreAssociation(provider.Endpoint, association);
				return association;
			} else if (associateUnsuccessfulResponse != null) {
				// TODO: code here
				throw new NotImplementedException();
			} else {
				throw new ProtocolException(MessagingStrings.UnexpectedMessageReceivedOfMany);
			}
		}
	}
}
