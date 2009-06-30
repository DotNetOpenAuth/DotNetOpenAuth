using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.InfoCard;

public partial class OP_GSAInfoCard : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}
	protected void InfoCardLevel1_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
		MultiView1.ActiveViewIndex = 1;
		siteSpecificIdLabel.Text = e.Token.SiteSpecificId;
	}
}
