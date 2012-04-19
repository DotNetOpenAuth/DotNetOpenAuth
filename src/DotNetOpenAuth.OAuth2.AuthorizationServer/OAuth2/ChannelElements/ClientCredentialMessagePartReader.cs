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

	/// <summary>
	/// Reads client authentication information from the message payload itself (POST entity as a URI-encoded parameter).
	/// </summary>
	public class ClientCredentialMessagePartReader : ClientAuthenticationModuleBase {
		/// <summary>
		/// The authorization server host.
		/// </summary>
		private readonly IAuthorizationServerHost authorizationServerHost;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientCredentialMessagePartReader"/> class.
		/// </summary>
		/// <param name="authorizationServerHost">The authorization server host.</param>
		public ClientCredentialMessagePartReader(IAuthorizationServerHost authorizationServerHost) {
			Requires.NotNull(authorizationServerHost, "authorizationServerHost");
			this.authorizationServerHost = authorizationServerHost;
		}

		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="requestMessage">The incoming message.</param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public override ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier) {
			Requires.NotNull(requestMessage, "requestMessage");
			clientIdentifier = requestMessage.ClientIdentifier;
			return TryAuthenticateClient(this.authorizationServerHost, requestMessage.ClientIdentifier, requestMessage.ClientSecret);
		}
	}
}
