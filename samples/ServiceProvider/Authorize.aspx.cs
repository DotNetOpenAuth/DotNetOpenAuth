using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Conducts the user through a Consumer authorization process.
/// </summary>
public partial class Authorize : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack && false) {
			Response.Redirect("~/AuthorizedConsumers.aspx");
		}
	}
}
