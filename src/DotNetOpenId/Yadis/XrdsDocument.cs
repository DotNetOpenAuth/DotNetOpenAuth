using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Yadis {
	class XrdsDocument : XrdsNode {
		public XrdsDocument(XPathNavigator xrdsNavigator)
			: base(xrdsNavigator) {
			XmlNamespaceResolver.AddNamespace("xrd", XrdsNode.XrdNamespace);
			XmlNamespaceResolver.AddNamespace("xrds", XrdsNode.XrdsNamespace);
			XmlNamespaceResolver.AddNamespace("openid10", DotNetOpenId.RelyingParty.ServiceEndpoint.OpenId10Namespace);
		}
		public XrdsDocument(XmlReader reader)
			: this(new XPathDocument(reader).CreateNavigator()) { }
		public XrdsDocument(string xml)
			: this(new XPathDocument(new StringReader(xml)).CreateNavigator()) { }

		public IEnumerable<XrdElement> XrdElements {
			get {
				foreach (XPathNavigator node in Node.Select("/xrds:XRDS/xrd:XRD", XmlNamespaceResolver)) {
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

		ServiceEndpoint createServiceEndpoint(Identifier claimedIdentifier) {
			// First search for OP Identifier service elements
			foreach (var service in findOPIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					return new ServiceEndpoint(ServiceEndpoint.ClaimedIdentifierForOPIdentifier,
						uri.Uri, ServiceEndpoint.ClaimedIdentifierForOPIdentifier,
						service.TypeElementUris);
				}
			}
			// Since we could not find an OP Identifier service element,
			// search for a Claimed Identifier element.
			foreach (var service in findClaimedIdentifierServices()) {
				foreach (var uri in service.UriElements) {
					// spec section 7.3.2.3 on Claimed Id -> CanonicalID substitution
					if (claimedIdentifier is XriIdentifier) {
						if (service.Xrd.CanonicalID == null)
							throw new OpenIdException(Strings.MissingCanonicalIDElement, claimedIdentifier);
						claimedIdentifier = service.Xrd.CanonicalID;
					}
					return new ServiceEndpoint(claimedIdentifier, uri.Uri, 
						service.ProviderLocalIdentifier, service.TypeElementUris);
				}
			}
			return null;
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
	}
}
