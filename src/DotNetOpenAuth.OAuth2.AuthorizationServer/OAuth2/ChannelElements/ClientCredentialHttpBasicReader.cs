//-----------------------------------------------------------------------
// <copyright file="ClientCredentialHttpBasicReader.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	public class ClientCredentialHttpBasicReader : ClientAuthenticationModuleBase {
		private readonly IAuthorizationServerHost authorizationServerHost;

		public ClientCredentialHttpBasicReader(IAuthorizationServerHost authorizationServerHost) {
			Requires.NotNull(authorizationServerHost, "authorizationServerHost");
			this.authorizationServerHost = authorizationServerHost;
		}

		public override ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier) {
			Requires.NotNull(requestMessage, "requestMessage");

			var credential = OAuthUtilities.ParseHttpBasicAuth(requestMessage.Headers);
			if (credential != null) {
				clientIdentifier = credential.UserName;
				return TryAuthenticateClient(this.authorizationServerHost, credential.UserName, credential.Password);
			}

			clientIdentifier = null;
			return ClientAuthenticationResult.NoAuthenticationRecognized;
		}
	}
}
