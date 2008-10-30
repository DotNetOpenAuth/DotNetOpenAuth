using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Interop {
	[ComVisible(true)]
	public class AuthenticationResponseShim {
		private readonly IAuthenticationResponse response;

		internal AuthenticationResponseShim(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			this.response = response;
		}

		public string ClaimedIdentifier {
			get { return this.response.ClaimedIdentifier; }
		}
		public string FriendlyIdentifierForDisplay {
			get { return this.response.FriendlyIdentifierForDisplay; }
		}
		public bool Successful {
			get { return this.response.Status == AuthenticationStatus.Authenticated; }
		}
		public string ExceptionMessage {
			get { return this.response.Exception != null ? this.response.Exception.Message : null; }
		}
	}
}
