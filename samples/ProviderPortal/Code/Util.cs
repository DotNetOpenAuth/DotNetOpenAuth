using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenId.Provider;
using ProviderPortal;
using DotNetOpenId;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util {
	public static string ExtractUserName(Uri url) {
		return url.Segments[url.Segments.Length - 1];
	}
	public static string ExtractUserName(Identifier identifier) {
		return ExtractUserName(new Uri(identifier.ToString()));
	}
	public static Identifier BuildIdentityUrl() {
		string username = HttpContext.Current.User.Identity.Name;
		// be sure to normalize case the way the user's identity page does.
		username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1).ToLowerInvariant();
		return new Uri(HttpContext.Current.Request.Url, "/user/" + username);
	}
}
