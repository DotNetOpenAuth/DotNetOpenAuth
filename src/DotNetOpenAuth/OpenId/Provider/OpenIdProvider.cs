//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public sealed class OpenIdProvider {
		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private ProviderSecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="associationStore">The association store to use.  Cannot be null.</param>
		public OpenIdProvider(IAssociationStore<AssociationRelyingPartyType> associationStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");

			this.Channel = new OpenIdChannel();
			this.AssociationStore = associationStore;
			this.SecuritySettings = ProviderSection.Configuration.SecuritySettings.CreateSecuritySettings();
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the security settings used by this Provider.
		/// </summary>
		public ProviderSecuritySettings SecuritySettings {
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
		internal IAssociationStore<AssociationRelyingPartyType> AssociationStore { get; private set; }

		/// <summary>
		/// Responds automatically to the incoming message.
		/// </summary>
		/// <remarks>
		/// The design of a method like this is flawed... but it helps us get tests going for now.
		/// </remarks>
		internal void AutoRespond() {
			var request = this.Channel.ReadFromRequest();

			var associateRequest = request as AssociateRequest;
			if (associateRequest != null) {
				IProtocolMessage response = associateRequest.CreateResponse(this.AssociationStore);
				this.Channel.Send(response);
			} else {
				// TODO: code here
				throw new NotImplementedException();
			}
		}
	}
}
