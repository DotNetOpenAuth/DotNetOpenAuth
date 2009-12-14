//-----------------------------------------------------------------------
// <copyright file="Model.IssuedToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class IssuedToken {
		/// <summary>
		/// Initializes a new instance of the <see cref="IssuedToken"/> class.
		/// </summary>
		public IssuedToken() {
			this.CreatedOnUtc = DateTime.UtcNow;
		}

		partial void OnCreatedOnUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}
	}
}
