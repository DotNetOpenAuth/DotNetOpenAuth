using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace DotNetOpenId.Yadis {
	class ServiceElement : XrdsNode, IComparable<ServiceElement> {
		public ServiceElement(XPathNavigator serviceElement, XrdElement parent) :
			base(serviceElement, parent) {
		}

		public XrdElement Xrd {
			get { return (XrdElement)ParentNode; }
		}

		public int? Priority {
			get {
				XPathNavigator n = Node.SelectSingleNode("@priority", XmlNamespaceResolver);
				return n != null ? n.ValueAsInt : (int?)null;
			}
		}

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

		public IEnumerable<TypeElement> TypeElements {
			get {
				foreach (XPathNavigator node in Node.Select("xrd:Type", XmlNamespaceResolver)) {
					yield return new TypeElement(node, this);
				}
			}
		}

		public string[] TypeElementUris {
			get {
				XPathNodeIterator types = Node.Select("xrd:Type", XmlNamespaceResolver);
				string[] typeUris = new string[types.Count];
				int i = 0;
				foreach (XPathNavigator type in types) {
					typeUris[i++] = type.Value;
				}
				return typeUris;
			}
		}

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

		public int CompareTo(ServiceElement other) {
			if (other == null) return -1;
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
