//-----------------------------------------------------------------------
// <copyright file="IIdentifierDiscoveryService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net.Http;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// A module that provides discovery services for OpenID identifiers.
	/// </summary>
	public interface IIdentifierDiscoveryService {
		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By design")]
		Task<IdentifierDiscoveryServiceResult> DiscoverAsync(Identifier identifier, CancellationToken cancellationToken);
	}

	/// <summary>
	/// Describes the result of <see cref="IIdentifierDiscoveryService.DiscoverAsync"/>.
	/// </summary>
	public class IdentifierDiscoveryServiceResult {
		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierDiscoveryServiceResult" /> class.
		/// </summary>
		/// <param name="results">The results.</param>
		/// <param name="abortDiscoveryChain">if set to <c>true</c>, no further discovery services will be called for this identifier.</param>
		public IdentifierDiscoveryServiceResult(IEnumerable<IdentifierDiscoveryResult> results, bool abortDiscoveryChain = false) {
			Requires.NotNull(results, "results");

			this.Results = results;
			this.AbortDiscoveryChain = abortDiscoveryChain;
		}

		/// <summary>
		/// Gets the results from this individual discovery service.
		/// </summary>
		public IEnumerable<IdentifierDiscoveryResult> Results { get; private set; }

		/// <summary>
		/// Gets a value indicating whether no further discovery services should be called for this identifier.
		/// </summary>
		public bool AbortDiscoveryChain { get; private set; }
	}
}
