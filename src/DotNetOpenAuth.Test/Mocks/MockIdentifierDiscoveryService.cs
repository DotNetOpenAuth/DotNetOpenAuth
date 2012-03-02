//-----------------------------------------------------------------------
// <copyright file="MockIdentifierDiscoveryService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	internal class MockIdentifierDiscoveryService : IIdentifierDiscoveryService {
		/// <summary>
		/// Initializes a new instance of the <see cref="MockIdentifierDiscoveryService"/> class.
		/// </summary>
		public MockIdentifierDiscoveryService() {
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
			var mockIdentifier = identifier as MockIdentifier;
			if (mockIdentifier == null) {
				abortDiscoveryChain = false;
				return Enumerable.Empty<IdentifierDiscoveryResult>();
			}

			abortDiscoveryChain = true;
			return mockIdentifier.DiscoveryEndpoints;
		}

		#endregion
	}
}
