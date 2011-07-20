namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId.Provider;

	[ContractClassFor(typeof(AssociateSuccessfulResponseProvider))]
	internal abstract class AssociateSuccessfulResponseProviderContract : AssociateSuccessfulResponseProvider {
		protected override Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			throw new NotImplementedException();
		}
	}
}
