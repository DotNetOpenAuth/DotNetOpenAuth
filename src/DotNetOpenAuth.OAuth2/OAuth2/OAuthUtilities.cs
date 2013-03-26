//-----------------------------------------------------------------------
// <copyright file="OAuthUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http.Headers;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	using HttpRequestHeaders = DotNetOpenAuth.Messaging.HttpRequestHeaders;

	/// <summary>
	/// Some common utility methods for OAuth 2.0.
	/// </summary>
	public static class OAuthUtilities {
		/// <summary>
		/// The <see cref="StringComparer"/> instance to use when comparing scope equivalence.
		/// </summary>
		public static readonly StringComparer ScopeStringComparer = StringComparer.Ordinal;

		/// <summary>
		/// The string "Basic ".
		/// </summary>
		private const string HttpBasicAuthScheme = "Basic";

		/// <summary>
		/// The delimiter between scope elements.
		/// </summary>
		private static readonly char[] scopeDelimiter = new char[] { ' ' };

		/// <summary>
		/// A colon, in a 1-length character array.
		/// </summary>
		private static readonly char[] ColonSeparator = new char[] { ':' };

		/// <summary>
		/// The encoding to use when preparing credentials for transit in HTTP Basic base64 encoding form.
		/// </summary>
		private static readonly Encoding HttpBasicEncoding = Encoding.UTF8;

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
		/// Parses a space-delimited list of scopes into a set.
		/// </summary>
		/// <param name="scopes">The space-delimited string.</param>
		/// <returns>A set.</returns>
		internal static HashSet<string> ParseScopeSet(string scopes) {
			Requires.NotNull(scopes, "scopes");
			return ParseScopeSet(scopes.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries));
		}

		/// <summary>
		/// Creates a set out of an array of strings.
		/// </summary>
		/// <param name="scopes">The array of strings.</param>
		/// <returns>A set.</returns>
		internal static HashSet<string> ParseScopeSet(string[] scopes) {
			Requires.NotNull(scopes, "scopes");
			return new HashSet<string>(scopes, StringComparer.Ordinal);
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
			ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(scopeToken), OAuthStrings.InvalidScopeToken, scopeToken);
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

			AuthorizeWithBearerToken(request.Headers, accessToken);
		}

		/// <summary>
		/// Authorizes an HTTP request using an OAuth 2.0 access token in an HTTP Authorization header.
		/// </summary>
		/// <param name="requestHeaders">The headers on the request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		internal static void AuthorizeWithBearerToken(WebHeaderCollection requestHeaders, string accessToken) {
			Requires.NotNull(requestHeaders, "requestHeaders");
			Requires.NotNullOrEmpty(accessToken, "accessToken");
			ErrorUtilities.VerifyProtocol(accessToken.All(ch => accessTokenAuthorizationHeaderAllowedCharacters.IndexOf(ch) >= 0), OAuthStrings.AccessTokenInvalidForHttpAuthorizationHeader);

			requestHeaders[HttpRequestHeader.Authorization] = string.Format(
				CultureInfo.InvariantCulture,
				Protocol.BearerHttpAuthorizationHeaderFormat,
				accessToken);
		}

		/// <summary>
		/// Applies the HTTP Authorization header for HTTP Basic authentication.
		/// </summary>
		/// <param name="headers">The headers collection to set the authorization header to.</param>
		/// <param name="userName">The username.  Cannot be empty.</param>
		/// <param name="password">The password.  Cannot be null.</param>
		internal static void ApplyHttpBasicAuth(System.Net.Http.Headers.HttpRequestHeaders headers, string userName, string password) {
			Requires.NotNull(headers, "headers");
			Requires.NotNullOrEmpty(userName, "userName");
			Requires.NotNull(password, "password");

			string concat = userName + ":" + password;
			byte[] bits = HttpBasicEncoding.GetBytes(concat);
			string base64 = Convert.ToBase64String(bits);
			headers.Authorization = new AuthenticationHeaderValue(HttpBasicAuthScheme, base64);
		}

		/// <summary>
		/// Extracts the username and password from an HTTP Basic authorized web header.
		/// </summary>
		/// <param name="headers">The incoming web headers.</param>
		/// <returns>The network credentials; or <c>null</c> if none could be discovered in the request.</returns>
		internal static NetworkCredential ParseHttpBasicAuth(System.Net.Http.Headers.HttpRequestHeaders headers) {
			Requires.NotNull(headers, "headers");

			var authorizationHeader = headers.Authorization;
			if (authorizationHeader != null && string.Equals(authorizationHeader.Scheme, HttpBasicAuthScheme, StringComparison.Ordinal)) {
				string base64 = authorizationHeader.Parameter;
				byte[] bits = Convert.FromBase64String(base64);
				string usernameColonPassword = HttpBasicEncoding.GetString(bits);
				string[] usernameAndPassword = usernameColonPassword.Split(ColonSeparator, 2);
				if (usernameAndPassword.Length == 2) {
					return new NetworkCredential(usernameAndPassword[0], usernameAndPassword[1]);
				}
			}

			return null;
		}
	}
}
