//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponseRelyingPartyContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Code contract for the <see cref="AssociateSuccessfulResponseRelyingParty"/> class.
	/// </summary>
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

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		protected internal override Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			Contract.Requires<ArgumentNullException>(request != null);
			throw new NotImplementedException();
		}
	}
}
