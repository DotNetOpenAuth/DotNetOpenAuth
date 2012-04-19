namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;

	public enum ClientAuthenticationResult {
		NoAuthenticationRecognized,

		ClientIdNotAuthenticated,

		ClientAuthenticated,

		ClientAuthenticationRejected,
	}

	public interface IClientAuthenticationModule {
		ClientAuthenticationResult TryAuthenticateClient(IDirectedProtocolMessage requestMessage, out string clientIdentifier);
	}
}
