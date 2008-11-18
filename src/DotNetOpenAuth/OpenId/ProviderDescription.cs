//-----------------------------------------------------------------------
// <copyright file="ProviderDescription.cs" company="Andrew Arnott">
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
		/// <param name="version">The OpenID version supported by this particular endpoint.</param>
		internal ProviderEndpointDescription(Uri providerEndpoint, Protocol version) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			ErrorUtilities.VerifyArgumentNotNull(version, "version");

			this.Endpoint = providerEndpoint;
			this.Protocol = version;
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
		internal Protocol Protocol { get; private set; }
	}
}
