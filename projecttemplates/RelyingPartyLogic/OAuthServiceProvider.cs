//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	public class OAuthServiceProvider {
		private const string PendingAuthorizationRequestSessionKey = "PendingAuthorizationRequest";

		/// <summary>
		/// The shared service description for this web site.
		/// </summary>
		private static AuthorizationServerDescription authorizationServerDescription;

		/// <summary>
		/// The shared authorization server.
		/// </summary>
		private static WebServerAuthorizationServer authorizationServer;

		/// <summary>
		/// The lock to synchronize initialization of the <see cref="authorizationServer"/> field.
		/// </summary>
		private static readonly object InitializerLock = new object();

		/// <summary>
		/// Gets the service provider.
		/// </summary>
		/// <value>The service provider.</value>
		public static WebServerAuthorizationServer AuthorizationServer {
			get {
				EnsureInitialized();
				return authorizationServer;
			}
		}

		/// <summary>
		/// Gets the service description.
		/// </summary>
		/// <value>The service description.</value>
		public static AuthorizationServerDescription AuthorizationServerDescription {
			get {
				EnsureInitialized();
				return authorizationServerDescription;
			}
		}

		/// <summary>
		/// Initializes the <see cref="authorizationServer"/> field if it has not yet been initialized.
		/// </summary>
		private static void EnsureInitialized() {
			if (authorizationServer == null) {
				lock (InitializerLock) {
					if (authorizationServerDescription == null) {
						authorizationServerDescription = new AuthorizationServerDescription {
							AuthorizationEndpoint = new Uri(Utilities.ApplicationRoot, "OAuth.ashx"),
							TokenEndpoint = new Uri(Utilities.ApplicationRoot, "OAuth.ashx"),
						};
					}

					if (authorizationServer == null) {
						authorizationServer = new WebServerAuthorizationServer(new OAuthAuthorizationServer());
					}
				}
			}
		}
	}
}
