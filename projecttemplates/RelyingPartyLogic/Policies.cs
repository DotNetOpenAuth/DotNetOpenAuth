//-----------------------------------------------------------------------
// <copyright file="Policies.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public class Policies {
		/// <summary>
		/// The set of OP Endpoints that we trust pre-verify email addresses before sending them
		/// with positive assertions.
		/// </summary>
		public static readonly Uri[] ProviderEndpointsProvidingTrustedEmails = new Uri[] {
			new Uri("https://www.google.com/accounts/o8/ud"),
			new Uri("https://open.login.yahooapis.com/openid/op/auth"),
		};
	}
}
