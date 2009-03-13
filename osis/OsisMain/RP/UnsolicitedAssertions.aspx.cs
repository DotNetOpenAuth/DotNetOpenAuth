using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId;

public partial class RP_UnsolicitedAssertions : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			rpRealmBox.Focus();
		}
	}
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		// We tunnel the realm through an Identifier first to allow
		// for sloppy user input auto-correction.
		Realm realm = new Realm(Identifier.Parse(rpRealmBox.Text));

		OpenIdProvider op = new OpenIdProvider();
		Uri assertedIdentityAndEndpoint = new Uri(Request.Url, Page.ResolveUrl("AffirmativeIdentity.aspx"));
		op.PrepareUnsolicitedAssertion(
			assertedIdentityAndEndpoint,
			realm,
			assertedIdentityAndEndpoint,
			assertedIdentityAndEndpoint).Send();
	}
}
