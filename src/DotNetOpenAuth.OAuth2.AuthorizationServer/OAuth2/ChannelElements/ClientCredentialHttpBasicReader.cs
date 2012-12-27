//-----------------------------------------------------------------------
// <copyright file="ClientCredentialHttpBasicReader.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// Reads client authentication information from the HTTP Authorization header via Basic authentication.
	/// </summary>
	public class ClientCredentialHttpBasicReader : ClientAuthenticationModule {
		/// <summary>
		/// Gets this module's contribution to an HTTP 401 WWW-Authenticate header so the client knows what kind of authentication this module supports.
		/// </summary>
		public override string AuthenticateHeader {
			get { return string.Format(CultureInfo.InvariantCulture, "Basic realm=\"{0}\"", this.Realm); }
		}

		/// <summary>
		/// Gets or sets the realm that is included in an HTTP WWW-Authenticate header included in a 401 Unauthorized response.
		/// </summary>
		public string Realm { get; set; }

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
				return TryAuthenticateClientBySecret(authorizationServerHost, credential.UserName, credential.Password);
			}

			clientIdentifier = null;
			return ClientAuthenticationResult.NoAuthenticationRecognized;
		}
	}
}
