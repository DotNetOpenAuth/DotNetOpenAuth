//-----------------------------------------------------------------------
// <copyright file="OpenIdXrdsHelper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;

	/// <summary>
	/// Adds OpenID-specific extension methods to the XrdsDocument class.
	/// </summary>
	internal static class OpenIdXrdsHelper {
		/// <summary>
		/// Finds the Relying Party return_to receiving endpoints.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of Relying Party descriptors for the return_to endpoints.</returns>
		/// <remarks>
		/// This is useful for Providers to send unsolicited assertions to Relying Parties,
		/// or for Provider's to perform RP discovery/verification as part of authentication.
		/// </remarks>
		internal static IEnumerable<RelyingPartyEndpointDescription> FindRelyingPartyReceivingEndpoints(this XrdsDocument xrds) {
			return from service in xrds.FindReturnToServices()
				   from uri in service.UriElements
				   select new RelyingPartyEndpointDescription(uri.Uri, service.TypeElementUris);
		}

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
		internal static IEnumerable<ServiceEndpoint> CreateServiceEndpoints(this XrdsDocument xrds, UriIdentifier claimedIdentifier, UriIdentifier userSuppliedIdentifier) {
			var endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(xrds.GenerateOPIdentifierServiceEndpoints(claimedIdentifier));

			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(xrds.GenerateClaimedIdentifierServiceEndpoints(claimedIdentifier, userSuppliedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(endpoints.ToStringDeferred(true));
			return endpoints;
		}

		/// <summary>
		/// Creates the service endpoints described in this document, useful for requesting
		/// authentication of one of the OpenID Providers that result from it.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="userSuppliedIdentifier">The user-supplied i-name that was used to discover this XRDS document.</param>
		/// <returns>A sequence of OpenID Providers that can assert ownership of the canonical ID given in this document.</returns>
		internal static IEnumerable<ServiceEndpoint> CreateServiceEndpoints(this XrdsDocument xrds, XriIdentifier userSuppliedIdentifier) {
			var endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(xrds.GenerateOPIdentifierServiceEndpoints(userSuppliedIdentifier));

			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(xrds.GenerateClaimedIdentifierServiceEndpoints(userSuppliedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(endpoints.ToStringDeferred(true));
			return endpoints;
		}

		/// <summary>
		/// Generates OpenID Providers that can authenticate using directed identity.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="opIdentifier">The OP Identifier entered (and resolved) by the user.</param>
		/// <returns>A sequence of the providers that can offer directed identity services.</returns>
		private static IEnumerable<ServiceEndpoint> GenerateOPIdentifierServiceEndpoints(this XrdsDocument xrds, Identifier opIdentifier) {
			return from service in xrds.FindOPIdentifierServices()
				   from uri in service.UriElements
				   let protocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris)
				   let providerDescription = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select ServiceEndpoint.CreateForProviderIdentifier(opIdentifier, providerDescription, service.Priority, uri.Priority);
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
		private static IEnumerable<ServiceEndpoint> GenerateClaimedIdentifierServiceEndpoints(this XrdsDocument xrds, UriIdentifier claimedIdentifier, UriIdentifier userSuppliedIdentifier) {
			return from service in xrds.FindClaimedIdentifierServices()
				   from uri in service.UriElements
				   where uri.Uri != null
				   let providerEndpoint = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier, providerEndpoint, service.Priority, uri.Priority);
		}

		/// <summary>
		/// Generates the OpenID Providers that are capable of asserting ownership
		/// of a particular XRI claimed identifier.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <param name="userSuppliedIdentifier">The i-name supplied by the user.</param>
		/// <returns>A sequence of the providers that can assert ownership of the given identifier.</returns>
		private static IEnumerable<ServiceEndpoint> GenerateClaimedIdentifierServiceEndpoints(this XrdsDocument xrds, XriIdentifier userSuppliedIdentifier) {
			foreach (var service in xrds.FindClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (service.Xrd.CanonicalID == null) {
						Logger.WarnFormat(XrdsStrings.MissingCanonicalIDElement, userSuppliedIdentifier);
						break; // skip on to next service
					}
					ErrorUtilities.VerifyProtocol(service.Xrd.IsCanonicalIdVerified, XrdsStrings.CIDVerificationFailed, userSuppliedIdentifier);

					// In the case of XRI names, the ClaimedId is actually the CanonicalID.
					var claimedIdentifier = new XriIdentifier(service.Xrd.CanonicalID);
					var providerEndpoint = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris);
					yield return ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier, providerEndpoint, service.Priority, uri.Priority);
				}
			}
		}

		/// <summary>
		/// Enumerates the XRDS service elements that describe OpenID Providers offering directed identity assertions.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of service elements.</returns>
		private static IEnumerable<ServiceElement> FindOPIdentifierServices(this XrdsDocument xrds) {
			return from xrd in xrds.XrdElements
				   from service in xrd.OpenIdProviderIdentifierServices
				   select service;
		}

		/// <summary>
		/// Returns the OpenID-compatible services described by a given XRDS document,
		/// in priority order.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of the services offered.</returns>
		private static IEnumerable<ServiceElement> FindClaimedIdentifierServices(this XrdsDocument xrds) {
			return from xrd in xrds.XrdElements
				   from service in xrd.OpenIdClaimedIdentifierServices
				   select service;
		}

		/// <summary>
		/// Enumerates the XRDS service elements that describe OpenID Relying Party return_to URLs
		/// that can receive authentication assertions.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of service elements.</returns>
		private static IEnumerable<ServiceElement> FindReturnToServices(this XrdsDocument xrds) {
			return from xrd in xrds.XrdElements
				   from service in xrd.OpenIdRelyingPartyReturnToServices
				   select service;
		}
	}
}
