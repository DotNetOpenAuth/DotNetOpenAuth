//-----------------------------------------------------------------------
// <copyright file="InMemoryConsumerDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public class InMemoryConsumerDescription : IConsumerDescription {
		#region IConsumerDescription Members

		public string Key { get; set; }

		public string Secret { get; set; }

		public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate { get; set; }

		public Uri Callback { get; set; }

		public DotNetOpenAuth.OAuth.VerificationCodeFormat VerificationCodeFormat { get; set; }

		public int VerificationCodeLength { get; set; }

		#endregion
	}
}
