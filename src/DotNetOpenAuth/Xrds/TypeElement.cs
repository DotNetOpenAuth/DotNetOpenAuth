//-----------------------------------------------------------------------
// <copyright file="TypeElement.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System.Xml.XPath;

	internal class TypeElement : XrdsNode {
		public TypeElement(XPathNavigator typeElement, ServiceElement parent) :
			base(typeElement, parent) {
		}

		public string Uri {
			get { return Node.Value; }
		}
	}
}
