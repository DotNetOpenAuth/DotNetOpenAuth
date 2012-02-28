//-----------------------------------------------------------------------
// <copyright file="Model.ClientAuthorization.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class ClientAuthorization {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAuthorization"/> class.
		/// </summary>
		public ClientAuthorization() {
			this.CreatedOnUtc = DateTime.UtcNow;
		}

		partial void OnCreatedOnUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}
	}
}
