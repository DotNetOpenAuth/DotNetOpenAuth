//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanProviderRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

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
			return new AssociateDiffieHellmanProviderResponse(this.Version, this);
		}
	}
}
