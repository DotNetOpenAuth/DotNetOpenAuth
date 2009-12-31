//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdWebRingSsoProvider.Code {
	using System;
	using System.Configuration;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	public class Util {
		public static string ExtractUserName(Uri url) {
			return url.Segments[url.Segments.Length - 1];
		}

		public static string ExtractUserName(Identifier identifier) {
			return ExtractUserName(new Uri(identifier.ToString()));
		}

		public static Identifier BuildIdentityUrl() {
			string username = HttpContext.Current.User.Identity.Name;
			int slash = username.IndexOf('\\');
			if (slash >= 0) {
				username = username.Substring(slash + 1);
			}
			return BuildIdentityUrl(username);
		}

		public static Identifier BuildIdentityUrl(string username) {
			// This sample Provider has a custom policy for normalizing URIs, which is that the whole
			// path of the URI be lowercase except for the first letter of the username.
			username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1).ToLowerInvariant();
			return new Uri(HttpContext.Current.Request.Url, HttpContext.Current.Response.ApplyAppPathModifier("~/user.aspx/" + username));
		}

		internal static void ProcessAuthenticationChallenge(IAuthenticationRequest idrequest) {
			// Verify that RP discovery is successful.
			if (idrequest.IsReturnUrlDiscoverable(ProviderEndpoint.Provider) != RelyingPartyDiscoveryResult.Success) {
				idrequest.IsAuthenticated = false;
				return;
			}

			// Verify that the RP is on the whitelist.  Realms are case sensitive.
			string[] whitelist = ConfigurationManager.AppSettings["whitelistedRealms"].Split(';');
			if (Array.IndexOf(whitelist, idrequest.Realm.ToString()) < 0) {
				idrequest.IsAuthenticated = false;
				return;
			}

			if (idrequest.IsDirectedIdentity) {
				if (HttpContext.Current.User.Identity.IsAuthenticated) {
					idrequest.LocalIdentifier = Util.BuildIdentityUrl();
					idrequest.IsAuthenticated = true;
				} else {
					idrequest.IsAuthenticated = false;
				}
			} else {
				string userOwningOpenIdUrl = Util.ExtractUserName(idrequest.LocalIdentifier);

				// NOTE: in a production provider site, you may want to only 
				// respond affirmatively if the user has already authorized this consumer
				// to know the answer.
				idrequest.IsAuthenticated = userOwningOpenIdUrl == HttpContext.Current.User.Identity.Name;
			}

			if (idrequest.IsAuthenticated.Value) {
				// add extension responses here.
			}
		}
	}
}