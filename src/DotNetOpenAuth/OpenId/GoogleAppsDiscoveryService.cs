//-----------------------------------------------------------------------
// <copyright file="GoogleAppsDiscoveryService.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The discovery service to support Google Apps for Domains' proprietary discovery.
	/// </summary>
	/// <remarks>
	/// The spec for this discovery mechanism can be found at:
	/// http://groups.google.com/group/google-federated-login-api/web/openid-discovery-for-hosted-domains
	/// and the XMLDSig spec referenced in that spec can be found at:
	/// http://wiki.oasis-open.org/xri/XrdOne/XmlDsigProfile
	/// </remarks>
	public class GoogleAppsDiscoveryService : IIdentifierDiscoveryService {
		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleAppsDiscoveryService"/> class.
		/// </summary>
		public GoogleAppsDiscoveryService() {
		}

		#region IIdentifierDiscoveryService Members

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="requestHandler">The means to place outgoing HTTP requests.</param>
		/// <param name="abortDiscoveryChain">if set to <c>true</c>, no further discovery services will be called for this identifier.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		public IEnumerable<IdentifierDiscoveryResult> Discover(Identifier identifier, IDirectWebRequestHandler requestHandler, out bool abortDiscoveryChain) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
