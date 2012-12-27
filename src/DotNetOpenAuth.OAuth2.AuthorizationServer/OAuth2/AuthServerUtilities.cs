//-----------------------------------------------------------------------
// <copyright file="AuthServerUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// Utility methods for authorization servers.
	/// </summary>
	internal static class AuthServerUtilities {
		/// <summary>
		/// Gets information about the client with a given identifier.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client information.  Never null.</returns>
		internal static IClientDescription GetClientOrThrow(this IAuthorizationServerHost authorizationServer, string clientIdentifier) {
			Requires.NotNullOrEmpty(clientIdentifier, "clientIdentifier");

			try {
				var result = authorizationServer.GetClient(clientIdentifier);
				ErrorUtilities.VerifyHost(result != null, OAuthStrings.ResultShouldNotBeNull, authorizationServer.GetType().FullName, "GetClient(string)");
				return result;
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, AuthServerStrings.ClientOrTokenSecretNotFound);
			} catch (ArgumentException ex) {
				throw ErrorUtilities.Wrap(ex, AuthServerStrings.ClientOrTokenSecretNotFound);
			}
		}

		/// <summary>
		/// Verifies a condition is true or throws an exception describing the problem.
		/// </summary>
		/// <param name="condition">The condition that evaluates to true to avoid an exception.</param>
		/// <param name="requestMessage">The request message.</param>
		/// <param name="error">A single error code from <see cref="Protocol.AccessTokenRequestErrorCodes"/>.</param>
		/// <param name="authenticationModule">The authentication module from which to glean the WWW-Authenticate header when applicable.</param>
		/// <param name="unformattedDescription">A human-readable UTF-8 encoded text providing additional information, used to assist the client developer in understanding the error that occurred.</param>
		/// <param name="args">The formatting arguments to generate the actual description.</param>
		internal static void TokenEndpointVerify(bool condition, AccessTokenRequestBase requestMessage, string error, ClientAuthenticationModule authenticationModule = null, string unformattedDescription = null, params object[] args) {
			if (!condition) {
				string description = unformattedDescription != null ? string.Format(CultureInfo.CurrentCulture, unformattedDescription, args) : null;

				string wwwAuthenticateHeader = null;
				if (authenticationModule != null) {
					wwwAuthenticateHeader = authenticationModule.AuthenticateHeader;
				}

				throw new TokenEndpointProtocolException(requestMessage, error, description, authenticateHeader: wwwAuthenticateHeader);
			}
		}
	}
}
