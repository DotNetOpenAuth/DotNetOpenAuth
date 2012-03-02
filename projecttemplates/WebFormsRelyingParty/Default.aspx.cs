//-----------------------------------------------------------------------
// <copyright file="Default.aspx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using RelyingPartyLogic;

	public partial class _Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			User user = Database.LoggedInUser;
			this.Label1.Text = user != null ? HttpUtility.HtmlEncode(user.FirstName) : "<not logged in>";
		}
	}
}
