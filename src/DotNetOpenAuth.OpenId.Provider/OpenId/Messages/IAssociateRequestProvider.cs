//-----------------------------------------------------------------------
// <copyright file="IAssociateRequestProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The openid.mode=associate message as it is received at the OpenID Provider.
	/// </summary>
	internal interface IAssociateRequestProvider : IDirectedProtocolMessage {
		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <returns>
		/// The appropriate association response message.
		/// </returns>
		/// <remarks>
		/// <para>If an association can be successfully created, the 
		/// AssociateSuccessfulResponse.CreateAssociation method must not be
		/// called by this method.</para>
		/// <para>Successful association response messages will derive from <see cref="AssociateSuccessfulResponse"/>.
		/// Failed association response messages will derive from <see cref="AssociateUnsuccessfulResponse"/>.</para>
		/// </remarks>
		IProtocolMessage CreateResponseCore();
	}
}
