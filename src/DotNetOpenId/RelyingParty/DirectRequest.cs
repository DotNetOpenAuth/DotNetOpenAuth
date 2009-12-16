using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("OpenId: {Protocol.Version}")]
	abstract class DirectRequest {
		protected DirectRequest(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, IDictionary<string, string> args) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			RelyingParty = relyingParty;
			Provider = provider;
			Args = args;
			if (Protocol.QueryDeclaredNamespaceVersion != null &&
				!Args.ContainsKey(Protocol.openid.ns))
				Args.Add(Protocol.openid.ns, Protocol.QueryDeclaredNamespaceVersion);
		}
		protected ServiceEndpoint Provider { get; private set; }
		protected Protocol Protocol { get { return Provider.Protocol; } }
		protected internal IDictionary<string, string> Args { get; private set; }
		protected OpenIdRelyingParty RelyingParty { get; private set; }

		protected IDictionary<string, string> GetResponse() {
			Logger.DebugFormat("Sending direct message to {0}: {1}{2}", Provider.ProviderEndpoint,
				Environment.NewLine, Util.ToString(Args));
			return RelyingParty.DirectMessageChannel.SendDirectMessageAndGetResponse(Provider, Args);
		}
	}
}
