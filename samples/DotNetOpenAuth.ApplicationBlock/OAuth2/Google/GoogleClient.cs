//-----------------------------------------------------------------------
// <copyright file="GoogleClient.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	//// https://accounts.google.com/o/oauth2/auth

	public class GoogleClient : WebServerClient {
		private static readonly AuthorizationServerDescription GoogleDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://accounts.google.com/o/oauth2/token"),
			AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth"),
			//// RevokeEndpoint = new Uri("https://accounts.google.com/o/oauth2/revoke"),
			ProtocolVersion = ProtocolVersion.V20
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleClient"/> class.
		/// </summary>
		public GoogleClient()
			: base(GoogleDescription) {
		}

		public async Task<IOAuth2Graph> GetGraphAsync(IAuthorizationState authState, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken)) {
			if ((authState != null) && (authState.AccessToken != null)) {
				var httpClient = new HttpClient(this.CreateAuthorizingHandler(authState));
				using (var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v1/userinfo", cancellationToken)) {
					response.EnsureSuccessStatusCode();
					using (var responseStream = await response.Content.ReadAsStreamAsync()) {
						return GoogleGraph.Deserialize(responseStream);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Well-known scopes defined by Google.
		/// </summary>
		/// <remarks>
		/// This sample includes just a few scopes.  For a complete list of permissions please refer to:
		/// https://developers.google.com/accounts/docs/OAuth2Login
		/// </remarks>
		public static class Scopes {
			public const string PlusMe = "https://www.googleapis.com/auth/plus.me";

			/// <summary>
			/// Scopes that cover queries for user data.
			/// </summary>
			public static class UserInfo {
				/// <summary>
				/// Gain read-only access to basic profile information, including a user identifier, name, profile photo, profile URL, country, language, timezone, and birthdate.
				/// </summary>
				public const string Profile = "https://www.googleapis.com/auth/userinfo.profile";

				/// <summary>
				/// Gain read-only access to the user's email address.
				/// </summary>
				public const string Email = "https://www.googleapis.com/auth/userinfo.email";
			}

			public static class Drive {
				/// <summary>
				/// Full, permissive scope to access all of a user's files. Request this scope only when it is strictly necessary.
				/// </summary>
				public const string Default = "https://www.googleapis.com/auth/drive";

				/// <summary>
				/// Per-file access to files created or opened by the app
				/// </summary>
				public const string File = "https://www.googleapis.com/auth/drive.file";

				/// <summary>
				/// Allows apps read-only access to the list of Drive apps a user has installed
				/// </summary>
				public const string AppsReadonly = "https://www.googleapis.com/auth/drive.apps.readonly";

				/// <summary>
				/// Allows read-only access to file metadata and file content
				/// </summary>
				public const string Readonly = "https://www.googleapis.com/auth/drive.readonly";

				/// <summary>
				/// Allows read-only access to file metadata, but does not allow any access to read or download file content
				/// </summary>
				public const string Metadata = "https://www.googleapis.com/auth/drive.readonly.metadata";

				/// <summary>
				/// Special scope used to let users approve installation of an app
				/// </summary>
				public const string Install = "https://www.googleapis.com/auth/drive.install";
			}
		}
	}
}
