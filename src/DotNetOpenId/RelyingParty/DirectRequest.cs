using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace DotNetOpenId.RelyingParty {
	abstract class DirectRequest {
		protected DirectRequest(Uri provider, IDictionary<string, string> args) {
			Provider = provider;
			Args = args;
		}
		protected Uri Provider { get; private set; }
		protected IDictionary<string, string> Args { get; private set; }

		protected IDictionary<string, string> GetResponse() {
			byte[] body = ProtocolMessages.Http.GetBytes(Args);

			try {
				FetchResponse resp = Fetcher.Request(Provider, body);
				IDictionary<string, string> data = ProtocolMessages.KeyValueForm.GetDictionary(new MemoryStream(resp.Data, 0, resp.Length));
				if (resp.Code == HttpStatusCode.BadRequest) {
					if (TraceUtil.Switch.TraceError) {
						Trace.TraceError("Bad request code returned from remote server: {0}.", resp.Code);
					}
					string errorMessage;
					if (data.TryGetValue(QueryStringArgs.openidnp.error, out errorMessage)) {
						throw new OpenIdException(string.Format(CultureInfo.CurrentCulture, "OpenID Provider returned an error: '{0}'.", errorMessage));
					} else {
						throw new OpenIdException("OpenID Provider returned an error but is missing the error message.");
					}
				} else if ((int)resp.Code < 200 || (int)resp.Code > 300) {
					if (TraceUtil.Switch.TraceError) {
						Trace.TraceError("Unexpcted HTTP code in direct response: {0}.", resp.Code);
					}
				}

				return data;
			} catch (ArgumentException e) {
				if (TraceUtil.Switch.TraceWarning)
					Trace.TraceWarning("Failure decoding Key-Value Form response from provider.");
				return null;
			} catch (WebException e) {
				Trace.TraceError("Failure while connecting to remote server: {0}.", e.Message);
				return null;
			}
		}
	}
}
