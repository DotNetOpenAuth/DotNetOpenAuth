using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RP_AssociationPoisoning : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (Request.QueryString["test"] == "1") {
				IdentityTest1.Visible = true;
			} else if (Request.QueryString["test"] == "2") {
				IdentityTest2.Visible = true;
			} else if (Request.QueryString["test"] == "3") {
				IdentityTest3.Visible = true;
			}
			if (Request.QueryString["stateless"] != null) {
				StatelessRPInvalid.Visible = true;
			}
		}
	}
}
