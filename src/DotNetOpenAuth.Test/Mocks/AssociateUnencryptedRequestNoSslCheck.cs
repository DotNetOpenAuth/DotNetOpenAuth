//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedRequestNoSslCheck.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// An associate request message that doesn't throw when
	/// it is used over HTTP (without SSL).
	/// </summary>
	internal class AssociateUnencryptedRequestNoSslCheck : AssociateUnencryptedRequest {
		internal AssociateUnencryptedRequestNoSslCheck(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint) {
		}

		public override void EnsureValidMessage() {
			// We deliberately do NOT call our base class method to avoid throwing
			// when no-encryption is used over an HTTP transport.
		}
	}
}
