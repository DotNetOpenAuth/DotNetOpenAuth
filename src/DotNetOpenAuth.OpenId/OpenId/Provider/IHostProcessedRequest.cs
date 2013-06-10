//-----------------------------------------------------------------------
// <copyright file="IHostProcessedRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Interface exposing incoming messages to the OpenID Provider that
	/// require interaction with the host site.
	/// </summary>
	public interface IHostProcessedRequest : IRequest {
		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion RelyingPartyVersion { get; }

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		Realm Realm { get; }

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		bool Immediate { get; }

		/// <summary>
		/// Gets or sets the provider endpoint claimed in the positive assertion.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// This value MUST match the value for the OP Endpoint in the discovery results for the
		/// claimed identifier being asserted in a positive response.
		/// </value>
		Uri ProviderEndpoint { get; set; }

		/// <summary>
		/// Attempts to perform relying party discovery of the return URL claimed by the Relying Party.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The details of how successful the relying party discovery was.
		/// </returns>
		/// <remarks>
		///   <para>Return URL verification is only attempted if this method is called.</para>
		///   <para>See OpenID Authentication 2.0 spec section 9.2.1.</para>
		/// </remarks>
		Task<RelyingPartyDiscoveryResult> IsReturnUrlDiscoverableAsync(IHostFactories hostFactories, CancellationToken cancellationToken = default(CancellationToken));
	}
}
