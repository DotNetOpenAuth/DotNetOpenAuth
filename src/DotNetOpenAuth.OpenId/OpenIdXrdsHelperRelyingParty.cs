//-----------------------------------------------------------------------
// <copyright file="OpenIdXrdsHelperRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using Validation;

	/// <summary>
	/// Adds OpenID-specific extension methods to the XrdsDocument class.
	/// </summary>
	internal static class OpenIdXrdsHelperRelyingParty {
		/// <summary>
		/// Creates the service endpoints described in this document, useful for requesting
		/// authentication of one of the OpenID Providers that result from it.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="claimedIdentifier">The claimed identifier that was used to discover this XRDS document.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <returns>
		/// A sequence of OpenID Providers that can assert ownership of the <paramref name="claimedIdentifier"/>.
		/// </returns>
		internal static IEnumerable<IdentifierDiscoveryResult> CreateServiceEndpoints(this IEnumerable<XrdElement> xrds, UriIdentifier claimedIdentifier, UriIdentifier userSuppliedIdentifier) {
			Requires.NotNull(xrds, "xrds");
			Requires.NotNull(claimedIdentifier, "claimedIdentifier");
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");

			var endpoints = new List<IdentifierDiscoveryResult>();
			endpoints.AddRange(xrds.GenerateOPIdentifierServiceEndpoints(userSuppliedIdentifier));
			endpoints.AddRange(xrds.GenerateClaimedIdentifierServiceEndpoints(claimedIdentifier, userSuppliedIdentifier));

			Logger.Yadis.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Yadis.Debug(endpoints.ToStringDeferred(true).ToString());
			return endpoints;
		}

		/// <summary>
		/// Creates the service endpoints described in this document, useful for requesting
		/// authentication of one of the OpenID Providers that result from it.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="userSuppliedIdentifier">The user-supplied i-name that was used to discover this XRDS document.</param>
		/// <returns>A sequence of OpenID Providers that can assert ownership of the canonical ID given in this document.</returns>
		internal static IEnumerable<IdentifierDiscoveryResult> CreateServiceEndpoints(this IEnumerable<XrdElement> xrds, XriIdentifier userSuppliedIdentifier) {
			Requires.NotNull(xrds, "xrds");
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");

			var endpoints = new List<IdentifierDiscoveryResult>();
			endpoints.AddRange(xrds.GenerateOPIdentifierServiceEndpoints(userSuppliedIdentifier));
			endpoints.AddRange(xrds.GenerateClaimedIdentifierServiceEndpoints(userSuppliedIdentifier));
			Logger.Yadis.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Yadis.Debug(endpoints.ToStringDeferred(true).ToString());
			return endpoints;
		}

		/// <summary>
		/// Generates OpenID Providers that can authenticate using directed identity.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="opIdentifier">The OP Identifier entered (and resolved) by the user.  Essentially the user-supplied identifier.</param>
		/// <returns>A sequence of the providers that can offer directed identity services.</returns>
		private static IEnumerable<IdentifierDiscoveryResult> GenerateOPIdentifierServiceEndpoints(this IEnumerable<XrdElement> xrds, Identifier opIdentifier) {
			Requires.NotNull(xrds, "xrds");
			Requires.NotNull(opIdentifier, "opIdentifier");
			return from service in xrds.FindOPIdentifierServices()
				   from uri in service.UriElements
				   let protocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris)
				   let providerDescription = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select IdentifierDiscoveryResult.CreateForProviderIdentifier(opIdentifier, providerDescription, service.Priority, uri.Priority);
		}

		/// <summary>
		/// Generates the OpenID Providers that are capable of asserting ownership
		/// of a particular URI claimed identifier.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <returns>
		/// A sequence of the providers that can assert ownership of the given identifier.
		/// </returns>
		private static IEnumerable<IdentifierDiscoveryResult> GenerateClaimedIdentifierServiceEndpoints(this IEnumerable<XrdElement> xrds, UriIdentifier claimedIdentifier, UriIdentifier userSuppliedIdentifier) {
			Requires.NotNull(xrds, "xrds");
			Requires.NotNull(claimedIdentifier, "claimedIdentifier");

			return from service in xrds.FindClaimedIdentifierServices()
				   from uri in service.UriElements
				   where uri.Uri != null
				   let providerEndpoint = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select IdentifierDiscoveryResult.CreateForClaimedIdentifier(claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier, providerEndpoint, service.Priority, uri.Priority);
		}

		/// <summary>
		/// Generates the OpenID Providers that are capable of asserting ownership
		/// of a particular XRI claimed identifier.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="userSuppliedIdentifier">The i-name supplied by the user.</param>
		/// <returns>A sequence of the providers that can assert ownership of the given identifier.</returns>
		private static IEnumerable<IdentifierDiscoveryResult> GenerateClaimedIdentifierServiceEndpoints(this IEnumerable<XrdElement> xrds, XriIdentifier userSuppliedIdentifier) {
			// Cannot use code contracts because this method uses yield return.
			////Requires.NotNull(xrds, "xrds");
			////			ErrorUtilities.VerifyArgumentNotNull(xrds, "xrds");

			foreach (var service in xrds.FindClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (service.Xrd.CanonicalID == null) {
						Logger.Yadis.WarnFormat(XrdsStrings.MissingCanonicalIDElement, userSuppliedIdentifier);
						break; // skip on to next service
					}
					ErrorUtilities.VerifyProtocol(service.Xrd.IsCanonicalIdVerified, XrdsStrings.CIDVerificationFailed, userSuppliedIdentifier);

					// In the case of XRI names, the ClaimedId is actually the CanonicalID.
					var claimedIdentifier = new XriIdentifier(service.Xrd.CanonicalID);
					var providerEndpoint = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris);
					yield return IdentifierDiscoveryResult.CreateForClaimedIdentifier(claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier, providerEndpoint, service.Priority, uri.Priority);
				}
			}
		}

		/// <summary>
		/// Enumerates the XRDS service elements that describe OpenID Providers offering directed identity assertions.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of service elements.</returns>
		private static IEnumerable<ServiceElement> FindOPIdentifierServices(this IEnumerable<XrdElement> xrds) {
			Requires.NotNull(xrds, "xrds");

			return from xrd in xrds
				   from service in xrd.OpenIdProviderIdentifierServices
				   select service;
		}

		/// <summary>
		/// Returns the OpenID-compatible services described by a given XRDS document,
		/// in priority order.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of the services offered.</returns>
		private static IEnumerable<ServiceElement> FindClaimedIdentifierServices(this IEnumerable<XrdElement> xrds) {
			Requires.NotNull(xrds, "xrds");

			return from xrd in xrds
				   from service in xrd.OpenIdClaimedIdentifierServices
				   select service;
		}
	}
}
