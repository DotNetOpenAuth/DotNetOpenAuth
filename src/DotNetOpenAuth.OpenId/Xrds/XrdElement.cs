//-----------------------------------------------------------------------
// <copyright file="XrdElement.cs" company="Outercurve Foundation, Scott Hanselman">
//     Copyright (c) Outercurve Foundation, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Xrds {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// The Xrd element in an XRDS document.
	/// </summary>
	internal class XrdElement : XrdsNode {
		/// <summary>
		/// Initializes a new instance of the <see cref="XrdElement"/> class.
		/// </summary>
		/// <param name="xrdElement">The XRD element.</param>
		/// <param name="parent">The parent.</param>
		public XrdElement(XPathNavigator xrdElement, XrdsDocument parent) :
			base(xrdElement, parent) {
		}

		/// <summary>
		/// Gets the child service elements.
		/// </summary>
		/// <value>The services.</value>
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

		/// <summary>
		/// Gets a value indicating whether this XRD element's resolution at the XRI resolver was successful.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this XRD's resolution was successful; otherwise, <c>false</c>.
		/// </value>
		public bool IsXriResolutionSuccessful {
			get {
				return this.XriResolutionStatusCode == 100;
			}
		}

		/// <summary>
		/// Gets the canonical ID (i-number) for this element.
		/// </summary>
		public string CanonicalID {
			get {
				var n = Node.SelectSingleNode("xrd:CanonicalID", XmlNamespaceResolver);
				return n != null ? n.Value : null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="CanonicalID"/> was verified.
		/// </summary>
		public bool IsCanonicalIdVerified {
			get {
				var n = Node.SelectSingleNode("xrd:Status", XmlNamespaceResolver);
				return n != null && string.Equals(n.GetAttribute("cid", string.Empty), "verified", StringComparison.Ordinal);
			}
		}

		/// <summary>
		/// Gets the services for OP Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdProviderIdentifierServices {
			get { return this.SearchForServiceTypeUris(p => p.OPIdentifierServiceTypeURI); }
		}

		/// <summary>
		/// Gets the services for Claimed Identifiers.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdClaimedIdentifierServices {
			get { return this.SearchForServiceTypeUris(p => p.ClaimedIdentifierServiceTypeURI); }
		}

		/// <summary>
		/// Gets the services that would be discoverable at an RP for return_to verification.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdRelyingPartyReturnToServices {
			get { return this.SearchForServiceTypeUris(p => p.RPReturnToTypeURI); }
		}

		/// <summary>
		/// Gets the services that would be discoverable at an RP for the UI extension icon.
		/// </summary>
		public IEnumerable<ServiceElement> OpenIdRelyingPartyIcons {
			get { return this.SearchForServiceTypeUris(p => "http://specs.openid.net/extensions/ui/icon"); }
		}

		/// <summary>
		/// Gets an enumeration of all Service/URI elements, sorted in priority order.
		/// </summary>
		public IEnumerable<UriElement> ServiceUris {
			get {
				return from service in this.Services
					   from uri in service.UriElements
					   select uri;
			}
		}

		/// <summary>
		/// Gets the XRI resolution status code.
		/// </summary>
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
		/// Searches for service sub-elements that have Type URI sub-elements that match
		/// one that we have for a known OpenID protocol version.
		/// </summary>
		/// <param name="p">A function that selects what element of the OpenID Protocol we're interested in finding.</param>
		/// <returns>A sequence of service elements that match the search criteria, sorted in XRDS @priority attribute order.</returns>
		internal IEnumerable<ServiceElement> SearchForServiceTypeUris(Func<Protocol, string> p) {
			var xpath = new StringBuilder();
			xpath.Append("xrd:Service[");
			foreach (var protocol in Protocol.AllVersions) {
				string typeUri = p(protocol);
				if (typeUri == null) {
					continue;
				}
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
