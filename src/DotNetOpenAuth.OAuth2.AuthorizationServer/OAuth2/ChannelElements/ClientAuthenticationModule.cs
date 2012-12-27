//-----------------------------------------------------------------------
// <copyright file="ClientAuthenticationModule.cs" company="Andrew Arnott">
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
	using Validation;

	/// <summary>
	/// A base class for extensions that can read incoming messages and extract the client identifier and
	/// possibly authentication information (like a shared secret, signed nonce, etc.)
	/// </summary>
	public abstract class ClientAuthenticationModule {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAuthenticationModule"/> class.
		/// </summary>
		protected ClientAuthenticationModule() {
		}

		/// <summary>
		/// Gets this module's contribution to an HTTP 401 WWW-Authenticate header so the client knows what kind of authentication this module supports.
		/// </summary>
		public virtual string AuthenticateHeader {
			get { return null; }
		}

		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="authorizationServerHost">The authorization server host.</param>
		/// <param name="requestMessage">The incoming message.</param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public abstract ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier);

		/// <summary>
		/// Validates a client identifier and shared secret against the authoriation server's database.
		/// </summary>
		/// <param name="authorizationServerHost">The authorization server host; cannot be <c>null</c>.</param>
		/// <param name="clientIdentifier">The alleged client identifier.</param>
		/// <param name="clientSecret">The alleged client secret to be verified.</param>
		/// <returns>An indication as to the outcome of the validation.</returns>
		protected static ClientAuthenticationResult TryAuthenticateClientBySecret(IAuthorizationServerHost authorizationServerHost, string clientIdentifier, string clientSecret) {
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
