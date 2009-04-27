namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	internal static class Util {
		internal static Uri GetAppPathRootedUri(string value) {
			string appPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
			if (!appPath.EndsWith("/")) {
				appPath += "/";
			}

			return new Uri(HttpContext.Current.Request.Url, appPath + value);
		}
	}
}
