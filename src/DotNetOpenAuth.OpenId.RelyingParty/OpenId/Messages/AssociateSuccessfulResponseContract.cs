namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	[ContractClassFor(typeof(AssociateSuccessfulResponseRelyingParty))]
	internal abstract class AssociateSuccessfulResponseRelyingPartyContract : AssociateSuccessfulResponseRelyingParty {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateSuccessfulResponseRelyingPartyContract"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		private AssociateSuccessfulResponseRelyingPartyContract(Version version, AssociateRequest request)
			: base(version, request) {
		}

		protected internal override Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Contract.Requires<ArgumentNullException>(request != null);
			throw new NotImplementedException();
		}
	}
}
