//-----------------------------------------------------------------------
// <copyright file="InMemoryServiceProviderAccessToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public class InMemoryServiceProviderAccessToken : IServiceProviderAccessToken {
		#region IServiceProviderAccessToken Members

		public string Token { get; set; }

		public DateTime? ExpirationDate { get; set; }

		public string Username { get; set; }

		public string[] Roles { get; set; }

		#endregion

		public string Secret { get; set; }

		public string Scope { get; set; }
	}
}
