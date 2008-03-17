using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

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

		public IEnumerable<ServiceElement> OpenIdServices {
			get {
				var xpath = new StringBuilder();
				xpath.Append("xrd:Service[");
				foreach (string uri in DotNetOpenId.RelyingParty.ServiceEndpoint.OpenIdTypeUris) {
					xpath.Append("xrd:Type/text()='");
					xpath.Append(uri);
					xpath.Append("' or ");
				}
				xpath.Length -= 4;
				xpath.Append("]");
				var services = new List<ServiceElement>();
				foreach (XPathNavigator service in Node.Select(xpath.ToString())) {
					services.Add(new ServiceElement(service, this));
				}
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
