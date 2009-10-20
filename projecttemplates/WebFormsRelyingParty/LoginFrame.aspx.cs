using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace WebFormsRelyingParty {
	public partial class LoginFrame : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void openIdButtonPanel_LoggedIn(object sender, OpenIdEventArgs e) {
			FormsAuthentication.RedirectFromLoginPage(e.ClaimedIdentifier, false);
		}
	}
}
