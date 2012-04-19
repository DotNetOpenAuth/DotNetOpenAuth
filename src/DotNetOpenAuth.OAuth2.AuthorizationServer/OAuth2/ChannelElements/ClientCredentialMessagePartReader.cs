//-----------------------------------------------------------------------
// <copyright file="ClientCredentialMessagePartReader.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.OAuth2.Messages;

	public class ClientCredentialMessagePartReader : ClientAuthenticationModuleBase {
		private readonly IAuthorizationServerHost authorizationServerHost;

		public ClientCredentialMessagePartReader(IAuthorizationServerHost authorizationServerHost) {
			Requires.NotNull(authorizationServerHost, "authorizationServerHost");
			this.authorizationServerHost = authorizationServerHost;
		}

		public override ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier) {
			Requires.NotNull(requestMessage, "requestMessage");
			clientIdentifier = requestMessage.ClientIdentifier;
			return TryAuthenticateClient(this.authorizationServerHost, requestMessage.ClientIdentifier, requestMessage.ClientSecret);
		}
	}
}
