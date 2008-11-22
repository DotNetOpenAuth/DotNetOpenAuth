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
	using DotNetOpenAuth.Messaging;
	using System.Text.RegularExpressions;

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

		/// <summary>
		/// Equivalent to UriBuilder.ToString() but omits port # if it may be implied.
		/// Equivalent to UriBuilder.Uri.ToString(), but doesn't throw an exception if the Host has a wildcard.
		/// </summary>
		public static string ToStringWithImpliedPorts(this UriBuilder builder) {
			ErrorUtilities.VerifyArgumentNotNull(builder, "builder");

			// We only check for implied ports on HTTP and HTTPS schemes since those
			// are the only ones supported by OpenID anyway.
			if ((builder.Port == 80 && string.Equals(builder.Scheme, "http", StringComparison.OrdinalIgnoreCase)) ||
				(builder.Port == 443 && string.Equals(builder.Scheme, "https", StringComparison.OrdinalIgnoreCase))) {
				// An implied port may be removed.
				string url = builder.ToString();
				// Be really careful to only remove the first :80 or :443 so we are guaranteed
				// we're removing only the port (and not something in the query string that 
				// looks like a port.
				return Regex.Replace(url, @"^(https?://[^:]+):\d+", m => m.Groups[1].Value, RegexOptions.IgnoreCase);
			} else {
				// The port must be explicitly given anyway.
				return builder.ToString();
			}
		}
	}
}
