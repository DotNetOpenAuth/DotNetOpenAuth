using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("IsAuthenticationValid: {IsAuthenticationValid}, OpenId: {Protocol.Version}")]
	class CheckAuthResponse : DirectResponse {
		public CheckAuthResponse(ServiceEndpoint provider, IDictionary<string, string> args)
			: base(provider, args) {
		}

		public string InvalidatedAssociationHandle {
			get {
				if (IsAuthenticationValid) {
					return Util.GetOptionalArg(Args, Protocol.openidnp.invalidate_handle);
				}
				return null;
			}
		}

		public bool IsAuthenticationValid {
			get {
				return Protocol.Args.IsValid.True.Equals(
					Util.GetRequiredArg(Args, Protocol.openidnp.is_valid), StringComparison.Ordinal);
			}
		}
	}
}
