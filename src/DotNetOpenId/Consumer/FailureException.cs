using System;

namespace DotNetOpenId.Consumer {
	public class FailureException : ProtocolException {
		public Uri IdentityUrl { get; private set; }

		public FailureException(Uri identityUrl, string message)
			: base(message) {
			IdentityUrl = identityUrl;
		}
	}
}