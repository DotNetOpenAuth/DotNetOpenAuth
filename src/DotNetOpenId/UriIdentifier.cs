using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Yadis;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;

namespace DotNetOpenId {
	class UriIdentifier : Identifier {
		static readonly string[] allowedSchemes = { "http", "https" };
		public static implicit operator Uri(UriIdentifier identifier) {
			if (identifier == null) return null;
			return identifier.Uri;
		}
		public static implicit operator UriIdentifier(Uri identifier) {
			if (identifier == null) return null;
			return new UriIdentifier(identifier);
		}

		public UriIdentifier(string uri) {
			if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");
			Uri canonicalUri;
			if (!TryCanonicalize(uri, out canonicalUri))
				throw new UriFormatException();
			Uri = canonicalUri;
		}
		public UriIdentifier(Uri uri) {
			if (uri == null) throw new ArgumentNullException("uri");
			if (!TryCanonicalize(new UriBuilder(uri), out uri))
				throw new UriFormatException();
			Uri = uri;
		}

		public Uri Uri { get; private set; }

		static bool isAllowedScheme(string uri) {
			if (string.IsNullOrEmpty(uri)) return false;
			return Array.FindIndex(allowedSchemes, s => uri.StartsWith(
				s + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase)) >= 0;
		}
		static bool isAllowedScheme(Uri uri) {
			if (uri == null) return false;
			return Array.FindIndex(allowedSchemes, s => 
				uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase)) >= 0;
		}
		static bool TryCanonicalize(string uri, out Uri canonicalUri) {
			canonicalUri = null;
			try {
				// Assume http:// scheme if an allowed scheme isn't given, and strip
				// fragments off.  Consistent with spec section 7.2#3
				if (!isAllowedScheme(uri)) uri = "http" + Uri.SchemeDelimiter + uri;
				if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute)) return false;
				// Use a UriBuilder because it helps to normalize the URL as well.
				return TryCanonicalize(new UriBuilder(uri), out canonicalUri);
			} catch (UriFormatException) {
				// We try not to land here with checks in the try block, but just in case.
				return false;
			}
		}
#if UNUSED
		static bool TryCanonicalize(string uri, out string canonicalUri) {
			Uri normalizedUri;
			bool result = TryCanonicalize(uri, out normalizedUri);
			canonicalUri = normalizedUri.AbsoluteUri;
			return result;
		}
#endif
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		static bool TryCanonicalize(UriBuilder uriBuilder, out Uri canonicalUri) {
			uriBuilder.Host = uriBuilder.Host.ToLowerInvariant();
			uriBuilder.Fragment = null;
			canonicalUri = uriBuilder.Uri;
			return true;
		}
		internal static bool IsValidUri(string uri) {
			Uri normalized;
			return TryCanonicalize(uri, out normalized);
		}
		internal static bool IsValidUri(Uri uri) {
			if (uri == null) return false;
			if (!uri.IsAbsoluteUri) return false;
			if (!isAllowedScheme(uri)) return false;
			return true;
		}

		public override bool Equals(object obj) {
			UriIdentifier other = obj as UriIdentifier;
			if (other == null) return false;
			return this.Uri == other.Uri;
		}
		public override int GetHashCode() {
			return Uri.GetHashCode();
		}
		public override string ToString() {
			return Uri.AbsoluteUri;
		}

	}
}
