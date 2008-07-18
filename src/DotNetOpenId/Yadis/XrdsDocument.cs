using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Provider;
using System.Diagnostics;

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

		internal ServiceEndpoint CreateServiceEndpoint(UriIdentifier claimedIdentifier) {
			return createServiceEndpoint(claimedIdentifier);
		}

		internal ServiceEndpoint CreateServiceEndpoint(XriIdentifier userSuppliedIdentifier) {
			return createServiceEndpoint(userSuppliedIdentifier);
		}

		const bool performCIDVerification = true;

		ServiceEndpoint createServiceEndpoint(Identifier userSuppliedOrClaimedIdentifier) {
			// First search for OP Identifier service elements
			foreach (var service in findOPIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					var protocol = Util.FindBestVersion(p => p.OPIdentifierServiceTypeURI, service.TypeElementUris);
					return new ServiceEndpoint(protocol.ClaimedIdentifierForOPIdentifier, uri.Uri, 
						protocol.ClaimedIdentifierForOPIdentifier, service.TypeElementUris);
				}
			}
			// Since we could not find an OP Identifier service element,
			// search for a Claimed Identifier element.
			foreach (var service in findClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (userSuppliedOrClaimedIdentifier is XriIdentifier) {
						if (service.Xrd.CanonicalID == null) {
							Logger.WarnFormat(Strings.MissingCanonicalIDElement, userSuppliedOrClaimedIdentifier);
							return null;
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
							Logger.InfoFormat("Performing XRI CanonicalID verification on user supplied identifier {0}, canonical id {1}.", userSuppliedOrClaimedIdentifier, service.Xrd.CanonicalID);
							Identifier canonicalId = service.Xrd.CanonicalID;
							return canonicalId.Discover();
						} else {
							userSuppliedOrClaimedIdentifier = service.Xrd.CanonicalID;
						}
					}
					return new ServiceEndpoint(userSuppliedOrClaimedIdentifier, uri.Uri, 
						service.ProviderLocalIdentifier, service.TypeElementUris);
				}
			}
			return null;
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
