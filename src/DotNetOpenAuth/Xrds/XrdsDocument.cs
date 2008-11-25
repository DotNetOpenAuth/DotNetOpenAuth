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

	internal class XrdsDocument : XrdsNode {
		public XrdsDocument(XPathNavigator xrdsNavigator)
			: base(xrdsNavigator) {
			XmlNamespaceResolver.AddNamespace("xrd", XrdsNode.XrdNamespace);
			XmlNamespaceResolver.AddNamespace("xrds", XrdsNode.XrdsNamespace);
			XmlNamespaceResolver.AddNamespace("openid10", Protocol.V10.XmlNamespace);
		}

		public XrdsDocument(XmlReader reader)
			: this(new XPathDocument(reader).CreateNavigator()) { }

		public XrdsDocument(string xml)
			: this(new XPathDocument(new StringReader(xml)).CreateNavigator()) { }

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

		internal bool IsXrdResolutionSuccessful {
			get { return this.XrdElements.All(xrd => xrd.IsXriResolutionSuccessful); }
		}

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

		internal IEnumerable<RelyingPartyEndpointDescription> FindRelyingPartyReceivingEndpoints() {
			return from service in this.FindReturnToServices()
				   from uri in service.UriElements
				   select new RelyingPartyEndpointDescription(uri.Uri, service.TypeElementUris);
		}

		private IEnumerable<ServiceEndpoint> GenerateOPIdentifierServiceEndpoints(Identifier opIdentifier) {
			return from service in this.FindOPIdentifierServices()
				   from uri in service.UriElements
				   let protocol = Protocol.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris)
				   select ServiceEndpoint.CreateForProviderIdentifier(opIdentifier, uri.Uri, service.TypeElementUris, service.Priority, uri.Priority);
		}

		private IEnumerable<ServiceEndpoint> GenerateClaimedIdentifierServiceEndpoints(UriIdentifier claimedIdentifier) {
			return from service in this.FindClaimedIdentifierServices()
				   from uri in service.UriElements
				   select ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, service.ProviderLocalIdentifier, uri.Uri, service.TypeElementUris, service.Priority, uri.Priority);
		}

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
					yield return ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier, uri.Uri, service.TypeElementUris, service.Priority, uri.Priority);
				}
			}
		}

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

		private IEnumerable<ServiceElement> FindReturnToServices() {
			return from xrd in this.XrdElements
				   from service in xrd.OpenIdRelyingPartyReturnToServices
				   select service;
		}
	}
}
