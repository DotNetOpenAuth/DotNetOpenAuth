using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	class UriIdentifier : Identifier {
		public UriIdentifier(string uri) {
			if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");
			// Assume http:// scheme if an allowed scheme isn't given, and strip
			// fragments off.  Consistent with spec section 7.2#3
			if (!(uri.StartsWith("http://") || uri.StartsWith("https://")))
				uri = "http://" + uri;
			// Use a UriBuilder because it helps to normalize the URL as well.
			UriBuilder builder = new UriBuilder(uri);
			builder.Fragment = null;
			Uri = builder.Uri;
		}

		public Uri Uri { get; private set; }

		internal static bool IsValidUri(string uri) {
			return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
		}

		public override bool Equals(object obj) {
			UriIdentifier other = obj as UriIdentifier;
			if (other == null) return false;
			return this.Uri == other.Uri;
		}

		public override string ToString() {
			return Uri.ToString();
		}

	}
}
