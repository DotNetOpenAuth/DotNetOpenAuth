using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("OpenId: {Protocol.Version}")]
	abstract class DirectRequest {
		protected DirectRequest(ServiceEndpoint provider, IDictionary<string, string> args) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			Provider = provider;
			Args = args;
			if (Protocol.QueryDeclaredNamespaceVersion != null &&
				!Args.ContainsKey(Protocol.openid.ns))
				Args.Add(Protocol.openid.ns, Protocol.QueryDeclaredNamespaceVersion);
		}
		protected ServiceEndpoint Provider { get; private set; }
		protected Protocol Protocol { get { return Provider.Protocol; } }
		protected IDictionary<string, string> Args { get; private set; }

		protected IDictionary<string, string> GetResponse() {
			byte[] body = ProtocolMessages.Http.GetBytes(Args);
			UntrustedWebResponse resp = null;
			IDictionary<string, string> args = null;
			try {
				resp = UntrustedWebRequest.Request(Provider.ProviderEndpoint, body);
				args = ProtocolMessages.KeyValueForm.GetDictionary(resp.ResponseStream);
			} catch (ArgumentException e) {
				throw new OpenIdException("Failure decoding Key-Value Form response from provider.", e);
			} catch (WebException e) {
				throw new OpenIdException("Failure while connecting to provider.", e);
			}
			// All error codes are supposed to be returned with 400, but
			// some (like myopenid.com) sometimes send errors as 200's.
			if (resp.StatusCode == HttpStatusCode.BadRequest ||
				Util.GetOptionalArg(args, Protocol.openidnp.mode) == Protocol.Args.Mode.error) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ProviderRespondedWithError, 
					Util.GetOptionalArg(args, Protocol.openidnp.error)), args);
			} else if (resp.StatusCode == HttpStatusCode.OK) {
				return args;
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ProviderRespondedWithUnrecognizedHTTPStatusCode, resp.StatusCode));
			}
		}
	}
}
