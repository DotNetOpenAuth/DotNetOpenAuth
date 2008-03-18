using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.RelyingParty {
	abstract class DirectRequest {
		protected DirectRequest(Uri provider, IDictionary<string, string> args) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (args == null) throw new ArgumentNullException("args");
			Provider = provider;
			Args = args;
			Args.Add(QueryStringArgs.openid.ns, QueryStringArgs.OpenIdNs.v20);
		}
		protected Uri Provider { get; private set; }
		protected IDictionary<string, string> Args { get; private set; }

		protected IDictionary<string, string> GetResponse() {
			byte[] body = ProtocolMessages.Http.GetBytes(Args);
			FetchResponse resp = null;
			IDictionary<string, string> args = null;
			try {
				resp = Fetcher.Request(Provider, body);
				args = ProtocolMessages.KeyValueForm.GetDictionary(resp.ResponseStream);
			} catch (ArgumentException e) {
				throw new OpenIdException("Failure decoding Key-Value Form response from provider.", e);
			} catch (WebException e) {
				throw new OpenIdException("Failure while connecting to provider.", e);
			}
			switch (resp.StatusCode) {
				case HttpStatusCode.OK:
					return args;
				case HttpStatusCode.BadRequest:
					string providerMessage;
					args.TryGetValue(QueryStringArgs.openidnp.error, out providerMessage);
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ProviderRespondedWithError, providerMessage), 
						Util.DictionaryToNameValueCollection(args));
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ProviderRespondedWithUnrecognizedHTTPStatusCode, resp.StatusCode));
			}
		}
	}
}
