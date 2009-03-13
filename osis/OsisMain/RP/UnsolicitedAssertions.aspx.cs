using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;

public partial class RP_UnsolicitedAssertions : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		OpenIdProvider op = new OpenIdProvider();
		Uri assertedIdentityAndEndpoint = new Uri(Request.Url, Page.ResolveUrl("AffirmativeIdentity.aspx"));
		op.PrepareUnsolicitedAssertion(
			assertedIdentityAndEndpoint,
			rpRealmBox.Text,
			assertedIdentityAndEndpoint,
			assertedIdentityAndEndpoint).Send();
	}
}
