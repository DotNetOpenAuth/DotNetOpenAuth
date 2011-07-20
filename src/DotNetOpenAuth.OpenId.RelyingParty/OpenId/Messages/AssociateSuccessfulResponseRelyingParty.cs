using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetOpenAuth.OpenId.Messages {
	internal abstract class AssociateSuccessfulResponseRelyingParty : AssociateSuccessfulResponse {
		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>The created association.</returns>
		protected abstract Association CreateAssociationAtRelyingParty(AssociateRequest request);
	}
}
