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

	/// <summary>
	/// Reads client authentication information from the HTTP Authorization header via Basic authentication.
	/// </summary>
	public class ClientCredentialHttpBasicReader : ClientAuthenticationModule {
		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="authorizationServerHost">The authorization server host.</param>
		/// <param name="requestMessage">The incoming message.</param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public override ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier) {
			Requires.NotNull(authorizationServerHost, "authorizationServerHost");
			Requires.NotNull(requestMessage, "requestMessage");

			var credential = OAuthUtilities.ParseHttpBasicAuth(requestMessage.Headers);
			if (credential != null) {
				clientIdentifier = credential.UserName;
				return TryAuthenticateClient(authorizationServerHost, credential.UserName, credential.Password);
			}

			clientIdentifier = null;
			return ClientAuthenticationResult.NoAuthenticationRecognized;
		}
	}
}
