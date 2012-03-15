//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanProviderRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association that uses Diffie-Hellman encryption.
	/// </summary>
	internal class AssociateDiffieHellmanProviderRequest : AssociateDiffieHellmanRequest, IAssociateRequestProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanProviderRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal AssociateDiffieHellmanProviderRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint) {
		}

		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <returns>
		/// The appropriate association response message.
		/// </returns>
		public IProtocolMessage CreateResponseCore() {
			var response = new AssociateDiffieHellmanProviderResponse(this.Version, this);
			response.AssociationType = this.AssociationType;
			return response;
		}
	}
}
