using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web;
using System.Globalization;
using System.Web.UI;
using System.Net;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedReturnToHtmlResourceName, "text/html")]

namespace DotNetOpenId.RelyingParty {
	public class OpenIdAjaxTextBox : WebControl, ICallbackEventHandler {
		internal const string EmbeddedScriptResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxTextBox.js";
		internal const string EmbeddedReturnToHtmlResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxReturnToForwarder.htm";

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdAjaxTextBox), EmbeddedScriptResourceName);
			string callbackMethod = Page.ClientScript.GetCallbackEventReference(this, "document.getElementsByName('openid_identifier')[0].value", "discoveryResult", null, true);
			Page.ClientScript.RegisterClientScriptBlock(GetType(), "callbackMethod", string.Format(CultureInfo.InvariantCulture, @"
<script language='javascript'>
	function performDiscovery() {{
		{0};
	}}
</script>", callbackMethod));
			Page.ClientScript.RegisterStartupScript(GetType(), "ajaxstartup", @"
<script language='javascript'>
ajaxOnLoad();
</script>");
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {

			string logoUrl = Page.ClientScript.GetWebResourceUrl(
				typeof(OpenIdTextBox), OpenIdTextBox.EmbeddedLogoResourceName);

			//writer.Write("<input name='openid_identifier' />");

			writer.WriteBeginTag("input");
			writer.WriteAttribute("name", "openid_identifier");
			writer.Write(" style='");
			writer.WriteStyleAttribute("background", string.Format(CultureInfo.InvariantCulture,
					"url({0}) no-repeat", HttpUtility.HtmlEncode(logoUrl)));
			writer.WriteStyleAttribute("background-position", "0 50%");
			writer.WriteStyleAttribute("padding-left", "18px");
			writer.WriteStyleAttribute("border-style", "solid");
			writer.WriteStyleAttribute("border-width", "1px");
			writer.WriteStyleAttribute("border-color", "lightgray");
			writer.Write("'");
			writer.Write(" />");
		}

		#region ICallbackEventHandler Members

		string callbackResult;

		public string GetCallbackResult() {
			return callbackResult;
		}

		public void RaiseCallbackEvent(string eventArgument) {
			OpenIdRelyingParty rp = new OpenIdRelyingParty();
			Realm realm = new Uri(Page.Request.Url, Page.Request.ApplicationPath);
			Uri return_to = new Uri(Page.Request.Url,
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedReturnToHtmlResourceName));
			IAuthenticationRequest req = rp.CreateRequest(eventArgument, realm, return_to);
			req.Mode = AuthenticationRequestMode.Immediate;
			callbackResult = req.RedirectingResponse.Headers[HttpResponseHeader.Location];
		}

		#endregion
	}
}
