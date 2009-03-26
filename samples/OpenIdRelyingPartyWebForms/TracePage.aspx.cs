namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Collections.Generic;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	public partial class TracePage : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.placeHolder1.Controls.Add(new Label { Text = HttpUtility.HtmlEncode(Global.LogMessages.ToString()) });
		}

		protected void clearLogButton_Click(object sender, EventArgs e) {
			Global.LogMessages.Length = 0;

			// clear the page immediately, and allow for F5 without a Postback warning.
			this.Response.Redirect(this.Request.Url.AbsoluteUri);
		}
	}
}
