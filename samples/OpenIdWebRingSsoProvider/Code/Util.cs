//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdWebRingSsoProvider.Code {
	using System;
	using System.Configuration;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Provider;

	public class Util {
		private const string RolesAttribute = "http://samples.dotnetopenauth.net/sso/roles";

		/// <summary>
		/// Gets a value indicating whether the authentication system used by the OP requires
		/// no user interaction (an HTTP header based authentication protocol).
		/// </summary>
		internal static bool ImplicitAuth {
			get {
				// This should return false if using FormsAuthentication.
				return bool.Parse(ConfigurationManager.AppSettings["ImplicitAuth"]);
			}
		}

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

		internal static async Task ProcessAuthenticationChallengeAsync(IAuthenticationRequest idrequest, CancellationToken cancellationToken) {
			// Verify that RP discovery is successful.
			var providerEndpoint = new ProviderEndpoint();
			if (await idrequest.IsReturnUrlDiscoverableAsync(providerEndpoint.Provider.Channel.HostFactories, cancellationToken) != RelyingPartyDiscoveryResult.Success) {
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
					// If the RP demands an immediate answer, or if we're using implicit authentication
					// and therefore have nothing further to ask the user, just reject the authentication.
					if (idrequest.Immediate || ImplicitAuth) {
						idrequest.IsAuthenticated = false;
					} else {
						// Send the user to a page to actually log into the OP.
						if (!HttpContext.Current.Request.Path.EndsWith("Login.aspx", StringComparison.OrdinalIgnoreCase)) {
							HttpContext.Current.Response.Redirect("~/Login.aspx");
						}
					}
				}
			} else {
				string userOwningOpenIdUrl = Util.ExtractUserName(idrequest.LocalIdentifier);

				// NOTE: in a production provider site, you may want to only 
				// respond affirmatively if the user has already authorized this consumer
				// to know the answer.
				idrequest.IsAuthenticated = userOwningOpenIdUrl == HttpContext.Current.User.Identity.Name;

				if (!idrequest.IsAuthenticated.Value && !ImplicitAuth && !idrequest.Immediate) {
					// Send the user to a page to actually log into the OP.
					if (!HttpContext.Current.Request.Path.EndsWith("Login.aspx", StringComparison.OrdinalIgnoreCase)) {
						HttpContext.Current.Response.Redirect("~/Login.aspx");
					}
				}
			}

			if (idrequest.IsAuthenticated.Value) {
				// add extension responses here.
				var fetchRequest = idrequest.GetExtension<FetchRequest>();
				if (fetchRequest != null) {
					var fetchResponse = new FetchResponse();
					if (fetchRequest.Attributes.Contains(RolesAttribute)) {
						// Inform the RP what roles this user should fill
						// These roles would normally come out of the user database
						// or Windows security groups.
						fetchResponse.Attributes.Add(RolesAttribute, "Member", "Admin");
					}
					idrequest.AddResponseExtension(fetchResponse);
				}
			}
		}
	}
}