//-----------------------------------------------------------------------
// <copyright file="UriIdentifier.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Security;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.UI.HtmlControls;
	using System.Xml;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;
	using Validation;

	/// <summary>
	/// A URI style of OpenID Identifier.
	/// </summary>
	[Serializable]
	[Pure]
	public sealed class UriIdentifier : Identifier {
		/// <summary>
		/// The allowed protocol schemes in a URI Identifier.
		/// </summary>
		private static readonly string[] allowedSchemes = { "http", "https" };

		/// <summary>
		/// The special scheme to use for HTTP URLs that should not have their paths compressed.
		/// </summary>
		private static NonPathCompressingUriParser roundTrippingHttpParser = new NonPathCompressingUriParser(Uri.UriSchemeHttp);

		/// <summary>
		/// The special scheme to use for HTTPS URLs that should not have their paths compressed.
		/// </summary>
		private static NonPathCompressingUriParser roundTrippingHttpsParser = new NonPathCompressingUriParser(Uri.UriSchemeHttps);

		/// <summary>
		/// The special scheme to use for HTTP URLs that should not have their paths compressed.
		/// </summary>
		private static NonPathCompressingUriParser publishableHttpParser = new NonPathCompressingUriParser(Uri.UriSchemeHttp);

		/// <summary>
		/// The special scheme to use for HTTPS URLs that should not have their paths compressed.
		/// </summary>
		private static NonPathCompressingUriParser publishableHttpsParser = new NonPathCompressingUriParser(Uri.UriSchemeHttps);

		/// <summary>
		/// A value indicating whether scheme substitution is being used to workaround
		/// .NET path compression that invalidates some OpenIDs that have trailing periods
		/// in one of their path segments.
		/// </summary>
		private static bool schemeSubstitution;

		/// <summary>
		/// Initializes static members of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <remarks>
		/// This method attempts to workaround the .NET Uri class parsing bug described here:
		/// https://connect.microsoft.com/VisualStudio/feedback/details/386695/system-uri-incorrectly-strips-trailing-dots?wa=wsignin1.0#tabs
		/// since some identifiers (like some of the pseudonymous identifiers from Yahoo) include path segments
		/// that end with periods, which the Uri class will typically trim off.
		/// </remarks>
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Some things just can't be done in a field initializer.")]
		static UriIdentifier() {
			if (Type.GetType("Mono.Runtime") != null) {
				// Uri scheme registration doesn't work on mono.
				return;
			}

			// Our first attempt to handle trailing periods in path segments is to leverage
			// full trust if it's available to rewrite the rules.
			// In fact this is the ONLY way in .NET 3.5 (and arguably in .NET 4.0) to send
			// outbound HTTP requests with trailing periods, so it's the only way to perform
			// discovery on such an identifier.
			try {
				UriParser.Register(roundTrippingHttpParser, "dnoarthttp", 80);
				UriParser.Register(roundTrippingHttpsParser, "dnoarthttps", 443);
				UriParser.Register(publishableHttpParser, "dnoahttp", 80);
				UriParser.Register(publishableHttpsParser, "dnoahttps", 443);
				roundTrippingHttpParser.Initialize(false);
				roundTrippingHttpsParser.Initialize(false);
				publishableHttpParser.Initialize(true);
				publishableHttpsParser.Initialize(true);
				schemeSubstitution = true;
				Logger.OpenId.Debug(".NET Uri class path compression overridden.");
				Reporting.RecordFeatureUse("FullTrust");
			} catch (SecurityException) {
				// We must be running in partial trust.  Nothing more we can do.
				Logger.OpenId.Warn("Unable to coerce .NET to stop compressing URI paths due to partial trust limitations.  Some URL identifiers may be unable to complete login.");
				Reporting.RecordFeatureUse("PartialTrust");
			} catch (FieldAccessException) { // one customer reported getting this exception
				// We must be running in partial trust.  Nothing more we can do.
				Logger.OpenId.Warn("Unable to coerce .NET to stop compressing URI paths due to partial trust limitations.  Some URL identifiers may be unable to complete login.");
				Reporting.RecordFeatureUse("PartialTrust");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		internal UriIdentifier(string uri)
			: this(uri, false) {
			Requires.NotNullOrEmpty(uri, "uri");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		/// <param name="requireSslDiscovery">if set to <c>true</c> [require SSL discovery].</param>
		internal UriIdentifier(string uri, bool requireSslDiscovery)
			: base(uri, requireSslDiscovery) {
			Requires.NotNullOrEmpty(uri, "uri");
			Uri canonicalUri;
			bool schemePrepended;
			if (!TryCanonicalize(uri, out canonicalUri, requireSslDiscovery, out schemePrepended)) {
				throw new UriFormatException();
			}
			if (requireSslDiscovery && canonicalUri.Scheme != Uri.UriSchemeHttps) {
				throw new ArgumentException(OpenIdStrings.ExplicitHttpUriSuppliedWithSslRequirement);
			}
			this.Uri = canonicalUri;
			this.SchemeImplicitlyPrepended = schemePrepended;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		internal UriIdentifier(Uri uri)
			: this(uri, false) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		/// <param name="requireSslDiscovery">if set to <c>true</c> [require SSL discovery].</param>
		internal UriIdentifier(Uri uri, bool requireSslDiscovery)
			: base(uri != null ? uri.OriginalString : null, requireSslDiscovery) {
			Requires.NotNull(uri, "uri");

			string uriAsString = uri.OriginalString;
			if (schemeSubstitution) {
				uriAsString = NormalSchemeToSpecialRoundTrippingScheme(uriAsString);
			}

			if (!TryCanonicalize(uriAsString, out uri)) {
				throw new UriFormatException();
			}
			if (requireSslDiscovery && uri.Scheme != Uri.UriSchemeHttps) {
				throw new ArgumentException(OpenIdStrings.ExplicitHttpUriSuppliedWithSslRequirement);
			}
			this.Uri = uri;
			this.SchemeImplicitlyPrepended = false;
		}

		/// <summary>
		/// Gets or sets a value indicating whether scheme substitution is being used to workaround
		/// .NET path compression that invalidates some OpenIDs that have trailing periods
		/// in one of their path segments.
		/// </summary>
		internal static bool SchemeSubstitutionTestHook {
			get { return schemeSubstitution; }
			set { schemeSubstitution = value; }
		}

		/// <summary>
		/// Gets the URI this instance represents.
		/// </summary>
		internal Uri Uri { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the scheme was missing when this 
		/// Identifier was created and added automatically as part of the 
		/// normalization process.
		/// </summary>
		internal bool SchemeImplicitlyPrepended { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this Identifier has characters or patterns that
		/// the <see cref="Uri"/> class normalizes away and invalidating the Identifier.
		/// </summary>
		internal bool ProblematicNormalization {
			get {
				if (schemeSubstitution) {
					// With full trust, we have no problematic URIs
					return false;
				}

				var simpleUri = new SimpleUri(this.OriginalString);
				if (simpleUri.Path.EndsWith(".", StringComparison.Ordinal) || simpleUri.Path.Contains("./")) {
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Converts a <see cref="UriIdentifier"/> instance to a <see cref="Uri"/> instance.
		/// </summary>
		/// <param name="identifier">The identifier to convert to an ordinary <see cref="Uri"/> instance.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Uri(UriIdentifier identifier) {
			if (identifier == null) {
				return null;
			}
			return identifier.Uri;
		}

		/// <summary>
		/// Converts a <see cref="Uri"/> instance to a <see cref="UriIdentifier"/> instance.
		/// </summary>
		/// <param name="identifier">The <see cref="Uri"/> instance to turn into a <see cref="UriIdentifier"/>.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator UriIdentifier(Uri identifier) {
			if (identifier == null) {
				return null;
			}
			return new UriIdentifier(identifier);
		}

		/// <summary>
		/// Tests equality between this URI and another URI.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			UriIdentifier other = obj as UriIdentifier;
			if (obj != null && other == null && Identifier.EqualityOnStrings) { // test hook to enable MockIdentifier comparison
				other = Identifier.Parse(obj.ToString()) as UriIdentifier;
			}
			if (other == null) {
				return false;
			}

			if (this.ProblematicNormalization || other.ProblematicNormalization) {
				return new SimpleUri(this.OriginalString).Equals(new SimpleUri(other.OriginalString));
			} else {
				return this.Uri == other.Uri;
			}
		}

		/// <summary>
		/// Returns the hash code of this XRI.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return Uri.GetHashCode();
		}

		/// <summary>
		/// Returns the string form of the URI.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			if (this.ProblematicNormalization) {
				return new SimpleUri(this.OriginalString).ToString();
			} else {
				return this.Uri.AbsoluteUri;
			}
		}

		/// <summary>
		/// Determines whether a URI is a valid OpenID Identifier (of any kind).
		/// </summary>
		/// <param name="uri">The URI to test for OpenID validity.</param>
		/// <returns>
		/// 	<c>true</c> if the identifier is valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// A valid URI is absolute (not relative) and uses an http(s) scheme.
		/// </remarks>
		internal static bool IsValidUri(string uri) {
			Uri normalized;
			bool schemePrepended;
			return TryCanonicalize(uri, out normalized, false, out schemePrepended);
		}

		/// <summary>
		/// Determines whether a URI is a valid OpenID Identifier (of any kind).
		/// </summary>
		/// <param name="uri">The URI to test for OpenID validity.</param>
		/// <returns>
		/// 	<c>true</c> if the identifier is valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// A valid URI is absolute (not relative) and uses an http(s) scheme.
		/// </remarks>
		internal static bool IsValidUri(Uri uri) {
			if (uri == null) {
				return false;
			}
			if (!uri.IsAbsoluteUri) {
				return false;
			}
			if (!IsAllowedScheme(uri)) {
				return false;
			}
			return true;
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
		internal override Identifier TrimFragment() {
			// If there is no fragment, we have no need to rebuild the Identifier.
			if (Uri.Fragment == null || Uri.Fragment.Length == 0) {
				return this;
			}

			// Strip the fragment.
			return new UriIdentifier(this.OriginalString.Substring(0, this.OriginalString.IndexOf('#')));
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
			// If this Identifier is already secure, reuse it.
			if (IsDiscoverySecureEndToEnd) {
				secureIdentifier = this;
				return true;
			}

			// If this identifier already uses SSL for initial discovery, return one
			// that guarantees it will be used throughout the discovery process.
			if (string.Equals(Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
				secureIdentifier = new UriIdentifier(this.Uri, true);
				return true;
			}

			// Otherwise, try to make this Identifier secure by normalizing to HTTPS instead of HTTP.
			if (this.SchemeImplicitlyPrepended) {
				UriBuilder newIdentifierUri = new UriBuilder(this.Uri);
				newIdentifierUri.Scheme = Uri.UriSchemeHttps;
				if (newIdentifierUri.Port == 80) {
					newIdentifierUri.Port = 443;
				}
				secureIdentifier = new UriIdentifier(newIdentifierUri.Uri, true);
				return true;
			}

			// This identifier is explicitly NOT https, so we cannot change it.
			secureIdentifier = new NoDiscoveryIdentifier(this, true);
			return false;
		}

		/// <summary>
		/// Determines whether the given URI is using a scheme in the list of allowed schemes.
		/// </summary>
		/// <param name="uri">The URI whose scheme is to be checked.</param>
		/// <returns>
		/// 	<c>true</c> if the scheme is allowed; otherwise, <c>false</c>.
		/// 	<c>false</c> is also returned if <paramref name="uri"/> is null.
		/// </returns>
		private static bool IsAllowedScheme(string uri) {
			if (string.IsNullOrEmpty(uri)) {
				return false;
			}
			return Array.FindIndex(
				allowedSchemes,
				s => uri.StartsWith(s + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase)) >= 0;
		}

		/// <summary>
		/// Determines whether the given URI is using a scheme in the list of allowed schemes.
		/// </summary>
		/// <param name="uri">The URI whose scheme is to be checked.</param>
		/// <returns>
		/// 	<c>true</c> if the scheme is allowed; otherwise, <c>false</c>.
		/// 	<c>false</c> is also returned if <paramref name="uri"/> is null.
		/// </returns>
		private static bool IsAllowedScheme(Uri uri) {
			if (uri == null) {
				return false;
			}
			return Array.FindIndex(
				allowedSchemes,
				s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase)) >= 0;
		}

		/// <summary>
		/// Tries to canonicalize a user-supplied identifier.
		/// This does NOT convert a user-supplied identifier to a Claimed Identifier!
		/// </summary>
		/// <param name="uri">The user-supplied identifier.</param>
		/// <param name="canonicalUri">The resulting canonical URI.</param>
		/// <param name="forceHttpsDefaultScheme">If set to <c>true</c> and the user-supplied identifier lacks a scheme, the "https://" scheme will be prepended instead of the standard "http://" one.</param>
		/// <param name="schemePrepended">if set to <c>true</c> [scheme prepended].</param>
		/// <returns>
		/// <c>true</c> if the identifier was valid and could be canonicalized.
		/// <c>false</c> if the identifier is outside the scope of allowed inputs and should be rejected.
		/// </returns>
		/// <remarks>
		/// Canonicalization is done by adding a scheme in front of an
		/// identifier if it isn't already present.  Other trivial changes that do not
		/// require network access are also done, such as lower-casing the hostname in the URI.
		/// </remarks>
		private static bool TryCanonicalize(string uri, out Uri canonicalUri, bool forceHttpsDefaultScheme, out bool schemePrepended) {
			Requires.NotNullOrEmpty(uri, "uri");

			canonicalUri = null;
			try {
				uri = DoSimpleCanonicalize(uri, forceHttpsDefaultScheme, out schemePrepended);
				if (schemeSubstitution) {
					uri = NormalSchemeToSpecialRoundTrippingScheme(uri);
				}

				// Use a UriBuilder because it helps to normalize the URL as well.
				return TryCanonicalize(uri, out canonicalUri);
			} catch (UriFormatException) {
				// We try not to land here with checks in the try block, but just in case.
				schemePrepended = false;
				return false;
			}
		}

		/// <summary>
		/// Fixes up the scheme if appropriate.
		/// </summary>
		/// <param name="uri">The URI, already in legal form (with http(s):// prepended if necessary).</param>
		/// <param name="canonicalUri">The resulting canonical URI.</param>
		/// <returns><c>true</c> if the canonicalization was successful; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// This does NOT standardize an OpenID URL for storage in a database, as
		/// it does nothing to convert the URL to a Claimed Identifier, besides the fact
		/// that it only deals with URLs whereas OpenID 2.0 supports XRIs.
		/// For this, you should lookup the value stored in IAuthenticationResponse.ClaimedIdentifier.
		/// </remarks>
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The user will see the result of this operation and they want to see it in lower case.")]
		private static bool TryCanonicalize(string uri, out Uri canonicalUri) {
			Requires.NotNull(uri, "uri");

			if (schemeSubstitution) {
				UriBuilder uriBuilder = new UriBuilder(uri);

				// Swap out our round-trippable scheme for the publishable (hidden) scheme.
				uriBuilder.Scheme = uriBuilder.Scheme == roundTrippingHttpParser.RegisteredScheme ? publishableHttpParser.RegisteredScheme : publishableHttpsParser.RegisteredScheme;
				canonicalUri = uriBuilder.Uri;
			} else {
				canonicalUri = new Uri(uri);
			}

			return true;
		}

		/// <summary>
		/// Gets the special non-compressing scheme or URL for a standard scheme or URL.
		/// </summary>
		/// <param name="normal">The ordinary URL or scheme name.</param>
		/// <returns>The non-compressing equivalent scheme or URL for the given value.</returns>
		private static string NormalSchemeToSpecialRoundTrippingScheme(string normal) {
			Requires.NotNullOrEmpty(normal, "normal");
			ErrorUtilities.VerifyInternal(schemeSubstitution, "Wrong schemeSubstitution value.");

			int delimiterIndex = normal.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
			string normalScheme = delimiterIndex < 0 ? normal : normal.Substring(0, delimiterIndex);
			string nonCompressingScheme;
			if (string.Equals(normalScheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(normalScheme, publishableHttpParser.RegisteredScheme, StringComparison.OrdinalIgnoreCase)) {
				nonCompressingScheme = roundTrippingHttpParser.RegisteredScheme;
			} else if (string.Equals(normalScheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(normalScheme, publishableHttpsParser.RegisteredScheme, StringComparison.OrdinalIgnoreCase)) {
				nonCompressingScheme = roundTrippingHttpsParser.RegisteredScheme;
			} else {
				throw new NotSupportedException();
			}

			return delimiterIndex < 0 ? nonCompressingScheme : nonCompressingScheme + normal.Substring(delimiterIndex);
		}

		/// <summary>
		/// Performs the minimal URL normalization to allow a string to be passed to the <see cref="Uri"/> constructor.
		/// </summary>
		/// <param name="uri">The user-supplied identifier URI to normalize.</param>
		/// <param name="forceHttpsDefaultScheme">if set to <c>true</c>, a missing scheme should result in HTTPS being prepended instead of HTTP.</param>
		/// <param name="schemePrepended">if set to <c>true</c>, the scheme was prepended during normalization.</param>
		/// <returns>The somewhat normalized URL.</returns>
		private static string DoSimpleCanonicalize(string uri, bool forceHttpsDefaultScheme, out bool schemePrepended) {
			Requires.NotNullOrEmpty(uri, "uri");

			schemePrepended = false;
			uri = uri.Trim();

			// Assume http:// scheme if an allowed scheme isn't given, and strip
			// fragments off.  Consistent with spec section 7.2#3
			if (!IsAllowedScheme(uri)) {
				uri = (forceHttpsDefaultScheme ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) +
					Uri.SchemeDelimiter + uri;
				schemePrepended = true;
			}

			return uri;
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Uri != null);
			Contract.Invariant(this.Uri.AbsoluteUri != null);
		}
#endif

		/// <summary>
		/// A simple URI class that doesn't suffer from the parsing problems of the <see cref="Uri"/> class.
		/// </summary>
		internal class SimpleUri {
			/// <summary>
			/// URI characters that separate the URI Path from subsequent elements.
			/// </summary>
			private static readonly char[] PathEndingCharacters = new char[] { '?', '#' };

			/// <summary>
			/// Initializes a new instance of the <see cref="SimpleUri"/> class.
			/// </summary>
			/// <param name="value">The value.</param>
			internal SimpleUri(string value) {
				Requires.NotNullOrEmpty(value, "value");

				bool schemePrepended;
				value = DoSimpleCanonicalize(value, false, out schemePrepended);

				// Leverage the Uri class's parsing where we can.
				Uri uri = new Uri(value);
				this.Scheme = uri.Scheme;
				this.Authority = uri.Authority;
				this.Query = uri.Query;
				this.Fragment = uri.Fragment;

				// Get the Path out ourselves, since the default Uri parser compresses it too much for OpenID.
				int schemeLength = value.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
				Assumes.True(schemeLength > 0);
				int hostStart = schemeLength + Uri.SchemeDelimiter.Length;
				int hostFinish = value.IndexOf('/', hostStart);
				if (hostFinish < 0) {
					this.Path = "/";
				} else {
					int pathFinish = value.IndexOfAny(PathEndingCharacters, hostFinish);
					Assumes.True(pathFinish >= hostFinish || pathFinish < 0);
					if (pathFinish < 0) {
						this.Path = value.Substring(hostFinish);
					} else {
						this.Path = value.Substring(hostFinish, pathFinish - hostFinish);
					}
				}

				this.Path = NormalizePathEscaping(this.Path);
			}

			/// <summary>
			/// Gets the scheme.
			/// </summary>
			/// <value>The scheme.</value>
			public string Scheme { get; private set; }

			/// <summary>
			/// Gets the authority.
			/// </summary>
			/// <value>The authority.</value>
			public string Authority { get; private set; }

			/// <summary>
			/// Gets the path of the URI.
			/// </summary>
			/// <value>The path from the URI.</value>
			public string Path { get; private set; }

			/// <summary>
			/// Gets the query.
			/// </summary>
			/// <value>The query.</value>
			public string Query { get; private set; }

			/// <summary>
			/// Gets the fragment.
			/// </summary>
			/// <value>The fragment.</value>
			public string Fragment { get; private set; }

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents this instance.
			/// </returns>
			public override string ToString() {
				return this.Scheme + Uri.SchemeDelimiter + this.Authority + this.Path + this.Query + this.Fragment;
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			/// <exception cref="T:System.NullReferenceException">
			/// The <paramref name="obj"/> parameter is null.
			/// </exception>
			public override bool Equals(object obj) {
				SimpleUri other = obj as SimpleUri;
				if (other == null) {
					return false;
				}

				// Note that this equality check is intentionally leaving off the Fragment part
				// to match Uri behavior, and is intentionally being case sensitive and insensitive
				// for different parts.
				return string.Equals(this.Scheme, other.Scheme, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(this.Authority, other.Authority, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(this.Path, other.Path, StringComparison.Ordinal) &&
					string.Equals(this.Query, other.Query, StringComparison.Ordinal);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode() {
				int hashCode = 0;
				hashCode += StringComparer.OrdinalIgnoreCase.GetHashCode(this.Scheme);
				hashCode += StringComparer.OrdinalIgnoreCase.GetHashCode(this.Authority);
				hashCode += StringComparer.Ordinal.GetHashCode(this.Path);
				hashCode += StringComparer.Ordinal.GetHashCode(this.Query);
				return hashCode;
			}

			/// <summary>
			/// Normalizes the characters that are escaped in the given URI path.
			/// </summary>
			/// <param name="path">The path to normalize.</param>
			/// <returns>The given path, with exactly those characters escaped which should be.</returns>
			private static string NormalizePathEscaping(string path) {
				Requires.NotNull(path, "path");

				string[] segments = path.Split('/');
				for (int i = 0; i < segments.Length; i++) {
					segments[i] = Uri.EscapeDataString(Uri.UnescapeDataString(segments[i]));
				}

				return string.Join("/", segments);
			}
		}

		/// <summary>
		/// A URI parser that does not compress paths, such as trimming trailing periods from path segments.
		/// </summary>
		private class NonPathCompressingUriParser : GenericUriParser {
			/// <summary>
			/// The field that stores the scheme that this parser is registered under.
			/// </summary>
			private static FieldInfo schemeField;

			/// <summary>
			/// The standard "http" or "https" scheme that this parser is subverting.
			/// </summary>
			private string standardScheme;

			/// <summary>
			/// Initializes a new instance of the <see cref="NonPathCompressingUriParser"/> class.
			/// </summary>
			/// <param name="standardScheme">The standard scheme that this parser will be subverting.</param>
			public NonPathCompressingUriParser(string standardScheme)
				: base(GenericUriParserOptions.DontCompressPath | GenericUriParserOptions.IriParsing | GenericUriParserOptions.Idn) {
				Requires.NotNullOrEmpty(standardScheme, "standardScheme");
				this.standardScheme = standardScheme;
			}

			/// <summary>
			/// Gets the scheme this parser is registered under.
			/// </summary>
			/// <value>The registered scheme.</value>
			internal string RegisteredScheme { get; private set; }

			/// <summary>
			/// Initializes this parser with the actual scheme it should appear to be.
			/// </summary>
			/// <param name="hideNonStandardScheme">if set to <c>true</c> Uris using this scheme will look like they're using the original standard scheme.</param>
			[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Schemes are traditionally displayed in lowercase.")]
			internal void Initialize(bool hideNonStandardScheme) {
				if (schemeField == null) {
					schemeField =
						typeof(UriParser).GetField("m_Scheme", BindingFlags.NonPublic | BindingFlags.Instance) ?? // .NET
						typeof(UriParser).GetField("scheme_name", BindingFlags.NonPublic | BindingFlags.Instance); // Mono
					ErrorUtilities.VerifyInternal(schemeField != null, "Unable to find the private field UriParser.m_Scheme");
				}

				this.RegisteredScheme = (string)schemeField.GetValue(this);

				if (hideNonStandardScheme) {
					schemeField.SetValue(this, this.standardScheme.ToLowerInvariant());
				}
			}
		}
	}
}
