using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace DotNetOpenId {
	/// <summary>
	/// An Identifier is either a "http" or "https" URI, or an XRI.
	/// </summary>
	[Serializable]
	public abstract class Identifier {
		/// <summary>
		/// Constructs an <see cref="Identifier"/>.
		/// </summary>
		/// <param name="isDiscoverySecureEndToEnd">
		/// Whether the derived class is prepared to guarantee end-to-end discovery
		/// and initial redirect for authentication is performed using SSL.
		/// </param>
		protected Identifier(bool isDiscoverySecureEndToEnd) {
			IsDiscoverySecureEndToEnd = isDiscoverySecureEndToEnd;
		}

		/// <summary>
		/// Whether this Identifier will ensure SSL is used throughout the discovery phase
		/// and initial redirect of authentication.
		/// </summary>
		/// <remarks>
		/// If this is False, a value of True may be obtained by calling <see cref="TryRequireSsl"/>.
		/// </remarks>
		protected internal bool IsDiscoverySecureEndToEnd { get; private set; }

		/// <summary>
		/// Converts the string representation of an Identifier to its strong type.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates"), SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads")]
		public static implicit operator Identifier(string identifier) {
			if (identifier == null) return null;
			return Parse(identifier);
		}
		/// <summary>
		/// Returns a strongly-typed Identifier for a given Uri.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator Identifier(Uri identifier) {
			if (identifier == null) return null;
			return new UriIdentifier(identifier);
		}
		/// <summary>
		/// Converts an Identifier to its string representation.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator String(Identifier identifier) {
			if (identifier == null) return null;
			return identifier.ToString();
		}
		/// <summary>
		/// Parses an identifier string and automatically determines
		/// whether it is an XRI or URI.
		/// </summary>
		/// <param name="identifier">Either a URI or XRI identifier.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public static Identifier Parse(string identifier) {
			if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException("identifier");
			if (XriIdentifier.IsValidXri(identifier)) {
				return new XriIdentifier(identifier);
			} else {
				return new UriIdentifier(identifier);
			}
		}
		/// <summary>
		/// Attempts to parse a string for an OpenId Identifier.
		/// </summary>
		/// <param name="value">The string to be parsed.</param>
		/// <param name="result">The parsed Identifier form.</param>
		/// <returns>
		/// True if the operation was successful.  False if the string was not a valid OpenId Identifier.
		/// </returns>
		public static bool TryParse(string value, out Identifier result) {
			if (IsValid(value)) {
				result = Parse(value);
				return true;
			} else {
				result = null;
				return false;
			}
		}
		/// <summary>
		/// Gets whether a given string represents a valid Identifier format.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public static bool IsValid(string identifier) {
			return XriIdentifier.IsValidXri(identifier) || UriIdentifier.IsValidUri(identifier);
		}
		/// <summary>
		/// Performs discovery on the Identifier.
		/// </summary>
		/// <returns>
		/// An initialized structure containing the discovered provider endpoint information.
		/// </returns>
		internal abstract IEnumerable<ServiceEndpoint> Discover();

		/// <summary>
		/// Tests equality between two <see cref="Identifier"/>s.
		/// </summary>
		public static bool operator ==(Identifier id1, Identifier id2) {
			if ((object)id1 == null ^ (object)id2 == null) return false;
			if ((object)id1 == null) return true;
			return id1.Equals(id2);
		}
		/// <summary>
		/// Tests inequality between two <see cref="Identifier"/>s.
		/// </summary>
		public static bool operator !=(Identifier id1, Identifier id2) {
			return !(id1 == id2);
		}
		/// <summary>
		/// Tests equality between two <see cref="Identifier"/>s.
		/// </summary>
		public override bool Equals(object obj) {
			Debug.Fail("This should be overridden in every derived class.");
			return base.Equals(obj);
		}
		/// <summary>
		/// Gets the hash code for an <see cref="Identifier"/> for storage in a hashtable.
		/// </summary>
		public override int GetHashCode() {
			Debug.Fail("This should be overridden in every derived class.");
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns an <see cref="Identifier"/> that has no URI fragment.
		/// Quietly returns the original <see cref="Identifier"/> if it is not 
		/// a <see cref="UriIdentifier"/> or no fragment exists.
		/// </summary>
		internal abstract Identifier TrimFragment();

		/// <summary>
		/// Converts a given identifier to its secure equivalent.  
		/// UriIdentifiers originally created with an implied HTTP scheme change to HTTPS.
		/// Discovery is made to require SSL for the entire resolution process.
		/// </summary>
		/// <param name="secureIdentifier">
		/// The newly created secure identifier.
		/// If the conversion fails, <paramref name="secureIdentifier"/> retains
		/// <i>this</i> identifiers identity, but will never discover any endpoints.
		/// </param>
		/// <returns>
		/// True if the secure conversion was successful.
		/// False if the Identifier was originally created with an explicit HTTP scheme.
		/// </returns>
		internal abstract bool TryRequireSsl(out Identifier secureIdentifier);
	}
}
