//-----------------------------------------------------------------------
// <copyright file="Identifier.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An Identifier is either a "http" or "https" URI, or an XRI.
	/// </summary>
	[Serializable]
	public abstract class Identifier {
		/// <summary>
		/// Initializes a new instance of the <see cref="Identifier"/> class.
		/// </summary>
		/// <param name="isDiscoverySecureEndToEnd">
		/// Whether the derived class is prepared to guarantee end-to-end discovery
		/// and initial redirect for authentication is performed using SSL.
		/// </param>
		protected Identifier(bool isDiscoverySecureEndToEnd) {
			this.IsDiscoverySecureEndToEnd = isDiscoverySecureEndToEnd;
		}

		/// <summary>
		/// Gets a value indicating whether this Identifier will ensure SSL is 
		/// used throughout the discovery phase and initial redirect of authentication.
		/// </summary>
		/// <remarks>
		/// If this is <c>false</c>, a value of <c>true</c> may be obtained by calling 
		/// <see cref="TryRequireSsl"/>.
		/// </remarks>
		protected internal bool IsDiscoverySecureEndToEnd { get; private set; }

		/// <summary>
		/// Converts the string representation of an Identifier to its strong type.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>The particular Identifier instance to represent the value given.</returns>
		[SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "Not all identifiers are URIs.")]
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Our named alternate is Parse.")]
		public static implicit operator Identifier(string identifier) {
			if (identifier == null) {
				return null;
			}
			return Parse(identifier);
		}

		/// <summary>
		/// Converts a given Uri to a strongly-typed Identifier.
		/// </summary>
		/// <param name="identifier">The identifier to convert.</param>
		/// <returns>The result of the conversion.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "We have a Parse function.")]
		public static implicit operator Identifier(Uri identifier) {
			if (identifier == null) {
				return null;
			}
			return new UriIdentifier(identifier);
		}

		/// <summary>
		/// Converts an Identifier to its string representation.
		/// </summary>
		/// <param name="identifier">The identifier to convert to a string.</param>
		/// <returns>The result of the conversion.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "We have a Parse function.")]
		public static implicit operator string(Identifier identifier) {
			if (identifier == null) {
				return null;
			}
			return identifier.ToString();
		}

		/// <summary>
		/// Parses an identifier string and automatically determines
		/// whether it is an XRI or URI.
		/// </summary>
		/// <param name="identifier">Either a URI or XRI identifier.</param>
		/// <returns>An <see cref="Identifier"/> instance for the given value.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Some of these identifiers are not properly formatted to be Uris at this stage.")]
		public static Identifier Parse(string identifier) {
			ErrorUtilities.VerifyArgumentNotNull(identifier, "identifier");
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
		/// Checks the validity of a given string representation of some Identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>
		/// 	<c>true</c> if the specified identifier is valid; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Some of these identifiers are not properly formatted to be Uris at this stage.")]
		public static bool IsValid(string identifier) {
			return XriIdentifier.IsValidXri(identifier) || UriIdentifier.IsValidUri(identifier);
		}

		/// <summary>
		/// Tests equality between two <see cref="Identifier"/>s.
		/// </summary>
		/// <param name="id1">The first Identifier.</param>
		/// <param name="id2">The second Identifier.</param>
		/// <returns>
		/// <c>true</c> if the two instances should be considered equal; <c>false</c> otherwise.
		/// </returns>
		public static bool operator ==(Identifier id1, Identifier id2) {
			return id1.EqualsNullSafe(id2);
		}

		/// <summary>
		/// Tests inequality between two <see cref="Identifier"/>s.
		/// </summary>
		/// <param name="id1">The first Identifier.</param>
		/// <param name="id2">The second Identifier.</param>
		/// <returns>
		/// <c>true</c> if the two instances should be considered unequal; <c>false</c> if they are equal.
		/// </returns>
		public static bool operator !=(Identifier id1, Identifier id2) {
			return !id1.EqualsNullSafe(id2);
		}

		/// <summary>
		/// Tests equality between two <see cref="Identifier"/>s.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			Debug.Fail("This should be overridden in every derived class.");
			return base.Equals(obj);
		}

		/// <summary>
		/// Gets the hash code for an <see cref="Identifier"/> for storage in a hashtable.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			Debug.Fail("This should be overridden in every derived class.");
			return base.GetHashCode();
		}

		/// <summary>
		/// Performs discovery on the Identifier.
		/// </summary>
		/// <param name="requestHandler">The web request handler to use for discovery.</param>
		/// <returns>
		/// An initialized structure containing the discovered provider endpoint information.
		/// </returns>
		internal abstract IEnumerable<ServiceEndpoint> Discover(IDirectWebRequestHandler requestHandler);

		/// <summary>
		/// Returns an <see cref="Identifier"/> that has no URI fragment.
		/// Quietly returns the original <see cref="Identifier"/> if it is not
		/// a <see cref="UriIdentifier"/> or no fragment exists.
		/// </summary>
		/// <returns>A new <see cref="Identifier"/> instance if there was a 
		/// fragment to remove, otherwise this same instance..</returns>
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
