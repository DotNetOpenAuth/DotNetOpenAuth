using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	class CheckAuthRequest : DirectRequest {
		CheckAuthRequest(Uri provider, IDictionary<string, string> args) :
			base(provider, args) {
		}

		public static CheckAuthRequest Create(Uri provider, IDictionary<string, string> query) {
			Protocol protocol = Protocol.Default;
			string signed = query[Protocol.Default.openid.signed];

			if (signed == null)
				// #XXX: oidutil.log('No signature present; checkAuth aborted')
				return null;

			// Arguments that are always passed to the server and not
			// included in the signature.
			string[] whitelist = new string[] { Protocol.Default.openidnp.assoc_handle, Protocol.Default.openidnp.sig, Protocol.Default.openidnp.signed, Protocol.Default.openidnp.invalidate_handle };
			string[] splitted = signed.Split(',');

			// combine the previous 2 arrays (whitelist + splitted) into a new array: signed_array
			string[] signed_array = new string[whitelist.Length + splitted.Length];
			Array.Copy(whitelist, signed_array, whitelist.Length);
			Array.Copy(splitted, 0, signed_array, whitelist.Length, splitted.Length);

			var check_args = new Dictionary<string, string>();

			foreach (string key in query.Keys) {
				if (key.StartsWith(Protocol.Default.openid.Prefix, StringComparison.OrdinalIgnoreCase)
					&& Array.IndexOf(signed_array, key.Substring(Protocol.Default.openid.Prefix.Length)) > -1)
					check_args[key] = query[key];
			}
			check_args[Protocol.Default.openid.mode] = protocol.Args.Mode.check_authentication;

			return new CheckAuthRequest(provider, check_args);
		}

		CheckAuthResponse response;
		public CheckAuthResponse Response {
			get {
				if (response == null) {
					response = new CheckAuthResponse(Provider, GetResponse());
				}
				return response;
			}
		}
	}
}
