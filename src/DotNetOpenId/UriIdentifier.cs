using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Yadis;

namespace DotNetOpenId {
	[Serializable]
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

		public UriIdentifier(string uri) : this(uri, false) { }
		public UriIdentifier(string uri, bool requireSslDiscovery)
			: base(requireSslDiscovery) {
			if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");
			Uri canonicalUri;
			bool schemePrepended;
			if (!TryCanonicalize(uri, out canonicalUri, requireSslDiscovery, out schemePrepended))
				throw new UriFormatException();
			if (requireSslDiscovery && canonicalUri.Scheme != Uri.UriSchemeHttps) {
				throw new ArgumentException(Strings.ExplicitHttpUriSuppliedWithSslRequirement);
			}
			Uri = canonicalUri;
			SchemeImplicitlyPrepended = schemePrepended;
		}
		public UriIdentifier(Uri uri) : this(uri, false) { }
		public UriIdentifier(Uri uri, bool requireSslDiscovery)
			: base(requireSslDiscovery) {
			if (uri == null) throw new ArgumentNullException("uri");
			if (!TryCanonicalize(new UriBuilder(uri), out uri))
				throw new UriFormatException();
			if (requireSslDiscovery && uri.Scheme != Uri.UriSchemeHttps) {
				throw new ArgumentException(Strings.ExplicitHttpUriSuppliedWithSslRequirement);
			}
			Uri = uri;
			SchemeImplicitlyPrepended = false;
		}

