//-----------------------------------------------------------------------
// <copyright file="IdentifierDiscoveryServices.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A service that can perform discovery on OpenID identifiers.
	/// </summary>
	internal class IdentifierDiscoveryServices {
		/// <summary>
		/// The RP or OP that is hosting these services.
		/// </summary>
		private readonly IOpenIdHost host;

		/// <summary>
		/// Backing field for the <see cref="DiscoveryServices"/> property.
		/// </summary>
		private readonly IList<IIdentifierDiscoveryService> discoveryServices = new List<IIdentifierDiscoveryService>(2);

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierDiscoveryServices"/> class.
		/// </summary>
		/// <param name="host">The RP or OP that creates this instance.</param>
		internal IdentifierDiscoveryServices(IOpenIdHost host) {
			Requires.NotNull(host, "host");

			this.host = host;
			this.discoveryServices.AddRange(OpenIdElement.Configuration.RelyingParty.DiscoveryServices.CreateInstances(true));
		}

		/// <summary>
		/// Gets the list of services that can perform discovery on identifiers given.
		/// </summary>
		public IList<IIdentifierDiscoveryService> DiscoveryServices {
			get { return this.discoveryServices; }
		}

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to discover services for.</param>
		/// <returns>A non-null sequence of services discovered for the identifier.</returns>
		public IEnumerable<IdentifierDiscoveryResult> Discover(Identifier identifier) {
			Requires.NotNull(identifier, "identifier");
			Contract.Ensures(Contract.Result<IEnumerable<IdentifierDiscoveryResult>>() != null);

			IEnumerable<IdentifierDiscoveryResult> results = Enumerable.Empty<IdentifierDiscoveryResult>();
			foreach (var discoverer in this.DiscoveryServices) {
				bool abortDiscoveryChain;
				var discoveryResults = discoverer.Discover(identifier, this.host.WebRequestHandler, out abortDiscoveryChain).CacheGeneratedResults();
				results = results.Concat(discoveryResults);
				if (abortDiscoveryChain) {
					Logger.OpenId.InfoFormat("Further discovery on '{0}' was stopped by the {1} discovery service.", identifier, discoverer.GetType().Name);
					break;
				}
			}

			// If any OP Identifier service elements were found, we must not proceed
			// to use any Claimed Identifier services, per OpenID 2.0 sections 7.3.2.2 and 11.2.
			// For a discussion on this topic, see
			// http://groups.google.com/group/dotnetopenid/browse_thread/thread/4b5a8c6b2210f387/5e25910e4d2252c8
			// Sometimes the IIdentifierDiscoveryService will automatically filter this for us, but
			// just to be sure, we'll do it here as well.
			if (!this.host.SecuritySettings.AllowDualPurposeIdentifiers) {
				results = results.CacheGeneratedResults(); // avoid performing discovery repeatedly
				var opIdentifiers = results.Where(result => result.ClaimedIdentifier == result.Protocol.ClaimedIdentifierForOPIdentifier);
				var claimedIdentifiers = results.Where(result => result.ClaimedIdentifier != result.Protocol.ClaimedIdentifierForOPIdentifier);
				results = opIdentifiers.Any() ? opIdentifiers : claimedIdentifiers;
			}

			return results;
		}
	}
}
