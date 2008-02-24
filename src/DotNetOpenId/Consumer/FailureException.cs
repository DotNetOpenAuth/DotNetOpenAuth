using System;

namespace DotNetOpenId.Consumer {
	public class FailureException : ProtocolException {
		public FailureException(string message, Uri identityUrl)
			: base(message, identityUrl) {
		}
	}
}