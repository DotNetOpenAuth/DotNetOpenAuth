using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

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
	}
}
