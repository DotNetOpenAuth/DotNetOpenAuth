//-----------------------------------------------------------------------
// <copyright file="UriIdentifier.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.UI.HtmlControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// A URI style of OpenID Identifier.
	/// </summary>
	[Serializable]
	public sealed class UriIdentifier : Identifier {
		/// <summary>
		/// The allowed protocol schemes in a URI Identifier.
		/// </summary>
		private static readonly string[] allowedSchemes = { "http", "https" };

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		internal UriIdentifier(string uri) : this(uri, false) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		/// <param name="requireSslDiscovery">if set to <c>true</c> [require SSL discovery].</param>
		internal UriIdentifier(string uri, bool requireSslDiscovery)
			: base(requireSslDiscovery) {
			ErrorUtilities.VerifyNonZeroLength(uri, "uri");
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
		internal UriIdentifier(Uri uri) : this(uri, false) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="UriIdentifier"/> class.
		/// </summary>
		/// <param name="uri">The value this identifier will represent.</param>
		/// <param name="requireSslDiscovery">if set to <c>true</c> [require SSL discovery].</param>
		internal UriIdentifier(Uri uri, bool requireSslDiscovery)
			: base(requireSslDiscovery) {
			ErrorUtilities.VerifyArgumentNotNull(uri, "uri");
			if (!TryCanonicalize(new UriBuilder(uri), out uri)) {
				throw new UriFormatException();
			}
			if (requireSslDiscovery && uri.Scheme != Uri.UriSchemeHttps) {
				throw new ArgumentException(OpenIdStrings.ExplicitHttpUriSuppliedWithSslRequirement);
			}
			this.Uri = uri;
			this.SchemeImplicitlyPrepended = false;
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
			if (other == null) {
				return false;
			}
			return this.Uri == other.Uri;
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
			return Uri.AbsoluteUri;
		}
#if UNUSED
		static bool TryCanonicalize(string uri, out string canonicalUri) {
			Uri normalizedUri;
			bool result = TryCanonicalize(uri, out normalizedUri);
			canonicalUri = normalizedUri.AbsoluteUri;
			return result;
		}
#endif
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
		/// Performs discovery on the Identifier.
		/// </summary>
		/// <param name="requestHandler">The web request handler to use for discovery.</param>
		/// <returns>
		/// An initialized structure containing the discovered provider endpoint information.
		/// </returns>
		internal override IEnumerable<ServiceEndpoint> Discover(IDirectWebRequestHandler requestHandler) {
			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();

			// Attempt YADIS discovery
			DiscoveryResult yadisResult = Yadis.Discover(requestHandler, this, IsDiscoverySecureEndToEnd);
			if (yadisResult != null) {
				if (yadisResult.IsXrds) {
					XrdsDocument xrds = new XrdsDocument(yadisResult.ResponseText);
					var xrdsEndpoints = xrds.CreateServiceEndpoints(yadisResult.NormalizedUri, this);

					// Filter out insecure endpoints if high security is required.
					if (IsDiscoverySecureEndToEnd) {
						xrdsEndpoints = xrdsEndpoints.Where(se => se.IsSecure);
					}
					endpoints.AddRange(xrdsEndpoints);
				}

				// Failing YADIS discovery of an XRDS document, we try HTML discovery.
				if (endpoints.Count == 0) {
					var htmlEndpoints = new List<ServiceEndpoint>(DiscoverFromHtml(yadisResult.NormalizedUri, this, yadisResult.ResponseText));
					if (htmlEndpoints.Any()) {
						Logger.DebugFormat("Total services discovered in HTML: {0}", htmlEndpoints.Count);
						Logger.Debug(htmlEndpoints.ToStringDeferred(true));
						endpoints.AddRange(htmlEndpoints.Where(ep => !IsDiscoverySecureEndToEnd || ep.IsSecure));
						if (endpoints.Count == 0) {
							Logger.Info("No HTML discovered endpoints met the security requirements.");
						}
					} else {
						Logger.Debug("HTML discovery failed to find any endpoints.");
					}
				} else {
					Logger.Debug("Skipping HTML discovery because XRDS contained service endpoints.");
				}
			}
			return endpoints;
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
			UriBuilder builder = new UriBuilder(Uri);
			builder.Fragment = null;
			return builder.Uri;
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
			if (String.Equals(Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
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
		/// Searches HTML for the HEAD META tags that describe OpenID provider services.
		/// </summary>
		/// <param name="claimedIdentifier">The final URL that provided this HTML document.
		/// This may not be the same as (this) userSuppliedIdentifier if the
		/// userSuppliedIdentifier pointed to a 301 Redirect.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="html">The HTML that was downloaded and should be searched.</param>
		/// <returns>
		/// A sequence of any discovered ServiceEndpoints.
		/// </returns>
		private static IEnumerable<ServiceEndpoint> DiscoverFromHtml(Uri claimedIdentifier, UriIdentifier userSuppliedIdentifier, string html) {
			var linkTags = new List<HtmlLink>(HtmlParser.HeadTags<HtmlLink>(html));
			foreach (var protocol in Protocol.AllPracticalVersions) {
				// rel attributes are supposed to be interpreted with case INsensitivity, 
				// and is a space-delimited list of values. (http://www.htmlhelp.com/reference/html40/values.html#linktypes)
				var serverLinkTag = linkTags.FirstOrDefault(tag => Regex.IsMatch(tag.Attributes["rel"], @"\b" + Regex.Escape(protocol.HtmlDiscoveryProviderKey) + @"\b", RegexOptions.IgnoreCase));
				if (serverLinkTag == null) {
					continue;
				}

				Uri providerEndpoint = null;
				if (Uri.TryCreate(serverLinkTag.Href, UriKind.Absolute, out providerEndpoint)) {
					// See if a LocalId tag of the discovered version exists
					Identifier providerLocalIdentifier = null;
					var delegateLinkTag = linkTags.FirstOrDefault(tag => Regex.IsMatch(tag.Attributes["rel"], @"\b" + Regex.Escape(protocol.HtmlDiscoveryLocalIdKey) + @"\b", RegexOptions.IgnoreCase));
					if (delegateLinkTag != null) {
						if (Identifier.IsValid(delegateLinkTag.Href)) {
							providerLocalIdentifier = delegateLinkTag.Href;
						} else {
							Logger.WarnFormat("Skipping endpoint data because local id is badly formed ({0}).", delegateLinkTag.Href);
							continue; // skip to next version
						}
					}

					// Choose the TypeURI to match the OpenID version detected.
					string[] typeURIs = { protocol.ClaimedIdentifierServiceTypeURI };
					yield return ServiceEndpoint.CreateForClaimedIdentifier(
						claimedIdentifier,
						providerLocalIdentifier,
						new ProviderEndpointDescription(providerEndpoint, typeURIs),
						(int?)null,
						(int?)null);
				}
			}
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
			ErrorUtilities.VerifyNonZeroLength(uri, "uri");

			uri = uri.Trim();
			canonicalUri = null;
			schemePrepended = false;
			try {
				// Assume http:// scheme if an allowed scheme isn't given, and strip
				// fragments off.  Consistent with spec section 7.2#3
				if (!IsAllowedScheme(uri)) {
					uri = (forceHttpsDefaultScheme ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) +
						Uri.SchemeDelimiter + uri;
					schemePrepended = true;
				}

				// Use a UriBuilder because it helps to normalize the URL as well.
				return TryCanonicalize(new UriBuilder(uri), out canonicalUri);
			} catch (UriFormatException) {
				// We try not to land here with checks in the try block, but just in case.
				return false;
			}
		}

		/// <summary>
		/// Removes the fragment from a URL and sets the host to lowercase.
		/// </summary>
		/// <param name="uriBuilder">The URI builder with the value to canonicalize.</param>
		/// <param name="canonicalUri">The resulting canonical URI.</param>
		/// <returns><c>true</c> if the canonicalization was successful; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// This does NOT standardize an OpenID URL for storage in a database, as
		/// it does nothing to convert the URL to a Claimed Identifier, besides the fact
		/// that it only deals with URLs whereas OpenID 2.0 supports XRIs.
		/// For this, you should lookup the value stored in IAuthenticationResponse.ClaimedIdentifier.
		/// </remarks>
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The user will see the result of this operation and they want to see it in lower case.")]
		private static bool TryCanonicalize(UriBuilder uriBuilder, out Uri canonicalUri) {
			uriBuilder.Host = uriBuilder.Host.ToLowerInvariant();
			canonicalUri = uriBuilder.Uri;
			return true;
		}
	}
}
