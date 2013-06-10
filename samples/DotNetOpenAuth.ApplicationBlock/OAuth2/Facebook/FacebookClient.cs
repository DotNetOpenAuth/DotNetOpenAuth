//-----------------------------------------------------------------------
// <copyright file="FacebookClient.cs" company="Outercurve Foundation">
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

	public class FacebookClient : WebServerClient {
		private static readonly AuthorizationServerDescription FacebookDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token"),
			AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize"),
			ProtocolVersion = ProtocolVersion.V20
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="FacebookClient"/> class.
		/// </summary>
		public FacebookClient()
			: base(FacebookDescription) {
		}

		public async Task<IOAuth2Graph> GetGraphAsync(IAuthorizationState authState, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken)) {
			if ((authState != null) && (authState.AccessToken != null)) {
				var httpClient = new HttpClient(this.CreateAuthorizingHandler(authState));
				string fieldsStr = (fields == null) || (fields.Length == 0) ? FacebookGraph.Fields.Defaults : string.Join(",", fields);

				using (
					var response = await httpClient.GetAsync("https://graph.Facebook.com/me?fields=" + fieldsStr, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					using (var responseStream = await response.Content.ReadAsStreamAsync()) {
						return FacebookGraph.Deserialize(responseStream);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Well-known permissions defined by Facebook.
		/// </summary>
		/// <remarks>
		/// This sample includes just a few permissions.  For a complete list of permissions please refer to:
		/// https://developers.facebook.com/docs/reference/login/
		/// </remarks>
		public static class Scopes {
			#region Email Permissions
			/// <summary>
			/// Provides access to the user's primary email address in the email property. Do not spam users. Your use of email must comply both with Facebook policies and with the CAN-SPAM Act.
			/// </summary>
			public const string Email = "email";
			#endregion

			#region Extended Permissions
			/// <summary>
			/// Provides access to any friend lists the user created. All user's friends are provided as part of basic data, this extended permission grants access to the lists of friends a user has created, and should only be requested if your application utilizes lists of friends.
			/// </summary>
			public const string ReadFriendlists = "read_friendlists";

			/// <summary>
			/// Provides read access to the Insights data for pages, applications, and domains the user owns.
			/// </summary>
			public const string ReadInsights = "read_insights";
			#endregion

			#region Extended Profile Properties
			/// <summary>
			/// Provides access to the "About Me" section of the profile in the about property
			/// </summary>
			public const string UserAboutMe = "user_about_me";

			/// <summary>
			/// Provides access to the user's list of activities as the activities connection
			/// </summary>
			public const string UserActivities = "user_activities";

			/// <summary>
			/// Provides access to the birthday with year as the birthday property. Note that your app may determine if a user is "old enough" to use an app by obtaining the age_range public profile property
			/// </summary>
			public const string UserBirthday = "user_birthday";
			#endregion

			#region Open Graph Permissions
			#endregion

			#region Page Permissions
			#endregion

			#region Public Profile and Friend List
			#endregion
		}
	}
}
