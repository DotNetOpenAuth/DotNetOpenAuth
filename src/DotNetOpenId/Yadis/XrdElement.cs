using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Yadis {
	class XrdElement : XrdsNode {
		public XrdElement(XPathNavigator xrdElement, XrdsDocument parent) :
			base(xrdElement, parent) {
		}

		public IEnumerable<ServiceElement> Services {
			get {
				// We should enumerate them in priority order
				List<ServiceElement> services = new List<ServiceElement>();
				foreach (XPathNavigator node in Node.Select("xrd:Service", XmlNamespaceResolver)) {
					services.Add(new ServiceElement(node, this));
				}
				services.Sort();
				return services;
			}
		}

		public string CanonicalID {
			get {
				var n = Node.SelectSingleNode("xrd:CanonicalID", XmlNamespaceResolver);
				return n != null ? n.Value : null;
			}
		}

		/// <summary>
		/// Returns services for OP Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdProviderIdentifierServices {
			get {
				var xpath = new StringBuilder();
				xpath.Append("xrd:Service[");
				foreach (string uri in ProtocolConstants.OPIdentifierServiceTypeURIs.Values) {
					xpath.Append("xrd:Type/text()='");
					xpath.Append(uri);
					xpath.Append("' or ");
				}
				xpath.Length -= 4;
				xpath.Append("]");
				var services = new List<ServiceElement>();
				foreach (XPathNavigator service in Node.Select(xpath.ToString(), XmlNamespaceResolver)) {
					services.Add(new ServiceElement(service, this));
				}
				// Put the services in their own defined priority order
				services.Sort();
				return services;
			}
		}

		/// <summary>
		/// Returns services for Claimed Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdClaimedIdentifierServices {
			get {
				var xpath = new StringBuilder();
				xpath.Append("xrd:Service[");
				foreach (string uri in ProtocolConstants.ClaimedIdentifierServiceTypeURIs.Values) {
					xpath.Append("xrd:Type/text()='");
					xpath.Append(uri);
					xpath.Append("' or ");
				}
				xpath.Length -= 4;
				xpath.Append("]");
				var services = new List<ServiceElement>();
				foreach (XPathNavigator service in Node.Select(xpath.ToString(), XmlNamespaceResolver)) {
					services.Add(new ServiceElement(service, this));
				}
				// Put the services in their own defined priority order
				services.Sort();
				return services;
			}
		}

		/// <summary>
		/// An enumeration of all Service/URI elements, sorted in priority order.
		/// </summary>
		public IEnumerable<UriElement> ServiceUris {
			get {
				foreach (ServiceElement service in Services) {
					foreach (UriElement uri in service.UriElements) {
						yield return uri;
					}
				}
			}
		}
	}
}
