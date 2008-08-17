using System;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedReturnToHtmlResourceName, "text/html")]

namespace DotNetOpenId.RelyingParty {
	public class OpenIdAjaxTextBox : WebControl, ICallbackEventHandler {
		internal const string EmbeddedScriptResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxTextBox.js";
		internal const string EmbeddedReturnToHtmlResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxReturnToForwarder.htm";

		public IAuthenticationResponse AuthenticationResponse { get; private set; }

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

			if (Page.IsPostBack) {
				string authData = Page.Request.Form["openidAuthData"];
				if (!string.IsNullOrEmpty(authData)) {
					var authDataFields = HttpUtility.ParseQueryString(authData);
					// We won't use the actual request URL of this request because
					// the request we pass in must match the return_to value we gave
					// before, or else verification will throw a return_to-request mismatch error.
					Uri returnTo = authDataFields[Protocol.Default.openid.return_to] != null ?
						new Uri(authDataFields[Protocol.Default.openid.return_to]) : getAjaxReturnTo();
					var rp = new OpenIdRelyingParty(OpenIdRelyingParty.HttpApplicationStore,
						returnTo, authDataFields);
					AuthenticationResponse = rp.Response;
				}
			}
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
			Uri return_to = getAjaxReturnTo();

			string setupUrl, immediateUrl;

			IAuthenticationRequest req = rp.CreateRequest(eventArgument, realm, return_to);
			setupUrl = req.RedirectingResponse.Headers[HttpResponseHeader.Location];
			req.Mode = AuthenticationRequestMode.Immediate;
			immediateUrl = req.RedirectingResponse.Headers[HttpResponseHeader.Location];

			callbackResult = immediateUrl + " " + setupUrl;
		}

		private Uri getAjaxReturnTo() {
			Uri return_to = new Uri(Page.Request.Url,
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedReturnToHtmlResourceName));
			return return_to;
		}

		#endregion
	}
}
