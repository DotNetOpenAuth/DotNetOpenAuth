//-----------------------------------------------------------------------
// <copyright file="WindowsLiveClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;

	public class WindowsLiveClient : WebServerClient {
		private static readonly AuthorizationServerDescription WindowsLiveDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://oauth.live.com/token"),
			AuthorizationEndpoint = new Uri("https://oauth.live.com/authorize"),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsLiveClient"/> class.
		/// </summary>
		public WindowsLiveClient()
			: base(WindowsLiveDescription) {
		}

		/// <summary>
		/// Well-known scopes defined by the Windows Live service.
		/// </summary>
		/// <remarks>
		/// This sample includes just a few scopes.  For a complete list of scopes please refer to:
		/// http://msdn.microsoft.com/en-us/library/hh243646.aspx
		/// </remarks>
		public static class Scopes {
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
		}
	}
}
