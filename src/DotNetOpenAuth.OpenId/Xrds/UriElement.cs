//-----------------------------------------------------------------------
// <copyright file="UriElement.cs" company="Outercurve Foundation, Scott Hanselman">
//     Copyright (c) Outercurve Foundation, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System;
	using System.Xml.XPath;

	/// <summary>
	/// The Uri element in an XRDS document.
	/// </summary>
	internal class UriElement : XrdsNode, IComparable<UriElement> {
		/// <summary>
		/// Initializes a new instance of the <see cref="UriElement"/> class.
		/// </summary>
		/// <param name="uriElement">The URI element.</param>
		/// <param name="service">The service.</param>
		public UriElement(XPathNavigator uriElement, ServiceElement service) :
			base(uriElement, service) {
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
		/// Gets the URI.
		/// </summary>
		public Uri Uri {
			get {
				if (Node.Value != null) {
					string value = Node.Value.Trim();
					if (value.Length > 0) {
						return new Uri(value);
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the parent service.
		/// </summary>
		public ServiceElement Service {
			get { return (ServiceElement)ParentNode; }
		}

		#region IComparable<UriElement> Members

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
		public int CompareTo(UriElement other) {
			if (other == null) {
				return -1;
			}
			int compare = this.Service.CompareTo(other.Service);
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
