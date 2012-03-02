//-----------------------------------------------------------------------
// <copyright file="InMemoryServiceProviderRequestToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public class InMemoryServiceProviderRequestToken : IServiceProviderRequestToken {
		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryServiceProviderRequestToken"/> class.
		/// </summary>
		public InMemoryServiceProviderRequestToken() {
			this.CreatedOn = DateTime.Now;
		}

		#region IServiceProviderRequestToken Members

		public string Token { get; set; }

		public string ConsumerKey { get; set; }

		public DateTime CreatedOn { get; set; }

		public Uri Callback { get; set; }

		public string VerificationCode { get; set; }

		public Version ConsumerVersion { get; set; }

		#endregion

		public string Secret { get; set; }

		public string Scope { get; set; }
	}
}
