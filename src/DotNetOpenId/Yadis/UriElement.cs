using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace DotNetOpenId.Yadis {
	class UriElement : XrdsNode, IComparable<UriElement> {
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

		public ServiceElement Service {
			get { return (ServiceElement)ParentNode; }
		}

		#region IComparable<UriElement> Members

		public int CompareTo(UriElement other) {
			if (other == null) return -1;
			int compare = Service.CompareTo(other.Service);
			if (compare != 0) return compare;
	
			if (Priority.HasValue && other.Priority.HasValue) {
				return Priority.Value.CompareTo(other.Priority.Value);
			} else {
				if (Priority.HasValue) {
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
