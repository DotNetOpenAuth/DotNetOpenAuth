//-----------------------------------------------------------------------
// <copyright file="Default.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Admin {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	public partial class Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			usersRepeater.DataSource = Global.DataContext.User.Include("AuthenticationTokens");
			usersRepeater.DataBind();
		}
	}
}
