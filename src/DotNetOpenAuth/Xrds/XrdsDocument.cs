//-----------------------------------------------------------------------
// <copyright file="XrdsDocument.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An XRDS document.
	/// </summary>
	internal class XrdsDocument : XrdsNode {
		/// <summary>
		/// Initializes a new instance of the <see cref="XrdsDocument"/> class.
		/// </summary>
		/// <param name="xrdsNavigator">The root node of the XRDS document.</param>
		public XrdsDocument(XPathNavigator xrdsNavigator)
			: base(xrdsNavigator) {
			XmlNamespaceResolver.AddNamespace("xrd", XrdsNode.XrdNamespace);
			XmlNamespaceResolver.AddNamespace("xrds", XrdsNode.XrdsNamespace);
			XmlNamespaceResolver.AddNamespace("openid10", Protocol.V10.XmlNamespace);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XrdsDocument"/> class.
		/// </summary>
		/// <param name="reader">The Xml reader positioned at the root node of the XRDS document.</param>
		public XrdsDocument(XmlReader reader)
			: this(new XPathDocument(reader).CreateNavigator()) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XrdsDocument"/> class.
		/// </summary>
		/// <param name="xml">The text that is the XRDS document.</param>
		public XrdsDocument(string xml)
			: this(new XPathDocument(new StringReader(xml)).CreateNavigator()) { }

		/// <summary>
		/// Gets the XRD child elements of the document.
		/// </summary>
		public IEnumerable<XrdElement> XrdElements {
			get {
				// We may be looking at a full XRDS document (in the case of YADIS discovery)
				// or we may be looking at just an individual XRD element from a larger document
				// if we asked xri.net for just one.
				if (Node.SelectSingleNode("/xrds:XRDS", XmlNamespaceResolver) != null) {
					foreach (XPathNavigator node in Node.Select("/xrds:XRDS/xrd:XRD", XmlNamespaceResolver)) {
						yield return new XrdElement(node, this);
					}
				} else {
					XPathNavigator node = Node.SelectSingleNode("/xrd:XRD", XmlNamespaceResolver);
					yield return new XrdElement(node, this);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether all child XRD elements were resolved successfully.
		/// </summary>
		internal bool IsXrdResolutionSuccessful {
			get { return this.XrdElements.All(xrd => xrd.IsXriResolutionSuccessful); }
		}

		/// <summary>
		/// Creates the service endpoints described in this document, useful for requesting
		/// authentication of one of the OpenID Providers that result from it.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier that was used to discover this XRDS document.</param>
		/// <returns>A sequence of OpenID Providers that can assert ownership of the <paramref name="claimedIdentifier"/>.</returns>
		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(UriIdentifier claimedIdentifier) {
			var endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(this.GenerateOPIdentifierServiceEndpoints(claimedIdentifier));

			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(this.GenerateClaimedIdentifierServiceEndpoints(claimedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(endpoints.ToStringDeferred(true));
			return endpoints;
		}

		/// <summary>
		/// Creates the service endpoints described in this document, useful for requesting
		/// authentication of one of the OpenID Providers that result from it.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user-supplied i-name that was used to discover this XRDS document.</param>
		/// <returns>A sequence of OpenID Providers that can assert ownership of the canonical ID given in this document.</returns>
		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(XriIdentifier userSuppliedIdentifier) {
			var endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(this.GenerateOPIdentifierServiceEndpoints(userSuppliedIdentifier));

			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(this.GenerateClaimedIdentifierServiceEndpoints(userSuppliedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(endpoints.ToStringDeferred(true));
			return endpoints;
		}

		/// <summary>
		/// Finds the Relying Party return_to receiving endpoints.
		/// </summary>
		/// <returns>A sequence of Relying Party descriptors for the return_to endpoints.</returns>
		/// <remarks>
		/// This is useful for Providers to send unsolicited assertions to Relying Parties,
		/// or for Provider's to perform RP discovery/verification as part of authentication.
		/// </remarks>
		internal IEnumerable<RelyingPartyEndpointDescription> FindRelyingPartyReceivingEndpoints() {
			return from service in this.FindReturnToServices()
				   from uri in service.UriElements
				   select new RelyingPartyEndpointDescription(uri.Uri, service.TypeElementUris);
		}

		/// <summary>
		/// Generates OpenID Providers that can authenticate using directed identity.
		/// </summary>
		/// <param name="opIdentifier">The OP Identifier entered (and resolved) by the user.</param>
		/// <returns>A sequence of the providers that can offer directed identity services.</returns>
		private IEnumerable<ServiceEndpoint> GenerateOPIdentifierServiceEndpoints(Identifier opIdentifier) {
			return from service in this.FindOPIdentifierServices()
				   from uri in service.UriElements
				   let protocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris)
				   let providerDescription = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select ServiceEndpoint.CreateForProviderIdentifier(opIdentifier, providerDescription, service.Priority, uri.Priority);
		}

		/// <summary>
		/// Generates the OpenID Providers that are capable of asserting ownership
		/// of a particular URI claimed identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <returns>A sequence of the providers that can assert ownership of the given identifier.</returns>
		private IEnumerable<ServiceEndpoint> GenerateClaimedIdentifierServiceEndpoints(UriIdentifier claimedIdentifier) {
			return from service in this.FindClaimedIdentifierServices()
				   from uri in service.UriElements
				   let providerEndpoint = new ProviderEndpointDescription(uri.Uri, service.TypeElementUris)
				   select ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, service.ProviderLocalIdentifier, providerEndpoint, service.Priority, uri.Priority);
		}

		/// <summary>
		/// Generates the OpenID Providers that are capable of asserting ownership
		/// of a particular XRI claimed identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The i-name supplied by the user.</param>
		/// <returns>A sequence of the providers that can assert ownership of the given identifier.</returns>
		private IEnumerable<ServiceEndpoint> GenerateClaimedIdentifierServiceEndpoints(XriIdentifier userSuppliedIdentifier) {
			foreach (var service in this.FindClaimedIdentifierServices()) {
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
		/// <returns>A sequence of service elements.</returns>
		private IEnumerable<ServiceElement> FindOPIdentifierServices() {
			return from xrd in this.XrdElements
				   from service in xrd.OpenIdProviderIdentifierServices
				   select service;
		}

		/// <summary>
		/// Returns the OpenID-compatible services described by a given XRDS document,
		/// in priority order.
		/// </summary>
		/// <returns>A sequence of the services offered.</returns>
		private IEnumerable<ServiceElement> FindClaimedIdentifierServices() {
			return from xrd in this.XrdElements
				   from service in xrd.OpenIdClaimedIdentifierServices
				   select service;
		}

		/// <summary>
		/// Enumerates the XRDS service elements that describe OpenID Relying Party return_to URLs
		/// that can receive authentication assertions.
		/// </summary>
		/// <returns>A sequence of service elements.</returns>
		private IEnumerable<ServiceElement> FindReturnToServices() {
			return from xrd in this.XrdElements
				   from service in xrd.OpenIdRelyingPartyReturnToServices
				   select service;
		}
	}
}
