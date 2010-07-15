//-----------------------------------------------------------------------
// <copyright file="OAuthUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// The delimiter between scope elements.
		/// </summary>
		private static char[] scopeDelimiter = new char[] { ' ' };

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
		/// <param name="scope">The scope.</param>
		/// <param name="scopeComparer">The scope comparer, allowing scopes to be case sensitive or insensitive.
		/// Usually <see cref="StringComparer.Ordinal"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>.</param>
		/// <returns></returns>
		public static HashSet<string> BreakUpScopes(string scope, StringComparer scopeComparer) {
			Contract.Requires<ArgumentNullException>(scopeComparer != null, "scopeComparer");

			if (string.IsNullOrEmpty(scope)) {
				return new HashSet<string>();
			}

			return new HashSet<string>(scope.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries), scopeComparer);
		}

		/// <summary>
		/// Authorizes an HTTP request using an OAuth 2.0 access token in an HTTP Authorization header.
		/// </summary>
		/// <param name="request">The request to authorize.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		internal static void AuthorizeWithOAuthWrap(this HttpWebRequest request, string accessToken) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(accessToken));
			request.Headers[HttpRequestHeader.Authorization] = string.Format(
				CultureInfo.InvariantCulture,
				Protocol.HttpAuthorizationHeaderFormat,
				Uri.EscapeDataString(accessToken));
		}

		/// <summary>
		/// Gets information about the client with a given identifier.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client information.  Never null.</returns>
		internal static IConsumerDescription GetClientOrThrow(this IAuthorizationServer authorizationServer, string clientIdentifier) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);

			try {
				return authorizationServer.GetClient(clientIdentifier);
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuth.OAuthStrings.ConsumerOrTokenSecretNotFound);
			} catch (ArgumentException ex) {
				throw ErrorUtilities.Wrap(ex, OAuth.OAuthStrings.ConsumerOrTokenSecretNotFound);
			}
		}
	}
}
