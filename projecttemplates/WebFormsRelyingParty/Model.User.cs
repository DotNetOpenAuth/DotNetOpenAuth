//-----------------------------------------------------------------------
// <copyright file="Model.User.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public partial class User {
		partial void OnEmailAddressChanged() {
			// Whenever the email address is changed, we must reset its verified status.
			this.EmailAddressVerified = false;
		}
	}
}
