//-----------------------------------------------------------------------
// <copyright file="UriElement.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System;
	using System.Xml.XPath;

	internal class UriElement : XrdsNode, IComparable<UriElement> {
		public UriElement(XPathNavigator uriElement, ServiceElement service) :
			base(uriElement, service) {
		}

		public int? Priority {
			get {
				XPathNavigator n = Node.SelectSingleNode("@priority", XmlNamespaceResolver);
				return n != null ? n.ValueAsInt : (int?)null;
			}
		}

		public Uri Uri {
			get { return new Uri(Node.Value); }
		}

		public ServiceElement Service {
			get { return (ServiceElement)ParentNode; }
		}

		#region IComparable<UriElement> Members

		public int CompareTo(UriElement other) {
			if (other == null) {
				return -1;
			}
			int compare = Service.CompareTo(other.Service);
			if (compare != 0) {
				return compare;
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
