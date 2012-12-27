//-----------------------------------------------------------------------
// <copyright file="XriIdentifier.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Xml;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;
	using Validation;

	/// <summary>
	/// An XRI style of OpenID Identifier.
	/// </summary>
	[Serializable]
	[Pure]
	public sealed class XriIdentifier : Identifier {
		/// <summary>
		/// An XRI always starts with one of these symbols.
		/// </summary>
		internal static readonly char[] GlobalContextSymbols = { '=', '@', '+', '$', '!' };

		/// <summary>
		/// The scheme and separator "xri://"
		/// </summary>
		private const string XriScheme = "xri://";

		/// <summary>
		/// Backing store for the <see cref="CanonicalXri"/> property.
		/// </summary>
		private readonly string canonicalXri;

		/// <summary>
		/// Initializes a new instance of the <see cref="XriIdentifier"/> class.
		/// </summary>
		/// <param name="xri">The string value of the XRI.</param>
		internal XriIdentifier(string xri)
			: this(xri, false) {
			Requires.NotNullOrEmpty(xri, "xri");
			RequiresEx.Format(IsValidXri(xri), OpenIdStrings.InvalidXri);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XriIdentifier"/> class.
		/// </summary>
		/// <param name="xri">The XRI that this Identifier will represent.</param>
		/// <param name="requireSsl">
		/// If set to <c>true</c>, discovery and the initial authentication redirect will
		/// only succeed if it can be done entirely using SSL.
		/// </param>
		internal XriIdentifier(string xri, bool requireSsl)
			: base(xri, requireSsl) {
			Requires.NotNullOrEmpty(xri, "xri");
			RequiresEx.Format(IsValidXri(xri), OpenIdStrings.InvalidXri);
			Assumes.True(xri != null); // Proven by IsValidXri
			this.OriginalXri = xri;
			this.canonicalXri = CanonicalizeXri(xri);
		}

		/// <summary>
		/// Gets the original XRI supplied to the constructor.
		/// </summary>
		internal string OriginalXri { get; private set; }

		/// <summary>
		/// Gets the canonical form of the XRI string.
		/// </summary>
		internal string CanonicalXri {
			get {
				return this.canonicalXri;
			}
		}

		/// <summary>
		/// Tests equality between this XRI and another XRI.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			XriIdentifier other = obj as XriIdentifier;
			if (obj != null && other == null && Identifier.EqualityOnStrings) { // test hook to enable MockIdentifier comparison
				string objString = obj.ToString();
				ErrorUtilities.VerifyInternal(!string.IsNullOrEmpty(objString), "Identifier.ToString() returned a null or empty string.");
				other = Identifier.Parse(objString) as XriIdentifier;
			}
			if (other == null) {
				return false;
			}
			return this.CanonicalXri == other.CanonicalXri;
		}

		/// <summary>
		/// Returns the hash code of this XRI.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return this.CanonicalXri.GetHashCode();
		}

		/// <summary>
		/// Returns the canonical string form of the XRI.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			return this.CanonicalXri;
		}

		/// <summary>
		/// Tests whether a given string represents a valid XRI format.
		/// </summary>
		/// <param name="xri">The value to test for XRI validity.</param>
		/// <returns>
		/// 	<c>true</c> if the given string constitutes a valid XRI; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsValidXri(string xri) {
			Requires.NotNullOrEmpty(xri, "xri");
			xri = xri.Trim();

			// TODO: better validation code here
			return xri.IndexOfAny(GlobalContextSymbols) == 0
				|| xri.StartsWith("(", StringComparison.Ordinal)
				|| xri.StartsWith(XriScheme, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Returns an <see cref="Identifier"/> that has no URI fragment.
		/// Quietly returns the original <see cref="Identifier"/> if it is not
		/// a <see cref="UriIdentifier"/> or no fragment exists.
		/// </summary>
		/// <returns>
		/// A new <see cref="Identifier"/> instance if there was a
		/// fragment to remove, otherwise this same instance..
		/// </returns>
		/// <remarks>
		/// XRI Identifiers never have a fragment part, and thus this method
		/// always returns this same instance.
		/// </remarks>
		internal override Identifier TrimFragment() {
			return this;
		}

		/// <summary>
		/// Converts a given identifier to its secure equivalent.
		/// UriIdentifiers originally created with an implied HTTP scheme change to HTTPS.
		/// Discovery is made to require SSL for the entire resolution process.
		/// </summary>
		/// <param name="secureIdentifier">The newly created secure identifier.
		/// If the conversion fails, <paramref name="secureIdentifier"/> retains
		/// <i>this</i> identifiers identity, but will never discover any endpoints.</param>
		/// <returns>
		/// True if the secure conversion was successful.
		/// False if the Identifier was originally created with an explicit HTTP scheme.
		/// </returns>
		internal override bool TryRequireSsl(out Identifier secureIdentifier) {
			secureIdentifier = IsDiscoverySecureEndToEnd ? this : new XriIdentifier(this, true);
			return true;
		}

		/// <summary>
		/// Takes any valid form of XRI string and returns the canonical form of the same XRI.
		/// </summary>
		/// <param name="xri">The xri to canonicalize.</param>
		/// <returns>The canonicalized form of the XRI.</returns>
		/// <remarks>The canonical form, per the OpenID spec, is no scheme and no whitespace on either end.</remarks>
		private static string CanonicalizeXri(string xri) {
			Requires.NotNull(xri, "xri");
			xri = xri.Trim();
			if (xri.StartsWith(XriScheme, StringComparison.OrdinalIgnoreCase)) {
				Assumes.True(XriScheme.Length <= xri.Length); // should be implied by StartsWith
				xri = xri.Substring(XriScheme.Length);
			}
			return xri;
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.canonicalXri != null);
		}
#endif
	}
}
