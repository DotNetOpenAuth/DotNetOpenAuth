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
	using System.Threading.Tasks;
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		public Task<IdentifierDiscoveryServiceResult> DiscoverAsync(Identifier identifier, System.Threading.CancellationToken cancellationToken) {
			var mockIdentifier = identifier as MockIdentifier;
			if (mockIdentifier == null) {
				return Task.FromResult(new IdentifierDiscoveryServiceResult(Enumerable.Empty<IdentifierDiscoveryResult>(), abortDiscoveryChain: false));
			}

			return Task.FromResult(new IdentifierDiscoveryServiceResult(mockIdentifier.DiscoveryEndpoints, abortDiscoveryChain: true));
		}

		#endregion
	}
}
