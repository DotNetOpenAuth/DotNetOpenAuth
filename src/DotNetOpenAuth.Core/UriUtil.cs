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
	using System.Web;
	using System.Web.UI;
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

			NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
			Assumes.True(nvc != null); // BCL
			return nvc.Keys.OfType<string>().Any(key => key.StartsWith(prefix, StringComparison.Ordinal));
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

		/// <summary>
		/// Validates that a URL will be resolvable at runtime.
		/// </summary>
		/// <param name="page">The page hosting the control that receives this URL as a property.</param>
		/// <param name="designMode">If set to <c>true</c> the page is in design-time mode rather than runtime mode.</param>
		/// <param name="value">The URI to check.</param>
		/// <exception cref="UriFormatException">Thrown if the given URL is not a valid, resolvable URI.</exception>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Just to throw an exception on invalid input.")]
		internal static void ValidateResolvableUrl(Page page, bool designMode, string value) {
			if (string.IsNullOrEmpty(value)) {
				return;
			}

			if (page != null && !designMode) {
				Assumes.True(page.Request != null);

				// Validate new value by trying to construct a Realm object based on it.
				string relativeUrl = page.ResolveUrl(value);
				Assumes.True(page.Request.Url != null);
				Assumes.True(relativeUrl != null);
				new Uri(page.Request.Url, relativeUrl); // throws an exception on failure.
			} else {
				// We can't fully test it, but it should start with either ~/ or a protocol.
				if (Regex.IsMatch(value, @"^https?://")) {
					new Uri(value); // make sure it's fully-qualified, but ignore wildcards
				} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
					// this is valid too
				} else {
					throw new UriFormatException();
				}
			}
		}
	}
}
