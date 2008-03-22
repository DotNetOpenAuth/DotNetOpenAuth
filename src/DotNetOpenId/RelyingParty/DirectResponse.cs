using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class DirectResponse {
		protected DirectResponse(Uri provider, IDictionary<string, string> args) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			Provider = provider;
			Args = args;

			if (TraceUtil.Switch.TraceInfo) {
				if (!Args.ContainsKey(QueryStringArgs.openidnp.ns)) {
					Trace.TraceInformation("Direct response from provider lacked the {0} key.", QueryStringArgs.openid.ns);
				} else if (Args[QueryStringArgs.openidnp.ns] != ProtocolConstants.OpenIdNs.v20) {
					Trace.TraceInformation("Direct response from provider for key {0} was '{1}' rather than '{2}'.",
						QueryStringArgs.openid.ns, Args[QueryStringArgs.openidnp.ns], ProtocolConstants.OpenIdNs.v20);
				}
			}

		}
		protected Uri Provider { get; private set; }
		protected IDictionary<string, string> Args { get; private set; }
	}
}
