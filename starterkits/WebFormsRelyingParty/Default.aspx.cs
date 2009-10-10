//-----------------------------------------------------------------------
// <copyright file="Default.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsOpenIdRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	public partial class _Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			User user = Global.LoggedInUser;
			this.Label1.Text = user != null ? user.FirstName : "<not logged in>";
		}
	}
}
