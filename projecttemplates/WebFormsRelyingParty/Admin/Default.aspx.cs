//-----------------------------------------------------------------------
// <copyright file="Default.aspx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using RelyingPartyLogic;

	public partial class Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.usersRepeater.DataSource = Database.DataContext.Users.Include("AuthenticationTokens");
			this.usersRepeater.DataBind();
		}
	}
}
