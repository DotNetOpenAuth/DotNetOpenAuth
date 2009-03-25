using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.InfoCard;

public partial class RP_InfoCardPpid : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		InfoCardSelector1.Audience = null; // disable audience check to allow for cross-domain posting.
		if (!this.IsPostBack) {
			string action = string.IsNullOrEmpty(Form.Action) ? Page.Request.Url.AbsoluteUri : Form.Action;
			ListItem matchingActionItem = actionDropDown.Items.FindByValue(action);
			if (matchingActionItem == null) {
				actionDropDown.Items.Add(Page.Request.Url.AbsoluteUri);
				matchingActionItem = actionDropDown.Items.FindByValue(action);
			}
			actionDropDown.SelectedIndex = actionDropDown.Items.IndexOf(actionDropDown.Items.FindByValue(action));
		}
	}

	protected void InfoCardSelector1_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
		MultiView1.ActiveViewIndex = 1;
		siteSpecificIdentifierLabel.Text = e.Token.SiteSpecificId;
		ppidLabel.Text = e.Token.Claims[ClaimTypes.PPID];
		issuerPubKeyHashLabel.Text = e.Token.IssuerPubKeyHash;
		uniqueIdLabel.Text = e.Token.UniqueId;
	}

	protected void InfoCardSelector1_TokenProcessingError(object sender, TokenProcessingErrorEventArgs e) {
		MultiView1.ActiveViewIndex = 2;
		processingErrorLabel.Text = e.Exception.Message;
		unprocessedTokenLabel.Text = HttpUtility.HtmlEncode(e.TokenXml);
	}
}
