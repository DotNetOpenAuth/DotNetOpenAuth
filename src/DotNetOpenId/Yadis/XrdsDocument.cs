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
					yield return new XrdElement(node, this);
				}
			}
		}

		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(UriIdentifier claimedIdentifier) {
			return createServiceEndpoints(claimedIdentifier);
		}

		internal IEnumerable<ServiceEndpoint> CreateServiceEndpoints(XriIdentifier userSuppliedIdentifier) {
			return createServiceEndpoints(userSuppliedIdentifier);
		}

		const bool performCIDVerification = true;

		IEnumerable<ServiceEndpoint> createServiceEndpoints(Identifier userSuppliedOrClaimedIdentifier) {
			// First search for OP Identifier service elements
			bool opIdentifierServiceFound = false;
			foreach (var service in findOPIdentifierServices()) {
				opIdentifierServiceFound = true;
				foreach (var uri in service.UriElements) {
					var protocol = Util.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris);
					yield return new ServiceEndpoint(protocol.ClaimedIdentifierForOPIdentifier, uri.Uri, 
						protocol.ClaimedIdentifierForOPIdentifier, service.TypeElementUris, service.Priority, uri.Priority);
				}
			}
			// If any OP Identifier service elements were found, we must not proceed
			// to return any Claimed Identifier services.
			if (opIdentifierServiceFound) yield break;

			// Since we could not find an OP Identifier service element,
			// search for a Claimed Identifier element.
			foreach (var service in findClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (userSuppliedOrClaimedIdentifier is XriIdentifier) {
						if (service.Xrd.CanonicalID == null) {
							if (TraceUtil.Switch.TraceWarning) {
								Trace.TraceWarning(Strings.MissingCanonicalIDElement, userSuppliedOrClaimedIdentifier);
							}
							break; // skip on to next service
						}
						// In the case of XRI names, the ClaimedId is actually the CanonicalID.
						// Per http://dev.inames.net/wiki/XRI_CanonicalID_Verification as of 6/20/08, 
						// we need to perform CanonicalId verification when using xri.net as our proxy resolver
						// to protect ourselves against a security vulnerability.
						// We do this by asking the proxy to resolve again, based on the CanonicalId that we
						// just got from the XRI i-name.  We SHOULD get the same document back, but in case
						// of the attack it would be a different document, and the second document would be
						// the reliable one.
						if (performCIDVerification && userSuppliedOrClaimedIdentifier != service.Xrd.CanonicalID) {
							if (TraceUtil.Switch.TraceInfo) {
								Trace.TraceInformation("Performing XRI CanonicalID verification on user supplied identifier {0}, canonical id {1}.", userSuppliedOrClaimedIdentifier, service.Xrd.CanonicalID);
							}
							Identifier canonicalId = service.Xrd.CanonicalID;
							foreach (var endpoint in canonicalId.Discover()) {
								yield return endpoint;
							}
							yield break;
						} else {
							userSuppliedOrClaimedIdentifier = service.Xrd.CanonicalID;
						}
					}
					yield return new ServiceEndpoint(userSuppliedOrClaimedIdentifier, uri.Uri, 
						service.ProviderLocalIdentifier, service.TypeElementUris, service.Priority, uri.Priority);
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
				foreach( var service in xrd.OpenIdRelyingPartyReturnToServices) {
					yield return service;
				}
			}
		}
	}
}
