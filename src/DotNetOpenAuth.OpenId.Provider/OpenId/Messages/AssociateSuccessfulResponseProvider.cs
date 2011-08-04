//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponseProvider.cs" company="Andrew Arnott">
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

	[ContractClass(typeof(AssociateSuccessfulResponseProviderContract))]
	internal abstract class AssociateSuccessfulResponseProvider : AssociateSuccessfulResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateSuccessfulResponseProvider"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="request">The request.</param>
		internal AssociateSuccessfulResponseProvider(Version version, AssociateRequest request) :
			base(version, request) {
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
		/// <remarks>
		///   <para>The caller will update this message's <see cref="ExpiresIn"/> and <see cref="AssociationHandle"/>
		/// properties based on the <see cref="Association"/> returned by this method, but any other
		/// association type specific properties must be set by this method.</para>
		///   <para>The response message is updated to include the details of the created association by this method,
		/// but the resulting association is <i>not</i> added to the association store and must be done by the caller.</para>
		/// </remarks>
		protected internal abstract Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings);
	}
}
