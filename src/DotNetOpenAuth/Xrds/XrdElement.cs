//-----------------------------------------------------------------------
// <copyright file="XrdElement.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml.XPath;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Messaging;

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
				return n != null && string.Equals(n.GetAttribute("cid", string.Empty), "verified", StringComparison.Ordinal);
			}
		}

		private int XriResolutionStatusCode {
			get {
				var n = Node.SelectSingleNode("xrd:Status", XmlNamespaceResolver);
				string codeString = null;
				ErrorUtilities.VerifyProtocol(n != null && !string.IsNullOrEmpty(codeString = n.GetAttribute("code", string.Empty)), XrdsStrings.XriResolutionStatusMissing);
				int code;
				ErrorUtilities.VerifyProtocol(int.TryParse(codeString, out code) && code >= 100 && code < 400, XrdsStrings.XriResolutionStatusMissing);
				return code;
			}
		}

		/// <summary>
		/// Returns services for OP Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdProviderIdentifierServices {
			get { return SearchForServiceTypeUris(p => p.OPIdentifierServiceTypeURI); }
		}

		/// <summary>
		/// Returns services for Claimed Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdClaimedIdentifierServices {
			get { return SearchForServiceTypeUris(p => p.ClaimedIdentifierServiceTypeURI); }
		}

		public IEnumerable<ServiceElement> OpenIdRelyingPartyReturnToServices {
			get { return SearchForServiceTypeUris(p => p.RPReturnToTypeURI); }
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

		private IEnumerable<ServiceElement> SearchForServiceTypeUris(Func<Protocol, string> p) {
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
				services.Add(new ServiceElement(service, this));
			}
			// Put the services in their own defined priority order
			services.Sort();
			return services;
		}
	}
}
