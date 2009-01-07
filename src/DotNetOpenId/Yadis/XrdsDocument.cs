using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Yadis {
	class XrdsDocument : XrdsNode {
		public XrdsDocument(XPathNavigator xrdsNavigator)
			: base(xrdsNavigator) {
			XmlNamespaceResolver.AddNamespace("xrd", XrdsNode.XrdNamespace);
			XmlNamespaceResolver.AddNamespace("xrds", XrdsNode.XrdsNamespace);
			XmlNamespaceResolver.AddNamespace("openid10", Protocol.v10.XmlNamespace);
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
					if (node != null) {
						yield return new XrdElement(node, this);
					}
				}
			}
		}

		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(UriIdentifier claimedIdentifier) {
			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(generateOPIdentifierServiceEndpoints(claimedIdentifier));
			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(generateClaimedIdentifierServiceEndpoints(claimedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(Util.ToString(endpoints, true));
			return endpoints;
		}

		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(XriIdentifier userSuppliedIdentifier) {
			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();
			endpoints.AddRange(generateOPIdentifierServiceEndpoints(userSuppliedIdentifier));
			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (endpoints.Count == 0) {
				endpoints.AddRange(generateClaimedIdentifierServiceEndpoints(userSuppliedIdentifier));
			}
			Logger.DebugFormat("Total services discovered in XRDS: {0}", endpoints.Count);
			Logger.Debug(Util.ToString(endpoints, true));
			return endpoints;
		}

		IEnumerable<ServiceEndpoint> generateOPIdentifierServiceEndpoints(Identifier opIdentifier) {
			foreach (var service in findOPIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					var protocol = Util.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris);
					yield return ServiceEndpoint.CreateForProviderIdentifier(
						opIdentifier, uri.Uri, service.TypeElementUris,
						service.Priority, uri.Priority);
				}
			}
		}

		IEnumerable<ServiceEndpoint> generateClaimedIdentifierServiceEndpoints(UriIdentifier claimedIdentifier) {
			foreach (var service in findClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					yield return ServiceEndpoint.CreateForClaimedIdentifier(
						claimedIdentifier, service.ProviderLocalIdentifier,
						uri.Uri, service.TypeElementUris, service.Priority, uri.Priority);
				}
			}
		}

		IEnumerable<ServiceEndpoint> generateClaimedIdentifierServiceEndpoints(XriIdentifier userSuppliedIdentifier) {
			foreach (var service in findClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (service.Xrd.CanonicalID == null) {
						Logger.WarnFormat(Strings.MissingCanonicalIDElement, userSuppliedIdentifier);
						break; // skip on to next service
					}
					if (!service.Xrd.IsCanonicalIdVerified) {
						throw new OpenIdException(Strings.CIDVerificationFailed, userSuppliedIdentifier);
					}
					// In the case of XRI names, the ClaimedId is actually the CanonicalID.
					var claimedIdentifier = new XriIdentifier(service.Xrd.CanonicalID);
					yield return ServiceEndpoint.CreateForClaimedIdentifier(
						claimedIdentifier, userSuppliedIdentifier, service.ProviderLocalIdentifier,
						uri.Uri, service.TypeElementUris, service.Priority, uri.Priority);
				}
			}
		}

		internal IEnumerable<RelyingPartyReceivingEndpoint> FindRelyingPartyReceivingEndpoints() {
			foreach (var service in findReturnToServices()) {
				foreach (var uri in service.UriElements) {
					yield return new RelyingPartyReceivingEndpoint(uri.Uri, service.TypeElementUris);
				}
			}
		}

		IEnumerable<ServiceElement> findOPIdentifierServices() {
			foreach (var xrd in XrdElements) {
				foreach (var service in xrd.OpenIdProviderIdentifierServices) {
					yield return service;
				}
			}
		}

		/// <summary>
		/// Returns the OpenID-compatible services described by a given XRDS document,
		/// in priority order.
		/// </summary>
		IEnumerable<ServiceElement> findClaimedIdentifierServices() {
			foreach (var xrd in XrdElements) {
				foreach (var service in xrd.OpenIdClaimedIdentifierServices) {
					yield return service;
				}
			}
		}

		IEnumerable<ServiceElement> findReturnToServices() {
			foreach (var xrd in XrdElements) {
				foreach (var service in xrd.OpenIdRelyingPartyReturnToServices) {
					yield return service;
				}
			}
		}

		internal bool IsXrdResolutionSuccessful {
			get {
				foreach (var xrd in XrdElements) {
					if (!xrd.IsXriResolutionSuccessful) {
						return false;
					}
				}
				return true;
			}
		}
	}
}
