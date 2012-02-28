//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedResponseProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// An unencrypted association response as it is sent by the Provider.
	/// </summary>
	internal class AssociateUnencryptedResponseProvider : AssociateUnencryptedResponse, IAssociateSuccessfulResponseProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnencryptedResponseProvider"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		internal AssociateUnencryptedResponseProvider(Version version, AssociateUnencryptedRequest request)
			: base(version, request) {
		}

		/// <summary>
		/// Gets or sets the lifetime, in seconds, of this association. The Relying Party MUST NOT use the association after this time has passed.
		/// </summary>
		/// <value>
		/// An integer, represented in base 10 ASCII.
		/// </value>
		long IAssociateSuccessfulResponseProvider.ExpiresIn {
			get { return this.ExpiresIn; }
			set { this.ExpiresIn = value; }
		}

		/// <summary>
		/// Gets or sets the association handle is used as a key to refer to this association in subsequent messages.
		/// </summary>
		/// <value>
		/// A string 255 characters or less in length. It MUST consist only of ASCII characters in the range 33-126 inclusive (printable non-whitespace characters).
		/// </value>
		string IAssociateSuccessfulResponseProvider.AssociationHandle {
			get { return this.AssociationHandle; }
			set { this.AssociationHandle = value; }
		}

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <param name="associationStore">The Provider's association store.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		/// <remarks>
		///   <para>The caller will update this message's
		///   <see cref="AssociateSuccessfulResponse.ExpiresIn"/> and
		///   <see cref="AssociateSuccessfulResponse.AssociationHandle"/>
		/// properties based on the <see cref="Association"/> returned by this method, but any other
		/// association type specific properties must be set by this method.</para>
		///   <para>The response message is updated to include the details of the created association by this method,
		/// but the resulting association is <i>not</i> added to the association store and must be done by the caller.</para>
		/// </remarks>
		public Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Association association = HmacShaAssociationProvider.Create(Protocol, this.AssociationType, AssociationRelyingPartyType.Smart, associationStore, securitySettings);
			this.MacKey = association.SecretKey;
			return association;
		}
	}
}
