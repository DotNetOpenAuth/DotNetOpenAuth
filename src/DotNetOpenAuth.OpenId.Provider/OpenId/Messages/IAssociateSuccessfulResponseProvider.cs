//-----------------------------------------------------------------------
// <copyright file="IAssociateSuccessfulResponseProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// An outgoing successful association response from the OpenID Provider.
	/// </summary>
	internal interface IAssociateSuccessfulResponseProvider : IProtocolMessage {
		/// <summary>
		/// Gets or sets the expires in.
		/// </summary>
		/// <value>
		/// The expires in.
		/// </value>
		long ExpiresIn { get; set; }

		/// <summary>
		/// Gets or sets the association handle.
		/// </summary>
		/// <value>
		/// The association handle.
		/// </value>
		string AssociationHandle { get; set; }

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
		///   <para>The caller will update this message's <see cref="AssociateSuccessfulResponse.ExpiresIn"/> and <see cref="AssociateSuccessfulResponse.AssociationHandle"/>
		/// properties based on the <see cref="Association"/> returned by this method, but any other
		/// association type specific properties must be set by this method.</para>
		///   <para>The response message is updated to include the details of the created association by this method,
		/// but the resulting association is <i>not</i> added to the association store and must be done by the caller.</para>
		/// </remarks>
		Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings);
	}
}
