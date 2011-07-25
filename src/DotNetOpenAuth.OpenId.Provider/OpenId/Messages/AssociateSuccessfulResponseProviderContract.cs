namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId.Provider;

	[ContractClassFor(typeof(AssociateSuccessfulResponseProvider))]
	internal abstract class AssociateSuccessfulResponseProviderContract : AssociateSuccessfulResponseProvider {
		/// <summary>
		/// Prevents a default instance of the <see cref="AssociateSuccessfulResponseProviderContract"/> class from being created.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		private AssociateSuccessfulResponseProviderContract(Version version, AssociateRequest request)
			: base(version, request) {
		}

		protected internal override Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			throw new NotImplementedException();
		}
	}
}
