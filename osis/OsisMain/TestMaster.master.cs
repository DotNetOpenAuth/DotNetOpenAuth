using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class TestMaster : System.Web.UI.MasterPage {
	protected void resetTestButton_Click(object sender, EventArgs e) {
		this.Response.Redirect(this.Request.Url.AbsolutePath);
	}
}
