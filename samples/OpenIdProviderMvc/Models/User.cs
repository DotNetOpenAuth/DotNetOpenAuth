namespace OpenIdProviderMvc.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Routing;

	internal class User {
		internal static Uri GetClaimedIdentifierForUser(string username) {
			string appPath = HttpContext.Current.Request.ApplicationPath;
			if (!appPath.EndsWith("/")) {
				appPath += "/";
			}
			Uri claimedIdentifier = new Uri(
				HttpContext.Current.Request.Url,
				appPath + "user/" + username);
			return new Uri(claimedIdentifier.AbsoluteUri.ToLowerInvariant());
		}

		internal static string GetUserFromClaimedIdentifier(Uri claimedIdentifier) {
			Regex regex = new Regex(@"/user/([^/\?]+)");
			Match m = regex.Match(claimedIdentifier.AbsoluteUri);
			if (!m.Success) {
				throw new ArgumentException();
			}

			return m.Groups[1].Value;
		}

		internal static Uri GetNormalizedClaimedIdentifier(Uri uri) {
			return GetClaimedIdentifierForUser(GetUserFromClaimedIdentifier(uri));
		}
	}
}
