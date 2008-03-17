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

		internal ServiceEndpoint CreateServiceEndpoint(Identifier claimedIdentifier) {
			// Return the first service and URI to match OpenID requirements
			// as supported by this library.
			foreach (var service in findCompatibleServices()) {
				foreach (var uri in service.UriElements) {
					return new ServiceEndpoint(claimedIdentifier, uri.Uri, service.ProviderLocalIdentifier);
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the OpenID-compatible services described by a given XRDS document,
		/// in priority order.
		/// </summary>
		IEnumerable<ServiceElement> findCompatibleServices() {
			foreach (var xrd in XrdElements) {
				foreach (var service in xrd.OpenIdServices) {
					yield return service;
				}
			}
		}
	}
}
