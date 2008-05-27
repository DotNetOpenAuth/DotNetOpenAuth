using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	[DebuggerDisplay("RelyingPartyReceivingEndpoint: {RelyingPartyEndpoint}")]
	class RelyingPartyReceivingEndpoint {
		public RelyingPartyReceivingEndpoint(Uri relyingPartyEndpoint, string[] supportedServiceTypeUris) {
			RelyingPartyEndpoint = relyingPartyEndpoint;
			this.supportedServiceTypeUris = supportedServiceTypeUris;
		}
		/// <summary>
		/// The URL to the login page on the discovered relying party web site.
		/// </summary>
		public Uri RelyingPartyEndpoint { get; private set; }
		/// <summary>
		/// The Type URIs of supported services advertised on a relying party's XRDS document.
		/// </summary>
		string[] supportedServiceTypeUris;
		Protocol protocol;
		/// <summary>
		/// The OpenId protocol that the discovered relying party supports.
		/// </summary>
		public Protocol Protocol {
			get {
				if (protocol == null) {
					protocol = Util.FindBestVersion(p => p.RPReturnToTypeURI, supportedServiceTypeUris);
				}
				if (protocol != null) return protocol;
				throw new InvalidOperationException("Unable to determine the version of OpenID the Relying Party supports.");
			}
		}
	}
}
