//-----------------------------------------------------------------------
// <copyright file="ClientCredentialReader.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	public abstract class ClientAuthenticationModuleBase : IClientAuthenticationModule {
		protected ClientAuthenticationModuleBase() {
		}

		public abstract ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier);

		public ClientAuthenticationResult TryAuthenticateClient(IDirectedProtocolMessage requestMessage, out string clientIdentifier) {
			return this.TryAuthenticateClient((AuthenticatedClientRequestBase)requestMessage, out clientIdentifier);
		}

		protected static ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, string clientIdentifier, string clientSecret) {
			Requires.NotNull(authorizationServerHost, "authorizationServerHost");

			if (!string.IsNullOrEmpty(clientIdentifier)) {
				var client = authorizationServerHost.GetClient(clientIdentifier);
				if (client != null) {
					if (!string.IsNullOrEmpty(clientSecret)) {
						if (client.IsValidClientSecret(clientSecret)) {
							return ClientAuthenticationResult.ClientAuthenticated;
						} else { // invalid client secret
							return ClientAuthenticationResult.ClientAuthenticationRejected;
						}
					} else { // no client secret provided
						return ClientAuthenticationResult.ClientIdNotAuthenticated;
					}
				} else { // The client identifier is not recognized.
					return ClientAuthenticationResult.ClientAuthenticationRejected;
				}
			} else { // no client id provided.
				return ClientAuthenticationResult.NoAuthenticationRecognized;
			}
		}
	}
}
