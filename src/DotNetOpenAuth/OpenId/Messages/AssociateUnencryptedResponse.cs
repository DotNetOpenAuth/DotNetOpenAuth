//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// The successful unencrypted association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.2.
	/// </remarks>
	internal class AssociateUnencryptedResponse : AssociateSuccessfulResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnencryptedResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateUnencryptedResponse(Version responseVersion, AssociateUnencryptedRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
			SessionType = Protocol.Args.SessionType.NoEncryption;
		}

		/// <summary>
		/// Gets or sets the MAC key (shared secret) for this association, Base 64 (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” .) [RFC3548] encoded. 
		/// </summary>
		[MessagePart("mac_key", IsRequired = true, AllowEmpty = false)]
		internal byte[] MacKey { get; set; }

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>The created association.</returns>
		/// <remarks>
		/// 	<para>The caller will update this message's
		/// <see cref="AssociateSuccessfulResponse.ExpiresIn"/> and
		/// <see cref="AssociateSuccessfulResponse.AssociationHandle"/>
		/// properties based on the <see cref="Association"/> returned by this method, but any other
		/// association type specific properties must be set by this method.</para>
		/// 	<para>The response message is updated to include the details of the created association by this method,
		/// but the resulting association is <i>not</i> added to the association store and must be done by the caller.</para>
		/// </remarks>
		protected override Association CreateAssociationAtProvider(AssociateRequest request, ProviderSecuritySettings securitySettings) {
			Association association = HmacShaAssociation.Create(Protocol, this.AssociationType, AssociationRelyingPartyType.Smart, securitySettings);
			this.MacKey = association.SecretKey;
			return association;
		}

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>The created association.</returns>
		protected override Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Association association = HmacShaAssociation.Create(Protocol, this.AssociationType, this.AssociationHandle, this.MacKey, TimeSpan.FromSeconds(this.ExpiresIn));
			return association;
		}
	}
}
