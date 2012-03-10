namespace OpenIdProviderMvc.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Routing;
	using OpenIdProviderMvc.Code;

	internal class User {
		internal static Uri ClaimedIdentifierBaseUri {
			get { return Util.GetAppPathRootedUri("user/"); }
		}

		internal static Uri GetClaimedIdentifierForUser(string username) {
			if (string.IsNullOrEmpty(username)) {
				throw new ArgumentNullException("username");
			}

			return new Uri(ClaimedIdentifierBaseUri, username.ToLowerInvariant());
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
