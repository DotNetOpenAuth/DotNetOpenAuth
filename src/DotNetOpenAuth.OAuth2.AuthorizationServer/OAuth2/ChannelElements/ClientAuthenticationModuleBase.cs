//-----------------------------------------------------------------------
// <copyright file="ClientAuthenticationModuleBase.cs" company="Andrew Arnott">
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

	/// <summary>
	/// A convenient base class for imlementations of the <see cref="IClientAuthenticationModule"/> interface.
	/// </summary>
	public abstract class ClientAuthenticationModuleBase : IClientAuthenticationModule {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAuthenticationModuleBase"/> class.
		/// </summary>
		protected ClientAuthenticationModuleBase() {
		}

		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="requestMessage">The incoming message.</param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public abstract ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier);

		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="requestMessage">The incoming message.  Always an instance of <see cref="AuthenticatedClientRequestBase"/></param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public ClientAuthenticationResult TryAuthenticateClient(IDirectedProtocolMessage requestMessage, out string clientIdentifier) {
			return this.TryAuthenticateClient((AuthenticatedClientRequestBase)requestMessage, out clientIdentifier);
		}

		/// <summary>
		/// Validates a client identifier and shared secret against the authoriation server's database.
		/// </summary>
		/// <param name="authorizationServerHost">The authorization server host; cannot be <c>null</c>.</param>
		/// <param name="clientIdentifier">The alleged client identifier.</param>
		/// <param name="clientSecret">The alleged client secret to be verified.</param>
		/// <returns>An indication as to the outcome of the validation.</returns>
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
