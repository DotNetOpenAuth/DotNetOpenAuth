using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("{Exception.Message}")]
	class FailedAuthenticationResponse : IAuthenticationResponse {
		public FailedAuthenticationResponse(Exception exception) {
			Exception = exception;
		}

		#region IAuthenticationResponse Members

		public IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			return new Dictionary<string, string>();
		}

		public T GetExtension<T>() where T : DotNetOpenId.Extensions.IExtensionResponse, new() {
			return default(T);
		}

		public Identifier ClaimedIdentifier {
			get { return null; }
		}

		public AuthenticationStatus Status {
			get { return AuthenticationStatus.Failed; }
		}

		public Exception Exception { get; private set; }

		#endregion
	}
}
