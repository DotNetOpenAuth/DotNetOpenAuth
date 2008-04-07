using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	class RealmEndpoint {
		public RealmEndpoint(Protocol protocol, Uri relyingPartyEndpoint) {
			Protocol = protocol;
			RelyingPartyEndpoint = relyingPartyEndpoint;
		}
		/// <summary>
		/// The OpenId protocol that the discovered relying party supports.
		/// </summary>
		public Protocol Protocol { get; private set; }
		/// <summary>
		/// The URL to the login page on the discovered relying party web site.
		/// </summary>
		public Uri RelyingPartyEndpoint { get; private set; }
	}
}
