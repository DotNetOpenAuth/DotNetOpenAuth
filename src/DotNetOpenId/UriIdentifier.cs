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
			canonicalUri = normalizedUri.ToString();
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

		/// <summary>
		/// Downloads an XRDS document describing the services at some Identifier
		/// if it is available.
		/// </summary>
		/// <returns>
		/// An XrdsDocument if one is available, or null if none could be found.
		/// </returns>
		protected virtual XrdsDocument DownloadXrds() {
			throw new NotImplementedException();
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
			Identifier providerLocalIdentifier = null;
			int version = 0;
			foreach (var linkTag in Yadis.HtmlParser.HeadTags<HtmlLink>(html)) {
				switch (linkTag.Attributes["rel"]) {
					case ProtocolConstants.OpenId20Provider:
						providerEndpoint = new Uri(linkTag.Href);
						version = 2;
						break;
					case ProtocolConstants.OpenId20LocalId:
						providerLocalIdentifier = new Uri(linkTag.Href);
						version = 2;
						break;
					case ProtocolConstants.OpenId11Server:
						if (version == 0) { // do not override a 2.0 discovery
							providerEndpoint = new Uri(linkTag.Href);
							version = 1;
						}
						break;
					case ProtocolConstants.OpenId11Delegate:
						if (version <= 1) {
							providerLocalIdentifier = linkTag.Href;
						}
						break;
				}
			}
			if (providerEndpoint == null) {
				return null; // html did not contain openid.server link
			}
			// Choose the TypeURI to match the OpenID version detected.
			string[] typeURIs = { version == 2 ?
				ServiceEndpoint.OpenId20Type : ServiceEndpoint.OpenId11Type };
			return new ServiceEndpoint(claimedIdentifier, providerEndpoint, 
				providerLocalIdentifier, typeURIs);
		}

		internal override ServiceEndpoint Discover() {
			// Attempt YADIS discovery
			DiscoveryResult yadisResult = Yadis.Yadis.Discover(this);
			if (yadisResult != null) {
				if (yadisResult.IsXrds) {
					XrdsDocument xrds = new XrdsDocument(yadisResult.ResponseText);
					ServiceEndpoint ep = xrds.CreateServiceEndpoint(yadisResult.NormalizedUri);
					if (ep != null) return ep;
				}
				// Failing YADIS discovery of an XRDS document, we try HTML discovery.
				return DiscoverFromHtml(yadisResult.NormalizedUri, yadisResult.ResponseText);
			}
			return null;
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
			return Uri.ToString();
		}

	}
}
