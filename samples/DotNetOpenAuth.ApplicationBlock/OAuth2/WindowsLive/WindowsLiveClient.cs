//-----------------------------------------------------------------------
// <copyright file="WindowsLiveClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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

	public class WindowsLiveClient : WebServerClient {
		private static readonly AuthorizationServerDescription WindowsLiveDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://oauth.live.com/token"),
			AuthorizationEndpoint = new Uri("https://oauth.live.com/authorize"),
			ProtocolVersion = ProtocolVersion.V20
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsLiveClient"/> class.
		/// </summary>
		public WindowsLiveClient()
			: base(WindowsLiveDescription) {
		}

		public async Task<IOAuth2Graph> GetGraphAsync(IAuthorizationState authState, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken)) {
			if ((authState != null) && (authState.AccessToken != null)) {
				var httpClient = new HttpClient(this.CreateAuthorizingHandler(authState));
				using (var response = await httpClient.GetAsync("https://apis.live.net/v5.0/me", cancellationToken)) {
					response.EnsureSuccessStatusCode();
					using (var responseStream = await response.Content.ReadAsStreamAsync()) {
						// string debugJsonStr = new StreamReader(responseStream).ReadToEnd();
						WindowsLiveGraph windowsLiveGraph = WindowsLiveGraph.Deserialize(responseStream);

						// picture type resolution test 1
						// &type=small 96x96
						// &type=medium 96x96
						// &type=large 448x448
						windowsLiveGraph.AvatarUrl =
							new Uri("https://apis.live.net/v5.0/me/picture?access_token=" + Uri.EscapeDataString(authState.AccessToken));

						return windowsLiveGraph;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Well-known scopes defined by the Windows Live service.
		/// </summary>
		/// <remarks>
		/// This sample includes just a few scopes.  For a complete list of scopes please refer to:
		/// http://msdn.microsoft.com/en-us/library/hh243646.aspx
		/// </remarks>
		public static class Scopes {
			#region Core Scopes

			/// <summary>
			/// The ability of an app to read and update a user's info at any time. Without this scope, an app can access the user's info only while the user is signed in to Live Connect and is using your app.
			/// </summary>
			public const string OfflineAccess = "wl.offline_access";

			/// <summary>
			/// Single sign-in behavior. With single sign-in, users who are already signed in to Live Connect are also signed in to your website.
			/// </summary>
			public const string SignIn = "wl.signin";

			/// <summary>
			/// Read access to a user's basic profile info. Also enables read access to a user's list of contacts.
			/// </summary>
			public const string Basic = "wl.basic";

			#endregion

			#region Extended Scopes

			/// <summary>
			/// Read access to a user's birthday info including birth day, month, and year.
			/// </summary>
			public const string Birthday = "wl.birthday";

			/// <summary>
			/// Read access to a user's calendars and events.
			/// </summary>
			public const string Calendars = "wl.calendars";

			/// <summary>
			/// Read and write access to a user's calendars and events.
			/// </summary>
			public const string CalendarsUpdate = "wl.calendars_update";

			/// <summary>
			/// Read access to the birth day and birth month of a user's contacts. Note that this also gives read access to the user's birth day, birth month, and birth year.
			/// </summary>
			public const string ContactsBirthday = "wl.contacts_birthday";

			/// <summary>
			/// Creation of new contacts in the user's address book.
			/// </summary>
			public const string ContactsCreate = "wl.contacts_create";

			/// <summary>
			/// Read access to a user's calendars and events. Also enables read access to any calendars and events that other users have shared with the user.
			/// </summary>
			public const string ContactsCalendars = "wl.contacts_calendars";

			/// <summary>
			/// Read access to a user's albums, photos, videos, and audio, and their associated comments and tags. Also enables read access to any albums, photos, videos, and audio that other users have shared with the user.
			/// </summary>
			public const string ContactsPhotos = "wl.contacts_photos";

			/// <summary>
			/// Read access to Microsoft SkyDrive files that other users have shared with the user. Note that this also gives read access to the user's files stored in SkyDrive.
			/// </summary>
			public const string ContactsSkydrive = "wl.contacts_skydrive";

			/// <summary>
			/// Read access to a user's personal, preferred, and business email addresses.
			/// </summary>
			public const string Emails = "wl.emails";

			/// <summary>
			/// Creation of events on the user's default calendar.
			/// </summary>
			public const string EventsCreate = "wl.events_create";

			/// <summary>
			/// Enables signing in to the Windows Live Messenger Extensible Messaging and Presence Protocol (XMPP) service.
			/// </summary>
			public const string Messenger = "wl.messenger";

			/// <summary>
			/// Read access to a user's personal, business, and mobile phone numbers.
			/// </summary>
			public const string PhoneNumbers = "wl.phone_numbers";

			/// <summary>
			/// Read access to a user's photos, videos, audio, and albums.
			/// </summary>
			public const string Photos = "wl.photos";

			/// <summary>
			/// Read access to a user's postal addresses.
			/// </summary>
			public const string PostalAddresses = "wl.postal_addresses";

			/// <summary>
			/// Enables updating a user's status message.
			/// </summary>
			public const string Share = "wl.share";

			/// <summary>
			/// Read access to a user's files stored in SkyDrive.
			/// </summary>
			public const string Skydrive = "wl.skydrive";

			/// <summary>
			/// Read and write access to a user's files stored in SkyDrive.
			/// </summary>
			public const string SkydriveUpdate = "wl.skydrive_update";

			/// <summary>
			/// Read access to a user's employer and work position information.
			/// </summary>
			public const string WorkProfile = "wl.work_profile";

			#endregion
		}
	}
}
