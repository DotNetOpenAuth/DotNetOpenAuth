using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("OpenId: {Protocol.Version}")]
	class DirectResponse {
		protected DirectResponse(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, IDictionary<string, string> args) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			RelyingParty = relyingParty;
			Provider = provider;
			Args = args;

			// Make sure that the OP fulfills the required OpenID version.
			// We don't use Provider.Protocol here because that's just a cache of
			// what we _thought_ the OP would support, and our purpose is to double-check this.
			ProtocolVersion detectedProtocol = Protocol.DetectFromDirectResponse(args).ProtocolVersion;
			if (detectedProtocol < relyingParty.Settings.MinimumRequiredOpenIdVersion) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.MinimumOPVersionRequirementNotMet,
					Protocol.Lookup(relyingParty.Settings.MinimumRequiredOpenIdVersion).Version,
					Protocol.Lookup(detectedProtocol).Version));
			}

			if (Logger.IsErrorEnabled) {
				if (!Args.ContainsKey(Protocol.openidnp.ns)) {
					Logger.ErrorFormat("Direct response from provider lacked the {0} key.", Protocol.openid.ns);
				} else if (Args[Protocol.openidnp.ns] != Protocol.QueryDeclaredNamespaceVersion) {
					Logger.ErrorFormat("Direct response from provider for key {0} was '{1}' rather than '{2}'.",
						Protocol.openid.ns, Args[Protocol.openidnp.ns], Protocol.QueryDeclaredNamespaceVersion);
				}
			}
		}
		protected OpenIdRelyingParty RelyingParty { get; private set; }
		protected ServiceEndpoint Provider { get; private set; }
		protected internal IDictionary<string, string> Args { get; private set; }
		protected Protocol Protocol { get { return Provider.Protocol; } }
	}
}
