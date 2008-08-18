using System;
using System.Collections.Specialized;
using System.Globalization;
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
var openidbox = document.getElementsByName('openid_identifier')[0];
if (openidbox) {{ initAjaxOpenId(openidbox); }}
</script>", Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedDotNetOpenIdLogoResourceName)));

			if (Page.IsPostBack) {
				string authData = Page.Request.Form["openidAuthData"];
				if (!string.IsNullOrEmpty(authData)) {
					var authDataFields = HttpUtility.ParseQueryString(authData);
					// Lie about the request URL so it matches the return_to URL made
					// back when this authentication occurred.
					Uri returnTo = getReturnTo(Util.GetRequestUrlFromContext(), authDataFields);
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
							if (Page.Request.QueryString["dotnetopenid.immediate"] == "true") {
								req.Mode = AuthenticationRequestMode.Immediate;
							}
							req.RedirectToProvider();
						} catch (OpenIdException ex) {
							callbackUserAgentMethod("openidDiscoveryFailure('" + ex.Message.Replace("'", "\\'") + "')");
						}
					}
				}
			}
		}

		private Uri getReturnTo(Uri uri, System.Collections.Specialized.NameValueCollection authDataFields) {
			UriBuilder builder = new UriBuilder(uri);
			NameValueCollection returnToNVC = HttpUtility.ParseQueryString(builder.Query);
			foreach (string key in authDataFields) {
				if (returnToNVC[key] == null) {
					returnToNVC[key] = authDataFields[key];
				} else {
					if (returnToNVC[key] != authDataFields[key]) {
						throw new ArgumentException(Strings.ReturnToParamDoesNotMatchRequestUrl, key);
					}
				}
			}
			builder.Query = UriUtil.CreateQueryString(returnToNVC);
			return builder.Uri;
		}

		private void callbackUserAgentMethod(string methodCall) {
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture,
				@"<html><body><script language='javascript'>
					var objSrc = window.frameElement ? window.frameElement.openidBox : window.opener.waiting_openidBox;
					objSrc.{0};
					if (!window.frameElement) {{ self.close(); }}
				</script></body></html>", methodCall));
			Page.Response.End();
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {

			string logoUrl = Page.ClientScript.GetWebResourceUrl(
				typeof(OpenIdTextBox), OpenIdTextBox.EmbeddedLogoResourceName);

			writer.WriteBeginTag("span");
			writer.Write(" style='");
			writer.WriteStyleAttribute("position", "relative");
			writer.Write("'>");

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

			writer.WriteEndTag("span");
		}
	}
}
