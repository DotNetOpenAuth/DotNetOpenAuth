//-----------------------------------------------------------------------
// <copyright file="Model.User.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public partial class User {
		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		public User() {
			this.CreatedOnUtc = DateTime.UtcNow;
		}

		partial void OnCreatedOnUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}

		partial void OnEmailAddressChanged() {
			// Whenever the email address is changed, we must reset its verified status.
			this.EmailAddressVerified = false;
		}
	}
}
