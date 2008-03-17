using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	/// <summary>
	/// An Identifier is either a "http" or "https" URI, or an XRI.
	/// </summary>
	public abstract class Identifier {
		/// <summary>
		/// Converts the string representation of an Identifier to its strong type.
		/// </summary>
		public static implicit operator Identifier(string identifier) {
			return Parse(identifier);
		}
		/// <summary>
		/// Returns a strongly-typed Identifier for a given Uri.
		/// </summary>
		public static implicit operator Identifier(Uri identifier) {
			return new UriIdentifier(identifier);
		}
		/// <summary>
		/// Converts an Identifier to its string representation.
		/// </summary>
		public static implicit operator String(Identifier identifier) {
			return identifier.ToString();
		}
		/// <summary>
		/// Parses an identifier string and automatically determines
		/// whether it is an XRI or URI.
		/// </summary>
		/// <param name="identifier">Either a URI or XRI identifier.</param>
		public static Identifier Parse(string identifier) {
			if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException("identifier");
			if (XriIdentifier.IsValidXri(identifier)) {
				return new XriIdentifier(identifier);
			} else {
				return new UriIdentifier(identifier);
			}
		}

		public static bool IsValid(string identifier) {
			return XriIdentifier.IsValidXri(identifier) || UriIdentifier.IsValidUri(identifier);
		}
	}
}
