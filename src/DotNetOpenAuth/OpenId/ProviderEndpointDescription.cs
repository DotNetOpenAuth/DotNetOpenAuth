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
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Describes some OpenID Provider endpoint and its capabilities.
	/// </summary>
	/// <remarks>
	/// This is an immutable type.
	/// </remarks>
	[Serializable]
	internal class ProviderEndpointDescription : IProviderEndpoint {
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

		#region IProviderEndpoint Properties

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version IProviderEndpoint.Version {
			get { return this.ProtocolVersion; }
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		Uri IProviderEndpoint.Uri {
			get { return this.Endpoint; }
		}

		#endregion

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
		internal ReadOnlyCollection<string> Capabilities { get; private set; }

		#region IProviderEndpoint Methods

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
		public bool IsExtensionSupported<T>() where T : IOpenIdMessageExtension, new() {
			T extension = new T();
			return this.IsExtensionSupported(extension);
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
		public bool IsExtensionSupported(Type extensionType) {
			ErrorUtilities.VerifyArgumentNotNull(extensionType, "extensionType");
			ErrorUtilities.VerifyArgument(typeof(IOpenIdMessageExtension).IsAssignableFrom(extensionType), OpenIdStrings.TypeMustImplementX, typeof(IOpenIdMessageExtension).FullName);
			var extension = (IOpenIdMessageExtension)Activator.CreateInstance(extensionType);
			return this.IsExtensionSupported(extension);
		}

		#endregion

		/// <summary>
		/// Determines whether some extension is supported by the Provider.
		/// </summary>
		/// <param name="extensionUri">The extension URI.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported; otherwise, <c>false</c>.
		/// </returns>
		protected internal bool IsExtensionSupported(string extensionUri) {
			ErrorUtilities.VerifyNonZeroLength(extensionUri, "extensionUri");
			ErrorUtilities.VerifyOperation(this.Capabilities != null, OpenIdStrings.ExtensionLookupSupportUnavailable);
			return this.Capabilities.Contains(extensionUri);
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <param name="extension">An instance of the extension to check support for.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		protected internal bool IsExtensionSupported(IOpenIdMessageExtension extension) {
			ErrorUtilities.VerifyArgumentNotNull(extension, "extension");

			// Consider the primary case.
			if (this.IsExtensionSupported(extension.TypeUri)) {
				return true;
			}

			// Consider the secondary cases.
			if (extension.AdditionalSupportedTypeUris != null) {
				if (extension.AdditionalSupportedTypeUris.Any(typeUri => this.IsExtensionSupported(typeUri))) {
					return true;
				}
			}

			return false;
		}
	}
}
