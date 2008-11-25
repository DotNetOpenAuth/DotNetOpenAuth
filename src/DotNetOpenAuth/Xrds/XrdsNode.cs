//-----------------------------------------------------------------------
// <copyright file="XrdsNode.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System.Xml;
	using System.Xml.XPath;

	internal class XrdsNode {
		/// <summary>
		/// The XRD namespace xri://$xrd*($v*2.0)
		/// </summary>
		internal const string XrdNamespace = "xri://$xrd*($v*2.0)";

		/// <summary>
		/// The XRDS namespace xri://$xrds
		/// </summary>
		internal const string XrdsNamespace = "xri://$xrds";

		protected XrdsNode(XPathNavigator node, XrdsNode parentNode) {
			this.Node = node;
			this.ParentNode = parentNode;
			this.XmlNamespaceResolver = this.ParentNode.XmlNamespaceResolver;
		}

		protected XrdsNode(XPathNavigator document) {
			this.Node = document;
			this.XmlNamespaceResolver = new XmlNamespaceManager(document.NameTable);
		}

		protected XPathNavigator Node { get; private set; }

		protected XrdsNode ParentNode { get; private set; }

		protected XmlNamespaceManager XmlNamespaceResolver { get; private set; }
	}
}
