//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponseProviderContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Code contract for the <see cref="AssociateSuccessfulResponseProvider"/> class.
	/// </summary>
	[ContractClassFor(typeof(AssociateSuccessfulResponseProvider))]
	internal abstract class AssociateSuccessfulResponseProviderContract : AssociateSuccessfulResponseProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateSuccessfulResponseProviderContract"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		private AssociateSuccessfulResponseProviderContract(Version version, AssociateRequest request)
			: base(version, request) {
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
		protected internal override Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			throw new NotImplementedException();
		}
	}
}
