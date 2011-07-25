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
		/// <summary>
		/// Prevents a default instance of the <see cref="AssociateSuccessfulResponseRelyingPartyContract"/> class from being created.
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
