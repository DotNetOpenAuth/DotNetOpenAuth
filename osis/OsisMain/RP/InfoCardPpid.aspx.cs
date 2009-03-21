using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.InfoCard;

public partial class RP_InfoCardPpid : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}

	protected void InfoCardSelector1_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
		MultiView1.ActiveViewIndex = 1;
		siteSpecificIdentifierLabel.Text = e.Token.SiteSpecificId;
		ppidLabel.Text = e.Token.Claims[WellKnownClaimTypes.Ppid];
		issuerPubKeyHashLabel.Text = e.Token.IssuerPubKeyHash;
		uniqueIdLabel.Text = e.Token.UniqueId;
	}
}
