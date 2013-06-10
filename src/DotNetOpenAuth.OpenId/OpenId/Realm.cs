//-----------------------------------------------------------------------
// <copyright file="Realm.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;
	using Validation;

	/// <summary>
	/// A trust root to validate requests and match return URLs against.
	/// </summary>
	/// <remarks>
	/// This fills the OpenID Authentication 2.0 specification for realms.
	/// See http://openid.net/specs/openid-authentication-2_0.html#realms
	/// </remarks>
	[Serializable]
	[Pure]
	[DefaultEncoder(typeof(MessagePartRealmConverter))]
	public class Realm {
		/// <summary>
		/// A regex used to detect a wildcard that is being used in the realm.
		/// </summary>
		private const string WildcardDetectionPattern = @"^(\w+://)\*\.";

		/// <summary>
		/// A (more or less) comprehensive list of top-level (i.e. ".com") domains,
		/// for use by <see cref="IsSane"/> in order to disallow overly-broad realms
		/// that allow all web sites ending with '.com', for example.
		/// </summary>
		private static readonly string[] topLevelDomains = { "com", "edu", "gov", "int", "mil", "net", "org", "biz", "info", "name", "museum", "coop", "aero", "ac", "ad", "ae",
			"af", "ag", "ai", "al", "am", "an", "ao", "aq", "ar", "as", "at", "au", "aw", "az", "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj",
			"bm", "bn", "bo", "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci", "ck", "cl", "cm", "cn", "co", "cr",
			"cu", "cv", "cx", "cy", "cz", "de", "dj", "dk", "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "fi", "fj", "fk", "fm", "fo",
			"fr", "ga", "gd", "ge", "gf", "gg", "gh", "gi", "gl", "gm", "gn", "gp", "gq", "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr",
			"ht", "hu", "id", "ie", "il", "im", "in", "io", "iq", "ir", "is", "it", "je", "jm", "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp",
			"kr", "kw", "ky", "kz", "la", "lb", "lc", "li", "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "mg", "mh", "mk", "ml", "mm",
			"mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "no", "np", "nr",
			"nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "ru", "rw", "sa",
			"sb", "sc", "sd", "se", "sg", "sh", "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy", "sz", "tc", "td", "tf", "tg", "th",
			"tj", "tk", "tm", "tn", "to", "tp", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "uk", "um", "us", "uy", "uz", "va", "vc", "ve", "vg", "vi",
			"vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zw" };

		/// <summary>
		/// The Uri of the realm, with the wildcard (if any) removed.
		/// </summary>
		private Uri uri;

		/// <summary>
		/// Initializes a new instance of the <see cref="Realm"/> class.
		/// </summary>
		/// <param name="realmUrl">The realm URL to use in the new instance.</param>
		[SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "Not all realms are valid URLs (because of wildcards).")]
		public Realm(string realmUrl) {
			Requires.NotNull(realmUrl, "realmUrl"); // not non-zero check so we throw UriFormatException later
			this.DomainWildcard = Regex.IsMatch(realmUrl, WildcardDetectionPattern);
			this.uri = new Uri(Regex.Replace(realmUrl, WildcardDetectionPattern, m => m.Groups[1].Value));
			if (!this.uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
				!this.uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)) {
				throw new UriFormatException(
					string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidScheme, this.uri.Scheme));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Realm"/> class.
		/// </summary>
		/// <param name="realmUrl">The realm URL of the Relying Party.</param>
		public Realm(Uri realmUrl) {
			Requires.NotNull(realmUrl, "realmUrl");
			this.uri = realmUrl;
			if (!this.uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
				!this.uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)) {
				throw new UriFormatException(
					string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidScheme, this.uri.Scheme));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Realm"/> class.
		/// </summary>
		/// <param name="realmUriBuilder">The realm URI builder.</param>
		/// <remarks>
		/// This is useful because UriBuilder can construct a host with a wildcard
		/// in the Host property, but once there it can't be converted to a Uri.
		/// </remarks>
		internal Realm(UriBuilder realmUriBuilder)
			: this(SafeUriBuilderToString(realmUriBuilder)) { }

		/// <summary>
		/// Gets the suggested realm to use for the calling web application.
		/// </summary>
		/// <value>A realm that matches this applications root URL.</value>
		/// <remarks>
		/// 	<para>For most circumstances the Realm generated by this property is sufficient.
		/// However a wildcard Realm, such as "http://*.microsoft.com/" may at times be more
		/// desirable than "http://www.microsoft.com/" in order to allow identifier
		/// correlation across related web sites for directed identity Providers.</para>
		/// 	<para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public static Realm AutoDetect {
			get {
				RequiresEx.ValidState(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);

				var realmUrl = new UriBuilder(MessagingUtilities.GetWebRoot());

				// For RP discovery, the realm url MUST NOT redirect.  To prevent this for 
				// virtual directory hosted apps, we need to make sure that the realm path ends
				// in a slash (since our calculation above guarantees it doesn't end in a specific
				// page like default.aspx).
				if (!realmUrl.Path.EndsWith("/", StringComparison.Ordinal)) {
					realmUrl.Path += "/";
				}

				return realmUrl.Uri;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a '*.' prefix to the hostname is 
		/// used in the realm to allow subdomains or hosts to be added to the URL.
		/// </summary>
		public bool DomainWildcard { get; private set; }

		/// <summary>
		/// Gets the host component of this instance.
		/// </summary>
		public string Host {
			[DebuggerStepThrough]
			get { return this.uri.Host; }
		}

		/// <summary>
		/// Gets the scheme name for this URI.
		/// </summary>
		public string Scheme {
			[DebuggerStepThrough]
			get { return this.uri.Scheme; }
		}

		/// <summary>
		/// Gets the port number of this URI.
		/// </summary>
		public int Port {
			[DebuggerStepThrough]
			get { return this.uri.Port; }
		}

		/// <summary>
		/// Gets the absolute path of the URI.
		/// </summary>
		public string AbsolutePath {
			[DebuggerStepThrough]
			get { return this.uri.AbsolutePath; }
		}

		/// <summary>
		/// Gets the System.Uri.AbsolutePath and System.Uri.Query properties separated
		/// by a question mark (?).
		/// </summary>
		public string PathAndQuery {
			[DebuggerStepThrough]
			get { return this.uri.PathAndQuery; }
		}

		/// <summary>
		/// Gets the original string.
		/// </summary>
		/// <value>The original string.</value>
		internal string OriginalString {
			get { return this.uri.OriginalString; }
		}

		/// <summary>
		/// Gets the realm URL.  If the realm includes a wildcard, it is not included here.
		/// </summary>
		internal Uri NoWildcardUri {
			[DebuggerStepThrough]
			get { return this.uri; }
		}

		/// <summary>
		/// Gets the Realm discovery URL, where the wildcard (if present) is replaced with "www.".
		/// </summary>
		/// <remarks>
		/// See OpenID 2.0 spec section 9.2.1 for the explanation on the addition of
		/// the "www" prefix.
		/// </remarks>
		internal Uri UriWithWildcardChangedToWww {
			get {
				if (this.DomainWildcard) {
					UriBuilder builder = new UriBuilder(this.NoWildcardUri);
					builder.Host = "www." + builder.Host;
					return builder.Uri;
				} else {
					return this.NoWildcardUri;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this realm represents a reasonable (sane) set of URLs.
		/// </summary>
		/// <remarks>
		/// 'http://*.com/', for example is not a reasonable pattern, as it cannot meaningfully 
		/// specify the site claiming it. This function attempts to find many related examples, 
		/// but it can only work via heuristics. Negative responses from this method should be 
		/// treated as advisory, used only to alert the user to examine the trust root carefully.
		/// </remarks>
		internal bool IsSane {
			get {
				if (this.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) {
					return true;
				}

				string[] host_parts = this.Host.Split('.');

				string tld = host_parts[host_parts.Length - 1];

				if (Array.IndexOf(topLevelDomains, tld) < 0) {
					return false;
				}

				if (tld.Length == 2) {
					if (host_parts.Length == 1) {
						return false;
					}

					if (host_parts[host_parts.Length - 2].Length <= 3) {
						return host_parts.Length > 2;
					}
				} else {
					return host_parts.Length > 1;
				}

				return false;
			}
		}

		/// <summary>
		/// Implicitly converts the string-form of a URI to a <see cref="Realm"/> object.
		/// </summary>
		/// <param name="uri">The URI that the new Realm instance will represent.</param>
		/// <returns>The result of the conversion.</returns>
		[SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "Not all realms are valid URLs (because of wildcards).")]
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Not all Realms are valid URLs.")]
		[DebuggerStepThrough]
		public static implicit operator Realm(string uri) {
			return uri != null ? new Realm(uri) : null;
		}

		/// <summary>
		/// Implicitly converts a <see cref="Uri"/> to a <see cref="Realm"/> object.
		/// </summary>
		/// <param name="uri">The URI to convert to a realm.</param>
		/// <returns>The result of the conversion.</returns>
		[DebuggerStepThrough]
		public static implicit operator Realm(Uri uri) {
			return uri != null ? new Realm(uri) : null;
		}

		/// <summary>
		/// Implicitly converts a <see cref="Realm"/> object to its <see cref="String"/> form.
		/// </summary>
		/// <param name="realm">The realm to convert to a string value.</param>
		/// <returns>The result of the conversion.</returns>
		[DebuggerStepThrough]
		public static implicit operator string(Realm realm) {
			return realm != null ? realm.ToString() : null;
		}

		/// <summary>
		/// Checks whether one <see cref="Realm"/> is equal to another.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			Realm other = obj as Realm;
			if (other == null) {
				return false;
			}
			return this.uri.Equals(other.uri) && this.DomainWildcard == other.DomainWildcard;
		}

		/// <summary>
		/// Returns the hash code used for storing this object in a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return this.uri.GetHashCode() + (this.DomainWildcard ? 1 : 0);
		}

		/// <summary>
		/// Returns the string form of this <see cref="Realm"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			if (this.DomainWildcard) {
				UriBuilder builder = new UriBuilder(this.uri);
				builder.Host = "*." + builder.Host;
				return builder.ToStringWithImpliedPorts();
			} else {
				return this.uri.AbsoluteUri;
			}
		}

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">A string specifying URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool Contains(string url) {
			return this.Contains(new Uri(url));
		}

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool Contains(Uri url) {
			if (url.Scheme != this.Scheme) {
				return false;
			}

			if (url.Port != this.Port) {
				return false;
			}

			if (!this.DomainWildcard) {
				if (url.Host != this.Host) {
					return false;
				}
			} else {
				Debug.Assert(!string.IsNullOrEmpty(this.Host), "The host part of the Regex should evaluate to at least one char for successful parsed trust roots.");
				string[] host_parts = this.Host.Split('.');
				string[] url_parts = url.Host.Split('.');

				// If the domain containing the wildcard has more parts than the URL to match against,
				// it naturally can't be valid.
				// Unless *.example.com actually matches example.com too.
				if (host_parts.Length > url_parts.Length) {
					return false;
				}

				// Compare last part first and move forward.
				// Maybe could be done by using EndsWith, but piecewies helps ensure that
				// *.my.com doesn't match ohmeohmy.com but can still match my.com.
				for (int i = 0; i < host_parts.Length; i++) {
					string hostPart = host_parts[host_parts.Length - 1 - i];
					string urlPart = url_parts[url_parts.Length - 1 - i];
					if (!string.Equals(hostPart, urlPart, StringComparison.OrdinalIgnoreCase)) {
						return false;
					}
				}
			}

			// If path matches or is specified to root ... 
			// (deliberately case sensitive to protect security on case sensitive systems)
			if (this.PathAndQuery.Equals(url.PathAndQuery, StringComparison.Ordinal)
				|| this.PathAndQuery.Equals("/", StringComparison.Ordinal)) {
				return true;
			}

			// If trust root has a longer path, the return URL must be invalid.
			if (this.PathAndQuery.Length > url.PathAndQuery.Length) {
				return false;
			}

			// The following code assures that http://example.com/directory isn't below http://example.com/dir,
			// but makes sure http://example.com/dir/ectory is below http://example.com/dir
			int path_len = this.PathAndQuery.Length;
			string url_prefix = url.PathAndQuery.Substring(0, path_len);

			if (this.PathAndQuery != url_prefix) {
				return false;
			}

			// If trust root includes a query string ...
			if (this.PathAndQuery.Contains("?")) {
				// ... make sure return URL begins with a new argument
				return url.PathAndQuery[path_len] == '&';
			}

			// Or make sure a query string is introduced or a path below trust root
			return this.PathAndQuery.EndsWith("/", StringComparison.Ordinal)
				|| url.PathAndQuery[path_len] == '?'
				|| url.PathAndQuery[path_len] == '/';
		}

		/// <summary>
		/// Searches for an XRDS document at the realm URL, and if found, searches
		/// for a description of a relying party endpoints (OpenId login pages).
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="allowRedirects">Whether redirects may be followed when discovering the Realm.
		/// This may be true when creating an unsolicited assertion, but must be
		/// false when performing return URL verification per 2.0 spec section 9.2.1.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The details of the endpoints if found; or <c>null</c> if no service document was discovered.
		/// </returns>
		internal virtual async Task<IEnumerable<RelyingPartyEndpointDescription>> DiscoverReturnToEndpointsAsync(IHostFactories hostFactories, bool allowRedirects, CancellationToken cancellationToken) {
			XrdsDocument xrds = await this.DiscoverAsync(hostFactories, allowRedirects, cancellationToken);
			if (xrds != null) {
				return xrds.FindRelyingPartyReceivingEndpoints();
			}

			return null;
		}

		/// <summary>
		/// Searches for an XRDS document at the realm URL.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="allowRedirects">Whether redirects may be followed when discovering the Realm.
		/// This may be true when creating an unsolicited assertion, but must be
		/// false when performing return URL verification per 2.0 spec section 9.2.1.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The XRDS document if found; or <c>null</c> if no service document was discovered.
		/// </returns>
		internal virtual async Task<XrdsDocument> DiscoverAsync(IHostFactories hostFactories, bool allowRedirects, CancellationToken cancellationToken) {
			// Attempt YADIS discovery
			DiscoveryResult yadisResult = await Yadis.DiscoverAsync(hostFactories, this.UriWithWildcardChangedToWww, false, cancellationToken);
			if (yadisResult != null) {
				// Detect disallowed redirects, since realm discovery never allows them for security.
				ErrorUtilities.VerifyProtocol(allowRedirects || yadisResult.NormalizedUri == yadisResult.RequestUri, OpenIdStrings.RealmCausedRedirectUponDiscovery, yadisResult.RequestUri);
				if (yadisResult.IsXrds) {
					try {
						return new XrdsDocument(yadisResult.ResponseText);
					} catch (XmlException ex) {
						throw ErrorUtilities.Wrap(ex, XrdsStrings.InvalidXRDSDocument);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Calls <see cref="UriBuilder.ToString"/> if the argument is non-null.
		/// Otherwise throws <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <param name="realmUriBuilder">The realm URI builder.</param>
		/// <returns>The result of UriBuilder.ToString()</returns>
		/// <remarks>
		/// This simple method is worthwhile because it checks for null
		/// before dereferencing the UriBuilder.  Since this is called from
		/// within a constructor's base(...) call, this avoids a <see cref="NullReferenceException"/>
		/// when we should be throwing an <see cref="ArgumentNullException"/>.
		/// </remarks>
		private static string SafeUriBuilderToString(UriBuilder realmUriBuilder) {
			Requires.NotNull(realmUriBuilder, "realmUriBuilder");

			// Note: we MUST use ToString.  Uri property throws if wildcard is present.
			// Note that Uri.ToString() should generally be avoided, but UriBuilder.ToString()
			// is safe: http://blog.nerdbank.net/2008/04/uriabsoluteuri-and-uritostring-are-not.html
			return realmUriBuilder.ToString();
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.uri != null);
			Contract.Invariant(this.uri.AbsoluteUri != null);
		}
#endif

		/// <summary>
		/// Provides conversions to and from strings for messages that include members of this type.
		/// </summary>
		private class MessagePartRealmConverter : IMessagePartOriginalEncoder {
			/// <summary>
			/// Encodes the specified value.
			/// </summary>
			/// <param name="value">The value.  Guaranteed to never be null.</param>
			/// <returns>The <paramref name="value"/> in string form, ready for message transport.</returns>
			public string Encode(object value) {
				Requires.NotNull(value, "value");
				return value.ToString();
			}

			/// <summary>
			/// Decodes the specified value.
			/// </summary>
			/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
			/// <returns>The deserialized form of the given string.</returns>
			/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
			public object Decode(string value) {
				Requires.NotNull(value, "value");
				return new Realm(value);
			}

			/// <summary>
			/// Encodes the specified value as the original value that was formerly decoded.
			/// </summary>
			/// <param name="value">The value.  Guaranteed to never be null.</param>
			/// <returns>The <paramref name="value"/> in string form, ready for message transport.</returns>
			public string EncodeAsOriginalString(object value) {
				Requires.NotNull(value, "value");
				return ((Realm)value).OriginalString;
			}
		}
	}
}
