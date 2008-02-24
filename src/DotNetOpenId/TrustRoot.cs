using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId {
	/// <summary>
	/// A trust root to validate requests and match return URLs against.
	/// </summary>
	/// <!-- http://openid.net/specs/openid-authentication-1_1.html#anchor16 -->
	/// <!-- http://openid.net/specs/openid-authentication-1_1.html#anchor21 -->
	public class TrustRoot {
		static Regex _tr_regex = new Regex(@"^(?<scheme>https?)://((?<wildcard>\*)|(?<wildcard>\*\.)?(?<host>[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*)\.?)(:(?<port>[0-9]+))?(?<path>(/.*|$))");
		static string[] _top_level_domains =    {"com", "edu", "gov", "int", "mil", "net", "org", "biz", "info", "name", "museum", "coop", "aero", "ac", "ad", "ae",
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
			"vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zw"};
		string _scheme;
		/// <summary>
		/// Whether a '*.' prefix to the hostname is used in the trustroot to allow
		/// subdomains or hosts to be added to the URL.
		/// </summary>
		bool hasDomainWildcard;
		string _host;
		int _port;
		string _path;
		string _original;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public TrustRoot(string trustRootUrl) {
			_original = trustRootUrl;
			Match mo = _tr_regex.Match(trustRootUrl);

			if (mo.Success) {
				_scheme = mo.Groups["scheme"].Value;
				hasDomainWildcard = !string.IsNullOrEmpty(mo.Groups["wildcard"].Value);
				_host = mo.Groups["host"].Value.ToLowerInvariant();

				Group port_group = mo.Groups["port"];
				if (port_group.Success)
					_port = Convert.ToInt32(port_group.Value, CultureInfo.InvariantCulture);
				else 
					_port = (_scheme == "https") ? 443 : 80;

				_path = mo.Groups["path"].Value;
				if (string.IsNullOrEmpty(_path))
					_path = "/";
			} else {
				throw new UriFormatException(string.Format(CultureInfo.CurrentUICulture,
					"'{0}' is not a valid OpenID trustroot.", trustRootUrl));
			}
		}

		/// <summary>
		/// This method checks the to see if a trust root represents a reasonable (sane) set of URLs.
		/// </summary>
		/// <remarks>
		/// 'http://*.com/', for example is not a reasonable pattern, as it cannot meaningfully 
		/// specify the site claiming it. This function attempts to find many related examples, 
		/// but it can only work via heuristics. Negative responses from this method should be 
		/// treated as advisory, used only to alert the user to examine the trust root carefully.
		/// </remarks>
		internal bool IsSane {
			get {
				if (_host == "localhost")
					return true;

				string[] host_parts = _host.Split('.');

				string tld = host_parts[host_parts.Length - 1];

				if (Array.IndexOf(_top_level_domains, tld) < 0)
					return false;

				if (tld.Length == 2) {
					if (host_parts.Length == 1)
						return false;

					if (host_parts[host_parts.Length - 2].Length <= 3)
						return host_parts.Length > 2;

				} else {
					return host_parts.Length > 1;
				}

				return false;
			}
		}

		public string Url { get { return _original; } }

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">A string specifying URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool IsUrlWithinTrustRoot(string url) {
			Uri uri = new Uri(url);

			return IsUrlWithinTrustRoot(uri);
		}

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool IsUrlWithinTrustRoot(Uri url) {
			if (url.Scheme != _scheme)
				return false;

			if (url.Port != _port)
				return false;

			if (!hasDomainWildcard) {
				if (url.Host != _host) {
					return false;
				}
			} else {
				Debug.Assert(!string.IsNullOrEmpty(_host), "The host part of the Regex should evaluate to at least one char for successful parsed trust roots.");
				string[] host_parts = _host.Split('.');
				string[] url_parts = url.Host.Split('.');

				// If the domain contain the wildcard has more parts than the URL to match against,
				// it naturally can't be valid.
				// Unless *.example.com actually matches example.com too.
				if (host_parts.Length > url_parts.Length)
					return false;

				// Compare last part first and move forward.
				// Could be done by using EndsWith, but this solution seems more elegant.
				for (int i = host_parts.Length - 1; i >= 0; i--) {
					/*
					if (host_parts[i].Equals("*", StringComparison.Ordinal))
					{
						break;
					}
					 */

					if (!host_parts[i].Equals(url_parts[i + 1], StringComparison.OrdinalIgnoreCase)) {
						return false;
					}
				}
			}

			// If path matches or is specified to root ...
			if (_path.Equals(url.PathAndQuery, StringComparison.Ordinal)
				|| _path.Equals("/", StringComparison.Ordinal))
				return true;

			// If trust root has a longer path, the return URL must be invalid.
			if (_path.Length > url.PathAndQuery.Length)
				return false;

			// The following code assures that http://example.com/directory isn't below http://example.com/dir,
			// but makes sure http://example.com/dir/ectory is below http://example.com/dir
			int path_len = _path.Length;
			string url_prefix = url.PathAndQuery.Substring(0, path_len);

			if (_path != url_prefix)
				return false;

			// If trust root includes a query string ...
			if (_path.Contains("?")) {
				// ... make sure return URL begins with a new argument
				return url.PathAndQuery[path_len] == '&';
			}

			// Or make sure a query string is introduced or a path below trust root
			return _path.EndsWith("/", StringComparison.Ordinal)
				|| url.PathAndQuery[path_len] == '?'
				|| url.PathAndQuery[path_len] == '/';
		}

		public override string ToString() {
			return _original;
		}
	}
}