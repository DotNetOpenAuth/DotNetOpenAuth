//-----------------------------------------------------------------------
// <copyright file="UriUtil.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text.RegularExpressions;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Utility methods for working with URIs.
	/// </summary>
	internal static class UriUtil {
		/// <summary>
		/// Tests a URI for the presence of an OAuth payload.
		/// </summary>
		/// <param name="uri">The URI to test.</param>
		/// <param name="prefix">The prefix.</param>
		/// <returns>
		/// True if the URI contains an OAuth message.
		/// </returns>
		internal static bool QueryStringContainPrefixedParameters(this Uri uri, string prefix) {
			Requires.NotNullOrEmpty(prefix, "prefix");
			if (uri == null) {
				return false;
			}

			return PortableUtilities.ParseQueryString(uri.Query)
				.Any(pair => pair.Key != null && pair.Key.StartsWith(prefix, StringComparison.Ordinal));
		}

		/// <summary>
		/// Determines whether some <see cref="Uri"/> is using HTTPS.
		/// </summary>
		/// <param name="uri">The Uri being tested for security.</param>
		/// <returns>
		/// 	<c>true</c> if the URI represents an encrypted request; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsTransportSecure(this Uri uri) {
			Requires.NotNull(uri, "uri");
			return string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Equivalent to UriBuilder.ToString() but omits port # if it may be implied.
		/// Equivalent to UriBuilder.Uri.ToString(), but doesn't throw an exception if the Host has a wildcard.
		/// </summary>
		/// <param name="builder">The UriBuilder to render as a string.</param>
		/// <returns>The string version of the Uri.</returns>
		internal static string ToStringWithImpliedPorts(this UriBuilder builder) {
			Requires.NotNull(builder, "builder");

			// We only check for implied ports on HTTP and HTTPS schemes since those
			// are the only ones supported by OpenID anyway.
			if ((builder.Port == 80 && string.Equals(builder.Scheme, "http", StringComparison.OrdinalIgnoreCase)) ||
				(builder.Port == 443 && string.Equals(builder.Scheme, "https", StringComparison.OrdinalIgnoreCase))) {
				// An implied port may be removed.
				string url = builder.ToString();

				// Be really careful to only remove the first :80 or :443 so we are guaranteed
				// we're removing only the port (and not something in the query string that 
				// looks like a port.
				string result = Regex.Replace(url, @"^(https?://[^:]+):\d+", m => m.Groups[1].Value, RegexOptions.IgnoreCase);
				Assumes.True(result != null); // Regex.Replace never returns null
				return result;
			} else {
				// The port must be explicitly given anyway.
				return builder.ToString();
			}
		}
	}
}
