using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class XP_Selector : System.Web.UI.Page {
	private OpenIdRelyingParty rp = new OpenIdRelyingParty();
	protected void Page_Load(object sender, EventArgs e) {
		var response = rp.GetResponse();
		if (response != null) {
			if (response.Status == AuthenticationStatus.Authenticated) {
				MultiView1.SetActiveView(View2);
			} else {
				MultiView1.SetActiveView(View3);
				if (response.Status == AuthenticationStatus.Failed) {
					errorDetails.Text = HttpUtility.HtmlEncode(response.Exception.Message);
				} else {
					errorDetails.Text = "Authentication status: " + response.Status.ToString();
				}
			}
		}
	}
}
