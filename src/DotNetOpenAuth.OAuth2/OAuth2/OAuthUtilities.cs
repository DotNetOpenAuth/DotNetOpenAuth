﻿//-----------------------------------------------------------------------
// <copyright file="OAuthUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Some common utility methods for OAuth 2.0.
	/// </summary>
	public static class OAuthUtilities {
		/// <summary>
		/// The <see cref="StringComparer"/> instance to use when comparing scope equivalence.
		/// </summary>
		public static readonly StringComparer ScopeStringComparer = StringComparer.Ordinal;

		/// <summary>
		/// The delimiter between scope elements.
		/// </summary>
		private static char[] scopeDelimiter = new char[] { ' ' };

		/// <summary>
		/// The characters that may appear in an access token that is included in an HTTP Authorization header.
		/// </summary>
		/// <remarks>
		/// This is defined in OAuth 2.0 DRAFT 10, section 5.1.1. (http://tools.ietf.org/id/draft-ietf-oauth-v2-10.html#authz-header)
		/// </remarks>
		private static string accessTokenAuthorizationHeaderAllowedCharacters = MessagingUtilities.UppercaseLetters +
		                                                                        MessagingUtilities.LowercaseLetters +
		                                                                        MessagingUtilities.Digits +
		                                                                        @"!#$%&'()*+-./:<=>?@[]^_`{|}~\,;";

		/// <summary>
		/// Determines whether one given scope is a subset of another scope.
		/// </summary>
		/// <param name="requestedScope">The requested scope, which may be a subset of <paramref name="grantedScope"/>.</param>
		/// <param name="grantedScope">The granted scope, the suspected superset.</param>
		/// <returns>
		/// 	<c>true</c> if all the elements that appear in <paramref name="requestedScope"/> also appear in <paramref name="grantedScope"/>;
		/// <c>false</c> otherwise.
		/// </returns>
		public static bool IsScopeSubset(string requestedScope, string grantedScope) {
			if (string.IsNullOrEmpty(requestedScope)) {
				return true;
			}

			if (string.IsNullOrEmpty(grantedScope)) {
				return false;
			}

			var requestedScopes = new HashSet<string>(requestedScope.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries));
			var grantedScopes = new HashSet<string>(grantedScope.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries));
			return requestedScopes.IsSubsetOf(grantedScopes);
		}

		/// <summary>
		/// Identifies individual scope elements
		/// </summary>
		/// <param name="scope">The space-delimited list of scopes.</param>
		/// <returns>A set of individual scopes, with any duplicates removed.</returns>
		public static HashSet<string> SplitScopes(string scope) {
			if (string.IsNullOrEmpty(scope)) {
				return new HashSet<string>();
			}

			var set = new HashSet<string>(scope.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries), ScopeStringComparer);
			VerifyValidScopeTokens(set);
			return set;
		}

		/// <summary>
		/// Serializes a set of scopes as a space-delimited list.
		/// </summary>
		/// <param name="scopes">The scopes to serialize.</param>
		/// <returns>A space-delimited list.</returns>
		public static string JoinScopes(HashSet<string> scopes) {
			Requires.NotNull(scopes, "scopes");
			VerifyValidScopeTokens(scopes);
			return string.Join(" ", scopes.ToArray());
		}

		/// <summary>
		/// Verifies that a sequence of scope tokens are all valid.
		/// </summary>
		/// <param name="scopes">The scopes.</param>
		internal static void VerifyValidScopeTokens(IEnumerable<string> scopes) {
			Requires.NotNull(scopes, "scopes");
			foreach (string scope in scopes) {
				VerifyValidScopeToken(scope);
			}
		}

		/// <summary>
		/// Verifies that a given scope token (not a space-delimited set, but a single token) is valid.
		/// </summary>
		/// <param name="scopeToken">The scope token.</param>
		internal static void VerifyValidScopeToken(string scopeToken) {
			ErrorUtilities.VerifyProtocol(!String.IsNullOrEmpty(scopeToken), OAuthStrings.InvalidScopeToken, scopeToken);
			for (int i = 0; i < scopeToken.Length; i++) {
				// The allowed set of characters comes from OAuth 2.0 section 3.3 (draft 23)
				char ch = scopeToken[i];
				if (!(ch == '\x21' || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E'))) {
					ErrorUtilities.ThrowProtocol(OAuthStrings.InvalidScopeToken, scopeToken);
				}
			}
		}

		/// <summary>
		/// Authorizes an HTTP request using an OAuth 2.0 access token in an HTTP Authorization header.
		/// </summary>
		/// <param name="request">The request to authorize.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		internal static void AuthorizeWithBearerToken(this HttpWebRequest request, string accessToken) {
			Requires.NotNull(request, "request");
			Requires.NotNullOrEmpty(accessToken, "accessToken");
			ErrorUtilities.VerifyProtocol(accessToken.All(ch => accessTokenAuthorizationHeaderAllowedCharacters.IndexOf(ch) >= 0), OAuthStrings.AccessTokenInvalidForHttpAuthorizationHeader);

			request.Headers[HttpRequestHeader.Authorization] = string.Format(
				CultureInfo.InvariantCulture,
				Protocol.BearerHttpAuthorizationHeaderFormat,
				accessToken);
		}

		/// <summary>
		/// Gets information about the client with a given identifier.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client information.  Never null.</returns>
		internal static IClientDescription GetClientOrThrow(this IAuthorizationServer authorizationServer, string clientIdentifier) {
			Requires.NotNullOrEmpty(clientIdentifier, "clientIdentifier");
			Contract.Ensures(Contract.Result<IClientDescription>() != null);

			try {
				var result = authorizationServer.GetClient(clientIdentifier);
				ErrorUtilities.VerifyHost(result != null, OAuthStrings.ResultShouldNotBeNull, authorizationServer.GetType().FullName, "GetClient(string)");
				return result;
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuthStrings.ClientOrTokenSecretNotFound);
			} catch (ArgumentException ex) {
				throw ErrorUtilities.Wrap(ex, OAuthStrings.ClientOrTokenSecretNotFound);
			}
		}
	}
}
