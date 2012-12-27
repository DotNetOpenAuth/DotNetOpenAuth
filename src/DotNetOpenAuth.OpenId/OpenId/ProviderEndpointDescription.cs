//-----------------------------------------------------------------------
// <copyright file="ProviderEndpointDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// Describes some OpenID Provider endpoint and its capabilities.
	/// </summary>
	/// <remarks>
	/// This is an immutable type.
	/// </remarks>
	[Serializable]
	internal sealed class ProviderEndpointDescription : IProviderEndpoint {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderEndpointDescription"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The OpenID Provider endpoint URL.</param>
		/// <param name="openIdVersion">The OpenID version supported by this particular endpoint.</param>
		internal ProviderEndpointDescription(Uri providerEndpoint, Version openIdVersion) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNull(openIdVersion, "openIdVersion");

			this.Uri = providerEndpoint;
			this.Version = openIdVersion;
			this.Capabilities = new ReadOnlyCollection<string>(EmptyList<string>.Instance);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderEndpointDescription"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The URI the provider listens on for OpenID requests.</param>
		/// <param name="serviceTypeURIs">The set of services offered by this endpoint.</param>
		internal ProviderEndpointDescription(Uri providerEndpoint, IEnumerable<string> serviceTypeURIs) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNull(serviceTypeURIs, "serviceTypeURIs");

			this.Uri = providerEndpoint;
			this.Capabilities = new ReadOnlyCollection<string>(serviceTypeURIs.ToList());

			Protocol opIdentifierProtocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, serviceTypeURIs);
			Protocol claimedIdentifierProviderVersion = Protocol.FindBestVersion(p => p.ClaimedIdentifierServiceTypeURI, serviceTypeURIs);
			if (opIdentifierProtocol != null) {
				this.Version = opIdentifierProtocol.Version;
			} else if (claimedIdentifierProviderVersion != null) {
				this.Version = claimedIdentifierProviderVersion.Version;
			} else {
				ErrorUtilities.ThrowProtocol(OpenIdStrings.ProviderVersionUnrecognized, this.Uri);
			}
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider listens for incoming OpenID messages on.
		/// </summary>
		public Uri Uri { get; private set; }

		/// <summary>
		/// Gets the OpenID protocol version this endpoint supports.
		/// </summary>
		/// <remarks>
		/// If an endpoint supports multiple versions, each version must be represented
		/// by its own <see cref="ProviderEndpointDescription"/> object.
		/// </remarks>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		internal ReadOnlyCollection<string> Capabilities { get; private set; }

		#region IProviderEndpoint Members

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IProviderEndpoint.IsExtensionSupported<T>() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IProviderEndpoint.IsExtensionSupported(Type extensionType) {
			throw new NotImplementedException();
		}

		#endregion

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Capabilities != null);
		}
#endif
	}
}
