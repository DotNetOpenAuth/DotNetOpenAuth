//-----------------------------------------------------------------------
// <copyright file="OAuthHybrid.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal class OAuthHybrid {
		/// <summary>
		/// Initializes static members of the <see cref="OAuthHybrid"/> class.
		/// </summary>
		static OAuthHybrid() {
			ServiceProvider = new ServiceProviderOpenIdProvider(GetServiceDescription(), TokenManager);
		}

		internal static IServiceProviderTokenManager TokenManager {
			get {
				// This is merely a sample app.  A real web app SHOULD NEVER store a memory-only
				// token manager in application.  It should be an IServiceProviderTokenManager
				// implementation that is bound to a database.
				var tokenManager = (IServiceProviderTokenManager)HttpContext.Current.Application["TokenManager"];
				if (tokenManager == null) {
					HttpContext.Current.Application["TokenManager"] = tokenManager = new InMemoryTokenManager();
				}

				return tokenManager;
			}
		}

		internal static ServiceProviderOpenIdProvider ServiceProvider { get; private set; }

		internal static ServiceProviderHostDescription GetServiceDescription() {
			return new ServiceProviderHostDescription {
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
			};
		}
	}
}
