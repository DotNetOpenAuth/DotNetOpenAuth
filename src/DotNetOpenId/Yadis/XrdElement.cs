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
					services.Add(new ServiceElement(node.Clone(), this));
				}
				services.Sort();
				return services;
			}
		}


		int XriResolutionStatusCode {
			get {
				var n = Node.SelectSingleNode("xrd:Status", XmlNamespaceResolver);
				string codeString;
				if (n == null || string.IsNullOrEmpty(codeString = n.GetAttribute("code", ""))) {
					throw new OpenIdException(Strings.XriResolutionStatusMissing);
				}
				int code;
				if (!int.TryParse(codeString, out code) || code < 100 || code > 399) {
					throw new OpenIdException(Strings.XriResolutionStatusMissing);
				}
				return code;
			}
		}

		public bool IsXriResolutionSuccessful {
			get {
				return XriResolutionStatusCode == 100;
			}
		}

		public string CanonicalID {
			get {
				var n = Node.SelectSingleNode("xrd:CanonicalID", XmlNamespaceResolver);
				return n != null ? n.Value : null;
			}
		}

		public bool IsCanonicalIdVerified {
			get {
				var n = Node.SelectSingleNode("xrd:Status", XmlNamespaceResolver);
				return n != null && string.Equals(n.GetAttribute("cid", ""), "verified", StringComparison.Ordinal);
			}
		}

		IEnumerable<ServiceElement> searchForServiceTypeUris(Util.Func<Protocol, string> p) {
			var xpath = new StringBuilder();
			xpath.Append("xrd:Service[");
			foreach (var protocol in Protocol.AllVersions) {
				string typeUri = p(protocol);
				if (typeUri == null) continue;
				xpath.Append("xrd:Type/text()='");
				xpath.Append(typeUri);
				xpath.Append("' or ");
			}
			xpath.Length -= 4;
			xpath.Append("]");
			var services = new List<ServiceElement>();
			foreach (XPathNavigator service in Node.Select(xpath.ToString(), XmlNamespaceResolver)) {
				services.Add(new ServiceElement(service.Clone(), this));
			}
			// Put the services in their own defined priority order
			services.Sort();
			return services;
		}

		/// <summary>
		/// Returns services for OP Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdProviderIdentifierServices {
			get { return searchForServiceTypeUris(p => p.OPIdentifierServiceTypeURI); }
		}

		/// <summary>
		/// Returns services for Claimed Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdClaimedIdentifierServices {
			get { return searchForServiceTypeUris(p => p.ClaimedIdentifierServiceTypeURI); }
		}

		public IEnumerable<ServiceElement> OpenIdRelyingPartyReturnToServices {
			get { return searchForServiceTypeUris(p => p.RPReturnToTypeURI); }
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
