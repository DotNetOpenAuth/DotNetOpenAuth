//-----------------------------------------------------------------------
// <copyright file="ServiceElement.cs" company="Outercurve Foundation, Scott Hanselman">
//     Copyright (c) Outercurve Foundation, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.XPath;
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// The Service element in an XRDS document.
	/// </summary>
	internal class ServiceElement : XrdsNode, IComparable<ServiceElement> {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceElement"/> class.
		/// </summary>
		/// <param name="serviceElement">The service element.</param>
		/// <param name="parent">The parent.</param>
		public ServiceElement(XPathNavigator serviceElement, XrdElement parent) :
			base(serviceElement, parent) {
		}

		/// <summary>
		/// Gets the XRD parent element.
		/// </summary>
		public XrdElement Xrd {
			get { return (XrdElement)ParentNode; }
		}

		/// <summary>
		/// Gets the priority.
		/// </summary>
		public int? Priority {
			get {
				XPathNavigator n = Node.SelectSingleNode("@priority", XmlNamespaceResolver);
				return n != null ? n.ValueAsInt : (int?)null;
			}
		}

		/// <summary>
		/// Gets the URI child elements.
		/// </summary>
		public IEnumerable<UriElement> UriElements {
			get {
				List<UriElement> uris = new List<UriElement>();
				foreach (XPathNavigator node in Node.Select("xrd:URI", XmlNamespaceResolver)) {
					uris.Add(new UriElement(node, this));
				}
				uris.Sort();
				return uris;
			}
		}

		/// <summary>
		/// Gets the type child elements.
		/// </summary>
		/// <value>The type elements.</value>
		public IEnumerable<TypeElement> TypeElements {
			get {
				foreach (XPathNavigator node in Node.Select("xrd:Type", XmlNamespaceResolver)) {
					yield return new TypeElement(node, this);
				}
			}
		}

		/// <summary>
		/// Gets the type child element's URIs.
		/// </summary>
		public string[] TypeElementUris {
			get {
				return this.TypeElements.Select(type => type.Uri).ToArray();
			}
		}

		/// <summary>
		/// Gets the OP Local Identifier.
		/// </summary>
		public Identifier ProviderLocalIdentifier {
			get {
				var n = Node.SelectSingleNode("xrd:LocalID", XmlNamespaceResolver)
					?? Node.SelectSingleNode("openid10:Delegate", XmlNamespaceResolver);
				if (n != null && n.Value != null) {
					string value = n.Value.Trim();
					if (value.Length > 0) {
						return n.Value;
					}
				}

				return null;
			}
		}

		#region IComparable<ServiceElement> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
		/// Value
		/// Meaning
		/// Less than zero
		/// This object is less than the <paramref name="other"/> parameter.
		/// Zero
		/// This object is equal to <paramref name="other"/>.
		/// Greater than zero
		/// This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(ServiceElement other) {
			if (other == null) {
				return -1;
			}
			if (this.Priority.HasValue && other.Priority.HasValue) {
				return this.Priority.Value.CompareTo(other.Priority.Value);
			} else {
				if (this.Priority.HasValue) {
					return -1;
				} else if (other.Priority.HasValue) {
					return 1;
				} else {
					return 0;
				}
			}
		}

		#endregion
	}
}
