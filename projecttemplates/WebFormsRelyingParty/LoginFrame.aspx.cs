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
		private const string postLoginAssertionMethodName = "postLoginAssertion";

		protected void Page_Load(object sender, EventArgs e) {
			const string positiveAssertionParameterName = "positiveAssertion";
			const string originPageParameterName = "originPage";
			string script = string.Format(
				CultureInfo.InvariantCulture,
@"window.{2} = function({0}, {3}) {{
	$('#OpenIDForm')[0].target = '_top';
	$('#originPage')[0].setAttribute('value', {3});
	{1};
}};",
				positiveAssertionParameterName,
				this.ClientScript.GetPostBackEventReference(this, null, false),
				postLoginAssertionMethodName,
				originPageParameterName);
			this.ClientScript.RegisterClientScriptBlock(this.GetType(), "Postback", script, true);
		}
	}
}
