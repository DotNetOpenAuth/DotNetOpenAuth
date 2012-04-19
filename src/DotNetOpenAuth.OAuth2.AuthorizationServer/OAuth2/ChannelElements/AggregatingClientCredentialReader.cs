//-----------------------------------------------------------------------
// <copyright file="AggregatingClientCredentialReader.cs" company="Andrew Arnott">
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

	/// <summary>
	/// Applies OAuth 2 spec policy for supporting multiple methods of client authentication.
	/// </summary>
	internal class AggregatingClientCredentialReader : ClientAuthenticationModuleBase {
		/// <summary>
		/// The set of authenticators to apply to an incoming request.
		/// </summary>
		private readonly IEnumerable<IClientAuthenticationModule> authenticators;

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregatingClientCredentialReader"/> class.
		/// </summary>
		/// <param name="authenticators">The set of authentication modules to apply.</param>
		internal AggregatingClientCredentialReader(IEnumerable<IClientAuthenticationModule> authenticators) {
			Requires.NotNull(authenticators, "readers");
			this.authenticators = authenticators;
		}

		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="requestMessage">The incoming message.</param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		public override ClientAuthenticationResult TryAuthenticateClient(AuthenticatedClientRequestBase requestMessage, out string clientIdentifier) {
			Requires.NotNull(requestMessage, "requestMessage");

			IClientAuthenticationModule authenticator = null;
			ClientAuthenticationResult result = ClientAuthenticationResult.NoAuthenticationRecognized;
			clientIdentifier = null;

			foreach (var candidateAuthenticator in this.authenticators) {
				string candidateClientIdentifier;
				var resultCandidate = candidateAuthenticator.TryAuthenticateClient(requestMessage, out candidateClientIdentifier);

				ErrorUtilities.VerifyProtocol(
					result == ClientAuthenticationResult.NoAuthenticationRecognized || resultCandidate == ClientAuthenticationResult.NoAuthenticationRecognized,
					"Message rejected because multiple forms of client authentication ({0} and {1}) were detected, which is forbidden by the OAuth 2 Protocol Framework specification.",
					authenticator,
					candidateAuthenticator);

				if (resultCandidate != ClientAuthenticationResult.NoAuthenticationRecognized) {
					authenticator = candidateAuthenticator;
					result = resultCandidate;
					clientIdentifier = candidateClientIdentifier;
				}
			}

			return result;
		}
	}
}
