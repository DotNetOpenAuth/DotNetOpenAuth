using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace DotNetOpenId.Yadis {
	class UriElement : XrdsNode, IComparable<UriElement> {
		public UriElement(XPathNavigator uriElement, ServiceElement service) :
			base(uriElement, service) {
		}

		public int Priority {
			get { return Node.SelectSingleNode("@priority", XmlNamespaceResolver).ValueAsInt; }
		}

		public Uri Uri {
			get { return new Uri(Node.Value); }
		}

		public ServiceElement Service {
			get { return (ServiceElement)ParentNode; }
		}

		#region IComparable<UriElement> Members

		public int CompareTo(UriElement other) {
			int compare = Service.CompareTo(other.Service);
			return compare != 0 ? compare : Priority.CompareTo(other.Priority);
		}

		#endregion
	}
}
