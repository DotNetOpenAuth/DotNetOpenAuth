using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetOpenAuth.OpenId.Messages {
	internal class AssociateUnencryptedResponseRelyingParty : AssociateUnencryptedResponse {

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
