//-----------------------------------------------------------------------
// <copyright file="ProviderEndpointDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Describes some OpenID Provider endpoint and its capabilities.
	/// </summary>
	/// <remarks>
	/// This is an immutable type.
	/// </remarks>
	internal class ProviderEndpointDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderEndpointDescription"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The OpenID Provider endpoint URL.</param>
		/// <param name="openIdVersion">The OpenID version supported by this particular endpoint.</param>
		internal ProviderEndpointDescription(Uri providerEndpoint, Version openIdVersion) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			ErrorUtilities.VerifyArgumentNotNull(openIdVersion, "version");

			this.Endpoint = providerEndpoint;
			this.ProtocolVersion = openIdVersion;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderEndpointDescription"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The URI the provider listens on for OpenID requests.</param>
		/// <param name="serviceTypeURIs">The set of services offered by this endpoint.</param>
		internal ProviderEndpointDescription(Uri providerEndpoint, IEnumerable<string> serviceTypeURIs) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			ErrorUtilities.VerifyArgumentNotNull(serviceTypeURIs, "serviceTypeURIs");

			this.Endpoint = providerEndpoint;
			this.Capabilities = new ReadOnlyCollection<string>(serviceTypeURIs.ToList());

			Protocol opIdentifierProtocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, serviceTypeURIs);
			Protocol claimedIdentifierProviderVersion = Protocol.FindBestVersion(p => p.ClaimedIdentifierServiceTypeURI, serviceTypeURIs);
			if (opIdentifierProtocol != null) {
				this.ProtocolVersion = opIdentifierProtocol.Version;
			} else if (claimedIdentifierProviderVersion != null) {
				this.ProtocolVersion = claimedIdentifierProviderVersion.Version;
			}

			ErrorUtilities.VerifyProtocol(this.ProtocolVersion != null, OpenIdStrings.ProviderVersionUnrecognized, this.Endpoint);
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider listens for incoming OpenID messages on.
		/// </summary>
		internal Uri Endpoint { get; private set; }

		/// <summary>
		/// Gets the OpenID protocol version this endpoint supports.
		/// </summary>
		/// <remarks>
		/// If an endpoint supports multiple versions, each version must be represented
		/// by its own <see cref="ProviderEndpointDescription"/> object.
		/// </remarks>
		internal Version ProtocolVersion { get; private set; }

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		internal ReadOnlyCollection<string> Capabilities { get; private set;  }
	}
}
