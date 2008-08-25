using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace DotNetOpenId.RelyingParty {
	internal class DirectMessageHttpChannel : IDirectMessageChannel {
		#region IDirectMessageChannel Members

		public IDictionary<string, string> SendDirectMessageAndGetResponse(ServiceEndpoint provider, IDictionary<string, string> fields) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (fields == null) throw new ArgumentNullException("fields");

			byte[] body = ProtocolMessages.Http.GetBytes(fields);
			IDictionary<string, string> args;
			UntrustedWebResponse resp = null;
			string fullResponseText = null;
			try {
				resp = UntrustedWebRequest.Request(provider.ProviderEndpoint, body);
				// If an internal server error occurred, there won't be any KV-form stream
				// to read in.  So instead, preserve whatever error the server did send back
				// and throw it in the exception.
				if (resp.StatusCode == HttpStatusCode.InternalServerError) {
					string errorStream = new StreamReader(resp.ResponseStream).ReadToEnd();
					throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
						Strings.ProviderRespondedWithError, errorStream));
				}
				if (Logger.IsDebugEnabled) {
					fullResponseText = resp.ReadResponseString();
				}
				args = ProtocolMessages.KeyValueForm.GetDictionary(resp.ResponseStream);
				Logger.DebugFormat("Received direct response from {0}: {1}{2}", provider.ProviderEndpoint,
					Environment.NewLine, Util.ToString(args));
			} catch (ArgumentException e) {
				Logger.DebugFormat("Full response from provider (where KVF was expected):{0}{1}",
					Environment.NewLine, fullResponseText);
				throw new OpenIdException("Failure decoding Key-Value Form response from provider.", e);
			} catch (WebException e) {
				throw new OpenIdException("Failure while connecting to provider.", e);
			}
			// All error codes are supposed to be returned with 400, but
			// some (like myopenid.com) sometimes send errors as 200's.
			if (resp.StatusCode == HttpStatusCode.BadRequest ||
				Util.GetOptionalArg(args, provider.Protocol.openidnp.mode) == provider.Protocol.Args.Mode.error) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ProviderRespondedWithError,
					Util.GetOptionalArg(args, provider.Protocol.openidnp.error)), args);
			} else if (resp.StatusCode == HttpStatusCode.OK) {
				return args;
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ProviderRespondedWithUnrecognizedHTTPStatusCode, resp.StatusCode));
			}
		}

		#endregion
	}
}
