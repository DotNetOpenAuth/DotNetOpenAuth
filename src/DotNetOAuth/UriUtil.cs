using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace DotNetOAuth {
	class UriUtil {
		internal static bool QueryStringContainsOAuthParameters(Uri uri) {
			if (uri == null) return false;
			NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
			return nvc.Keys.OfType<string>().Any(key => key.StartsWith(Protocol.V10.ParameterPrefix, StringComparison.Ordinal));
		}
	}
}
