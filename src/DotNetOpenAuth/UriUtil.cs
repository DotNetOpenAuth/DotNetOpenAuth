//-----------------------------------------------------------------------
// <copyright file="UriUtil.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Web;

	/// <summary>
	/// Utility methods for working with URIs.
	/// </summary>
	internal static class UriUtil {
		/// <summary>
		/// Tests a URI for the presence of an OAuth payload.
		/// </summary>
		/// <param name="uri">The URI to test.</param>
		/// <returns>True if the URI contains an OAuth message.</returns>
		internal static bool QueryStringContainsOAuthParameters(Uri uri) {
			if (uri == null) {
				return false;
			}

			NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
			return nvc.Keys.OfType<string>().Any(key => key.StartsWith(OAuth.Protocol.V10.ParameterPrefix, StringComparison.Ordinal));
		}

		/// <summary>
		/// Determines whether some <see cref="Uri"/> is using HTTPS.
		/// </summary>
		/// <param name="uri">The Uri being tested for security.</param>
		/// <returns>
		/// 	<c>true</c> if the URI represents an encrypted request; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsTransportSecure(this Uri uri) {
			if (uri == null) {
				throw new ArgumentNullException("uri");
			}

			return string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
		}
	}
}
