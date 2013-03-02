namespace OAuth2ProtectedWebApi.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	using DotNetOpenAuth.Messaging.Bindings;

	internal class MemoryNonceStore : INonceStore {
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc) {
			return true;
		}
	}
}