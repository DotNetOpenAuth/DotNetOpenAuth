namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Messages;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId;

	[ContractClassFor(typeof(AssociateSuccessfulResponseRelyingParty))]
	internal abstract class AssociateSuccessfulResponseRelyingPartyContract : AssociateSuccessfulResponseRelyingParty {
		protected override Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Contract.Requires<ArgumentNullException>(request != null);
			throw new NotImplementedException();
		}
	}
}
