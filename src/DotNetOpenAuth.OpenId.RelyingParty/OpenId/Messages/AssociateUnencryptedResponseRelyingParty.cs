//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedResponseRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A response to an unencrypted assocation request, as it is received by the relying party.
	/// </summary>
	internal class AssociateUnencryptedResponseRelyingParty : AssociateUnencryptedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnencryptedResponseRelyingParty"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		internal AssociateUnencryptedResponseRelyingParty(Version version, AssociateUnencryptedRequest request)
			: base(version, request) {
		}

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>The created association.</returns>
		protected Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Association association = HmacShaAssociation.Create(Protocol, this.AssociationType, this.AssociationHandle, this.MacKey, TimeSpan.FromSeconds(this.ExpiresIn));
			return association;
		}
	}
}
