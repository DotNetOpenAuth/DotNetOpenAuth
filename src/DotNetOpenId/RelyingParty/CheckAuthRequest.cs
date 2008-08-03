using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class CheckAuthRequest : DirectRequest {
		CheckAuthRequest(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, IDictionary<string, string> args) :
			base(relyingParty, provider, args) {
		}

		public static CheckAuthRequest Create(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, IDictionary<string, string> query) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			Protocol protocol = provider.Protocol;
			string signed = query[protocol.openid.signed];

			if (signed == null)
				// #XXX: oidutil.log('No signature present; checkAuth aborted')
				return null;

			// Arguments that are always passed to the server and not
			// included in the signature.
			string[] whitelist = new string[] { protocol.openidnp.assoc_handle, protocol.openidnp.sig, protocol.openidnp.signed, protocol.openidnp.invalidate_handle };
			string[] splitted = signed.Split(',');

			// combine the previous 2 arrays (whitelist + splitted) into a new array: signed_array
			string[] signed_array = new string[whitelist.Length + splitted.Length];
			Array.Copy(whitelist, signed_array, whitelist.Length);
			Array.Copy(splitted, 0, signed_array, whitelist.Length, splitted.Length);

			var check_args = new Dictionary<string, string>();

			foreach (string key in query.Keys) {
				if (key.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)
					&& Array.IndexOf(signed_array, key.Substring(protocol.openid.Prefix.Length)) > -1)
					check_args[key] = query[key];
			}
			check_args[protocol.openid.mode] = protocol.Args.Mode.check_authentication;

			return new CheckAuthRequest(relyingParty, provider, check_args);
		}

		CheckAuthResponse response;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] // getter executes code
		public CheckAuthResponse Response {
			get {
				if (response == null) {
					response = new CheckAuthResponse(RelyingParty, Provider, GetResponse());
				}
				return response;
			}
		}
	}
}
