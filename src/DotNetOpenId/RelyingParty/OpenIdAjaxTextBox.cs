using System;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedDotNetOpenIdLogoResourceName, "image/gif")]

namespace DotNetOpenId.RelyingParty {
	public class OpenIdAjaxTextBox : WebControl {
		internal const string EmbeddedScriptResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxTextBox.js";
		internal const string EmbeddedDotNetOpenIdLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.dotnetopenid_16x16.gif";

		public IAuthenticationResponse AuthenticationResponse { get; private set; }

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdAjaxTextBox), EmbeddedScriptResourceName);
			Page.ClientScript.RegisterStartupScript(GetType(), "ajaxstartup", string.Format(CultureInfo.InvariantCulture, @"
<script language='javascript'>
var dotnetopenid_logo_url = '{0}';
initAjaxOpenId(document.getElementsByName('openid_identifier')[0]);
</script>", Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedDotNetOpenIdLogoResourceName)));

			if (Page.IsPostBack) {
				string authData = Page.Request.Form["openidAuthData"];
				if (!string.IsNullOrEmpty(authData)) {
					var authDataFields = HttpUtility.ParseQueryString(authData);
					Uri returnTo = Util.GetRequestUrlFromContext();
					var rp = new OpenIdRelyingParty(OpenIdRelyingParty.HttpApplicationStore,
						returnTo, authDataFields);
					AuthenticationResponse = rp.Response;
				}
			} else {
				string userSuppliedIdentifier = Page.Request.QueryString["dotnetopenid.userSuppliedIdentifier"];
				if (!string.IsNullOrEmpty(userSuppliedIdentifier)) {
					if (Page.Request.QueryString["dotnetopenid.phase"] == "2") {
						callbackUserAgentMethod("openidAuthResult(document.URL)");
					} else {
						OpenIdRelyingParty rp = new OpenIdRelyingParty();

						try {
							IAuthenticationRequest req = rp.CreateRequest(userSuppliedIdentifier);
							req.AddCallbackArguments("dotnetopenid.phase", "2");
							req.Mode = AuthenticationRequestMode.Immediate;
							req.RedirectToProvider();
						} catch (OpenIdException ex) {
							callbackUserAgentMethod("openidDiscoveryFailure('" + ex.Message.Replace("'", "\\'") + "')");
						}
					}
				}
			}
		}

		private void callbackUserAgentMethod(string methodCall) {
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture,
				"<html><body><script language='javascript'>window.frameElement.openidBox.{0};</script></body></html>", methodCall));
			Page.Response.End();
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {

			string logoUrl = Page.ClientScript.GetWebResourceUrl(
				typeof(OpenIdTextBox), OpenIdTextBox.EmbeddedLogoResourceName);

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
	}
}
