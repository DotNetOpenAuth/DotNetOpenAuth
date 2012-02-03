namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	/// <summary>
	/// A page to display recent log messages.
	/// </summary>
	public partial class TracePage : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.placeHolder1.Controls.Add(new Label { Text = HttpUtility.HtmlEncode(Logging.LogMessages.ToString()) });
		}

		protected void clearLogButton_Click(object sender, EventArgs e) {
			Logging.LogMessages.Length = 0;

			// clear the page immediately, and allow for F5 without a Postback warning.
			Response.Redirect(Request.Url.AbsoluteUri);
		}
	}
}