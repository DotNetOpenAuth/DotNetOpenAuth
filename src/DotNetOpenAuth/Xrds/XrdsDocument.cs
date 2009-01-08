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
					if (node != null) {
						yield return new XrdElement(node, this);
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether all child XRD elements were resolved successfully.
		/// </summary>
		internal bool IsXrdResolutionSuccessful {
			get { return this.XrdElements.All(xrd => xrd.IsXriResolutionSuccessful); }
		}
	}
}
