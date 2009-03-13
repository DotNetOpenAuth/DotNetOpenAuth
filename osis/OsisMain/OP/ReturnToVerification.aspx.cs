using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.Messaging;

public partial class OP_ReturnToVerification : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			identifierBox.Focus();
		}
	}

	private void TestAction(string returnTo) {
		if (!Page.IsValid) {
			return;
		}

		OpenIdRelyingParty rp = new OpenIdRelyingParty();
		// Only OpenID 2.0 and later support RP discovery.
		rp.SecuritySettings.MinimumRequiredOpenIdVersion = DotNetOpenAuth.OpenId.ProtocolVersion.V20;
		try {
			rp.CreateRequest(identifierBox.Text, Realm, new Uri(Request.Url, Page.ResolveUrl(returnTo))).RedirectToProvider();
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
		}
	}

	protected void beginVerifiableButton_Click(object sender, EventArgs e) {
		TestAction("ReturnToVerification.Valid.aspx");
	}

	protected void beginValid2Button_Click(object sender, EventArgs e) {
		TestAction("RPDiscoveryRealm/ReturnToVerification.Valid.aspx");
	}

	protected void beginUnverifiableButton_Click(object sender, EventArgs e) {
		// This is invalid because it is not mentioned in the XRDS file.
		TestAction("ReturnToVerification.Invalid.aspx");
	}

	private Uri Realm {
		get { return new Uri(Request.Url, Response.ApplyAppPathModifier("~/")); }
	}
}
