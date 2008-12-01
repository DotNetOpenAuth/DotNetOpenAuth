//-----------------------------------------------------------------------
// <copyright file="TypeElement.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System.Xml.XPath;

	/// <summary>
	/// The Type element in an XRDS document.
	/// </summary>
	internal class TypeElement : XrdsNode {
		/// <summary>
		/// Initializes a new instance of the <see cref="TypeElement"/> class.
		/// </summary>
		/// <param name="typeElement">The type element.</param>
		/// <param name="parent">The parent.</param>
		public TypeElement(XPathNavigator typeElement, ServiceElement parent) :
			base(typeElement, parent) {
		}

		/// <summary>
		/// Gets the URI.
		/// </summary>
		public string Uri {
			get { return Node.Value; }
		}
	}
}
