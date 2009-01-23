using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace DotNetOpenId.Yadis {
	class XrdsNode {
		internal const string XrdNamespace = "xri://$xrd*($v*2.0)";
		internal const string XrdsNamespace = "xri://$xrds";

		protected XrdsNode(XPathNavigator node, XrdsNode parentNode) {
			if (node == null) {
				throw new ArgumentNullException("node");
			}
			if (parentNode == null) {
				throw new ArgumentNullException("parentNode");
			}

			Node = node;
			ParentNode = parentNode;
			XmlNamespaceResolver = ParentNode.XmlNamespaceResolver;
		}
		protected XrdsNode(XPathNavigator document) {
			if (document == null) {
				throw new ArgumentNullException("document");
			}

			Node = document;
			XmlNamespaceResolver = new XmlNamespaceManager(document.NameTable);
		}

		protected XPathNavigator Node { get; private set; }
		protected XrdsNode ParentNode { get; private set; }
		protected XmlNamespaceManager XmlNamespaceResolver { get; private set; }
	}
}
