//-----------------------------------------------------------------------
// <copyright file="TokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using DotNetOpenAuth.OAuth2;

	public class TokenManager : IClientAuthorizationTracker {
		public IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState) {
			return new AuthorizationState {
				Callback = callbackUrl,
			};
		}
	}
}
