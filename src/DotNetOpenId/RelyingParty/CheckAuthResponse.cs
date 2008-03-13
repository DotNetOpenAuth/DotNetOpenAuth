using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	class CheckAuthResponse : DirectResponse {
		public CheckAuthResponse(Uri provider, IDictionary<string, string> args)
			: base(provider, args) {
		}

		public string InvalidatedAssociationHandle {
			get {
				string invalidateHandle = null;
				if (IsAuthenticationValid) {
					Args.TryGetValue(QueryStringArgs.openidnp.invalidate_handle, out invalidateHandle);
				}
				return invalidateHandle;
			}
		}

		public bool IsAuthenticationValid {
			get {
				string is_valid;
				Args.TryGetValue(QueryStringArgs.openidnp.is_valid, out is_valid);
				return QueryStringArgs.IsValid.True.Equals(is_valid, StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}