		public Uri Uri { get; private set; }
		/// <summary>
		/// Gets whether the scheme was missing when this Identifier was
		/// created and added automatically as part of the normalization
		/// process.
		/// </summary>
		internal bool SchemeImplicitlyPrepended { get; private set; }

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
		static bool TryCanonicalize(string uri, out Uri canonicalUri, bool forceHttpsDefaultScheme, out bool schemePrepended) {
			canonicalUri = null;
			schemePrepended = false;
			try {
				// Assume http:// scheme if an allowed scheme isn't given, and strip
				// fragments off.  Consistent with spec section 7.2#3
				if (!isAllowedScheme(uri)) {
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
#if UNUSED
		static bool TryCanonicalize(string uri, out string canonicalUri) {
			Uri normalizedUri;
			bool result = TryCanonicalize(uri, out normalizedUri);
			canonicalUri = normalizedUri.AbsoluteUri;
			return result;
		}
#endif
		/// <summary>
		/// Removes the fragment from a URL and sets the host to lowercase.
		/// </summary>
		/// <remarks>
		/// This does NOT standardize an OpenID URL for storage in a database, as
		/// it does nothing to convert the URL to a Claimed Identifier, besides the fact
		/// that it only deals with URLs whereas OpenID 2.0 supports XRIs.
		/// For this, you should lookup the value stored in IAuthenticationResponse.ClaimedIdentifier.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		static bool TryCanonicalize(UriBuilder uriBuilder, out Uri canonicalUri) {
			uriBuilder.Host = uriBuilder.Host.ToLowerInvariant();
			canonicalUri = uriBuilder.Uri;
			return true;
		}
		internal static bool IsValidUri(string uri) {
			Uri normalized;
			bool schemePrepended;
			return TryCanonicalize(uri, out normalized, false, out schemePrepended);
		}
		internal static bool IsValidUri(Uri uri) {
			if (uri == null) return false;
			if (!uri.IsAbsoluteUri) return false;
			if (!isAllowedScheme(uri)) return false;
			return true;
		}

		/// <summary>
		/// Searches HTML for the HEAD META tags that describe OpenID provider services.
		/// </summary>
		/// <param name="claimedIdentifier">
		/// The final URL that provided this HTML document.  
		/// This may not be the same as (this) userSuppliedIdentifier if the 
		/// userSuppliedIdentifier pointed to a 301 Redirect.
		/// </param>
		/// <param name="html">The HTML that was downloaded and should be searched.</param>
		/// <returns>
		/// An initialized ServiceEndpoint if the OpenID Provider information was
		/// found.  Otherwise null.
		/// </returns>
		/// <remarks>
		/// OpenID 2.0 tags are always used if they are present, otherwise
		/// OpenID 1.x tags are used if present.
		/// </remarks>
		protected virtual ServiceEndpoint DiscoverFromHtml(Uri claimedIdentifier, string html) {
			Uri providerEndpoint = null;
			Protocol discoveredProtocol = null;
			Identifier providerLocalIdentifier = null;
			var linkTags = new List<HtmlLink>(Yadis.HtmlParser.HeadTags<HtmlLink>(html));
			foreach (var protocol in Protocol.AllVersions) {
				foreach (var linkTag in linkTags) {
					// rel attributes are supposed to be interpreted with case INsensitivity, 
					// and is a space-delimited list of values. (http://www.htmlhelp.com/reference/html40/values.html#linktypes)
					if (Regex.IsMatch(linkTag.Attributes["rel"], @"\b" + Regex.Escape(protocol.HtmlDiscoveryProviderKey) + @"\b", RegexOptions.IgnoreCase)) {
						if (Uri.TryCreate(linkTag.Href, UriKind.Absolute, out providerEndpoint)) {
							discoveredProtocol = protocol;
							break;
						}
					}
				}
				if (providerEndpoint != null) break;
			}
			if (providerEndpoint == null)
				return null; // html did not contain openid.server link
			// See if a LocalId tag of the discovered version exists
			foreach (var linkTag in linkTags) {
				if (Regex.IsMatch(linkTag.Attributes["rel"], @"\b" + Regex.Escape(discoveredProtocol.HtmlDiscoveryLocalIdKey) + @"\b", RegexOptions.IgnoreCase)) {
					if (Identifier.IsValid(linkTag.Href)) {
						providerLocalIdentifier = linkTag.Href;
						break;
					} else {
						Logger.WarnFormat("Skipping endpoint data because local id is badly formed ({0}).", linkTag.Href);
						return null; // badly formed URL used as LocalId
					}
				}
			}

			// Choose the TypeURI to match the OpenID version detected.
			string[] typeURIs = { discoveredProtocol.ClaimedIdentifierServiceTypeURI };
			return ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, providerLocalIdentifier,
				providerEndpoint, typeURIs, (int?)null, (int?)null);
		}

		internal override IEnumerable<ServiceEndpoint> Discover() {
			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();
			// Attempt YADIS discovery
			DiscoveryResult yadisResult = Yadis.Yadis.Discover(this, IsDiscoverySecureEndToEnd);
			if (yadisResult != null) {
				if (yadisResult.IsXrds) {
					XrdsDocument xrds = new XrdsDocument(yadisResult.ResponseText);
					var xrdsEndpoints = xrds.CreateServiceEndpoints(yadisResult.NormalizedUri);
					// Filter out insecure endpoints if high security is required.
					if (IsDiscoverySecureEndToEnd) {
						xrdsEndpoints = Util.Where(xrdsEndpoints, se => se.IsSecure);
					}
					endpoints.AddRange(xrdsEndpoints);
				}
				// Failing YADIS discovery of an XRDS document, we try HTML discovery.
				if (endpoints.Count == 0) {
					ServiceEndpoint ep = DiscoverFromHtml(yadisResult.NormalizedUri, yadisResult.ResponseText);
					if (ep != null && (!IsDiscoverySecureEndToEnd || ep.IsSecure)) {
						endpoints.Add(ep);
					}
				}
			}
			return endpoints;
		}

		internal override Identifier TrimFragment() {
			// If there is no fragment, we have no need to rebuild the Identifier.
			if (Uri.Fragment == null || Uri.Fragment.Length == 0)
				return this;

			// Strip the fragment.
			UriBuilder builder = new UriBuilder(Uri);
			builder.Fragment = null;
			return builder.Uri;
		}

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
			if (SchemeImplicitlyPrepended) {
				UriBuilder newIdentifierUri = new UriBuilder(this.Uri);
				newIdentifierUri.Scheme = Uri.UriSchemeHttps;
				if (newIdentifierUri.Port == 80) {
					newIdentifierUri.Port = 443;
				}
				secureIdentifier = new UriIdentifier(newIdentifierUri.Uri, true);
				return true;
			}

			// This identifier is explicitly NOT https, so we cannot change it.
			secureIdentifier = new NoDiscoveryIdentifier(this);
			return false;
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
