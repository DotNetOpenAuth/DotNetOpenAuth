//-----------------------------------------------------------------------
// <copyright file="AssociateRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association.
	/// </summary>
	[DebuggerDisplay("OpenID {ProtocolVersion} {Mode} {AssociationType} {SessionType}")]
	internal abstract class AssociateRequest : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		protected AssociateRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint, "associate", MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the preferred association type. The association type defines the algorithm to be used to sign subsequent messages. 
		/// </summary>
		/// <value>Value: A valid association type from Section 8.3.</value>
		[MessagePart("openid.assoc_type", IsRequired = true, AllowEmpty = false)]
		internal string AssociationType { get; set; }

		/// <summary>
		/// Gets or sets the preferred association session type. This defines the method used to encrypt the association's MAC key in transit. 
		/// </summary>
		/// <value>Value: A valid association session type from Section 8.4 (Association Session Types). </value>
		/// <remarks>Note: Unless using transport layer encryption, "no-encryption" MUST NOT be used. </remarks>
		[MessagePart("openid.session_type", IsRequired = true, AllowEmpty = true)]
		[MessagePart("openid.session_type", IsRequired = true, AllowEmpty = false, MinVersion = "2.0")]
		internal string SessionType { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		public override void EnsureValidMessage() {
			base.EnsureValidMessage();

			ErrorUtilities.VerifyProtocol(
				!string.Equals(this.SessionType, Protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal) || this.Recipient.IsTransportSecure(),
				OpenIdStrings.NoEncryptionSessionRequiresHttps,
				this);
		}

		/// <summary>
		/// Creates an association request message that is appropriate for a given Provider.
		/// </summary>
		/// <param name="securityRequirements">The set of requirements the selected association type must comply to.</param>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>
		/// The message to send to the Provider to request an association.
		/// Null if no association could be created that meet the security requirements
		/// and the provider OpenID version.
		/// </returns>
		internal static AssociateRequest Create(SecuritySettings securityRequirements, ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(securityRequirements, "securityRequirements");
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			AssociateRequest associateRequest;

			// Apply our knowledge of the endpoint's transport, OpenID version, and
			// security requirements to decide the best association 
			bool unencryptedAllowed = provider.Endpoint.IsTransportSecure();
			bool useDiffieHellman = !unencryptedAllowed;
			string associationType, sessionType;
			if (!HmacShaAssociation.TryFindBestAssociation(Protocol.Lookup(provider.ProtocolVersion), securityRequirements, useDiffieHellman, out associationType, out sessionType)) {
				// There are no associations that meet all requirements.
				Logger.Warn("Security requirements and protocol combination knock out all possible association types.  Dumb mode forced.");
				return null;
			}

			if (unencryptedAllowed) {
				associateRequest = new AssociateUnencryptedRequest(provider.ProtocolVersion, provider.Endpoint);
				associateRequest.AssociationType = associationType;
			} else {
				var diffieHellmanAssociateRequest = new AssociateDiffieHellmanRequest(provider.ProtocolVersion, provider.Endpoint);
				diffieHellmanAssociateRequest.AssociationType = associationType;
				diffieHellmanAssociateRequest.SessionType = sessionType;
				diffieHellmanAssociateRequest.InitializeRequest();
				associateRequest = diffieHellmanAssociateRequest;
			}

			return associateRequest;
		}

		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <param name="associationStore">The association store where a new association (if created) will be stored.  Must not be null.</param>
		/// <returns>
		/// The appropriate association response that is ready to be sent back to the Relying Party.
		/// </returns>
		/// <remarks>
		/// <para>If an association is created, it will be automatically be added to the provided
		/// association store.</para>
		/// <para>Successful association response messages will derive from <see cref="AssociateSuccessfulResponse"/>.
		/// Failed association response messages will derive from <see cref="AssociateUnsuccessfulResponse"/>.</para>
		/// </remarks>
		internal IProtocolMessage CreateResponse(IAssociationStore<AssociationRelyingPartyType> associationStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");

			var response = this.CreateResponseCore();

			// Create and store the association if this is a successful response.
			var successResponse = response as AssociateSuccessfulResponse;
			if (successResponse != null) {
				Association association = successResponse.CreateAssociation(this);
				associationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
			}

			return response;
		}

		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <returns>
		/// The appropriate association response message.
		/// </returns>
		/// <remarks>
		/// <para>If an association can be successfully created, the 
		/// <see cref="AssociateSuccessfulResponse.CreateAssociation"/> method must not be
		/// called by this method.</para>
		/// <para>Successful association response messages will derive from <see cref="AssociateSuccessfulResponse"/>.
		/// Failed association response messages will derive from <see cref="AssociateUnsuccessfulResponse"/>.</para>
		/// </remarks>
		protected abstract IProtocolMessage CreateResponseCore();
	}
}
