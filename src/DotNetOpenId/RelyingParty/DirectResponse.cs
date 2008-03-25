using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class DirectResponse {
		protected DirectResponse(ServiceEndpoint provider, IDictionary<string, string> args) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			Provider = provider;
			Args = args;

			if (TraceUtil.Switch.TraceInfo) {
				if (!Args.ContainsKey(Protocol.openidnp.ns)) {
					Trace.TraceInformation("Direct response from provider lacked the {0} key.", Protocol.openid.ns);
				} else if (Args[Protocol.openidnp.ns] != Protocol.QueryDeclaredNamespaceVersion) {
					Trace.TraceInformation("Direct response from provider for key {0} was '{1}' rather than '{2}'.",
						Protocol.openid.ns, Args[Protocol.openidnp.ns], Protocol.QueryDeclaredNamespaceVersion);
				}
			}

		}
		protected ServiceEndpoint Provider { get; private set; }
		protected IDictionary<string, string> Args { get; private set; }
		protected Protocol Protocol { get { return Provider.Protocol; } }
	}
}
