//-----------------------------------------------------------------------
// <copyright file="RelyingPartyUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;

	public static class RelyingPartyUtilities {
		public static string OpenIdAjaxTextBox(this HtmlHelper helper, string name) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(name));

			var box = new OpenIdAjaxTextBox();
			box.Name = name;

			StringWriter sw = new StringWriter();
			HtmlTextWriter writer = new HtmlTextWriter(sw);
			box.RenderControl(writer);
			return sw.ToString();
		}

		/// <summary>
		/// Performs discovery on some identifier on behalf of Javascript running on the browser.
		/// </summary>
		/// <param name="identifier">The identifier on which to perform discovery.</param>
		/// <param name="realm">The realm of the relying party.</param>
		/// <param name="returnTo">The URL that receives OpenID responses.</param>
		/// <returns>The JSON result to return to the user agent.</returns>
		/// <remarks>
		/// We prepare a JSON object with this interface:
		/// class jsonResponse {
		///    string claimedIdentifier;
		///    Array requests; // never null
		///    string error; // null if no error
		/// }
		/// Each element in the requests array looks like this:
		/// class jsonAuthRequest {
		///    string endpoint;  // URL to the OP endpoint
		///    string immediate; // URL to initiate an immediate request
		///    string setup;     // URL to initiate a setup request.
		/// }
		/// </remarks>
		public static JsonResult AjaxDiscover(Identifier identifier, Realm realm, Uri returnTo, Action<IAuthenticationRequest> attachExtensions) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(realm != null);
			Contract.Requires<ArgumentNullException>(returnTo != null);

			OpenIdSelector selector = new OpenIdSelector();
			selector.RealmUrl = realm;
			selector.ReturnToUrl = returnTo.AbsoluteUri;
			selector.ID = "MVC";
			var requests = selector.CreateRequests(identifier).CacheGeneratedResults();
			if (requests.Any()) {
				if (attachExtensions != null) {
					foreach (var request in requests) {
						attachExtensions(request);
					}
				}
				return new JsonResult {
					Data = new {
						claimedIdentifier = requests.First().ClaimedIdentifier,
						requests = requests.Select(req => new {
							endpoint = req.Provider.Uri.AbsoluteUri,
							immediate = GetRedirectUrl(selector.RelyingParty, req, true),
							setup = GetRedirectUrl(selector.RelyingParty, req, false),
						}).ToArray()
					},
				};
			} else {
				return new JsonResult {
					Data = new {
						requests = new object[0],
						error = "No OpenID endpoint found",
					}
				};
			}
		}

		public static ActionResult AjaxReturnTo(HttpRequestBase request) {
			Contract.Requires<ArgumentNullException>(request != null);

			Logger.OpenId.DebugFormat("AJAX (iframe) callback from OP: {0}", request.Url);
			string extensionsJson = null;
			var relyingPartyNonVerifying = OpenIdRelyingParty.CreateNonVerifying();
			var authResponse = relyingPartyNonVerifying.GetResponse();
			Logger.Controls.DebugFormat(
				"The MVC controller checked for an authentication response from a popup window or iframe using a non-verifying RP and found: {0}",
				authResponse.Status);
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				//this.OnUnconfirmedPositiveAssertion(); // event handler will fill the clientScriptExtensions collection.
				var extensionsDictionary = new Dictionary<string, string>();
				//foreach (var pair in this.clientScriptExtensions) {
				//    IClientScriptExtensionResponse extension = (IClientScriptExtensionResponse)authResponse.GetExtension(pair.Key);
				//    if (extension == null) {
				//        continue;
				//    }
				//    var positiveResponse = (PositiveAuthenticationResponse)authResponse;
				//    string js = extension.InitializeJavaScriptData(positiveResponse.Response);
				//    if (!string.IsNullOrEmpty(js)) {
				//        extensionsDictionary[pair.Value] = js;
				//    }
				//}

				extensionsJson = MessagingUtilities.CreateJsonObject(extensionsDictionary, true);
			}

			string payload = "document.URL";
			if (request.HttpMethod == "POST") {
				// Promote all form variables to the query string, but since it won't be passed
				// to any server (this is a javascript window-to-window transfer) the length of
				// it can be arbitrarily long, whereas it was POSTed here probably because it
				// was too long for HTTP transit.
				UriBuilder payloadUri = new UriBuilder(request.Url);
				payloadUri.AppendQueryArgs(request.Form.ToDictionary());
				payload = MessagingUtilities.GetSafeJavascriptValue(payloadUri.Uri.AbsoluteUri);
			}

			if (!string.IsNullOrEmpty(extensionsJson)) {
				payload += ", " + extensionsJson;
			}

			return CallbackUserAgentMethod("dnoa_internal.processAuthorizationResult(" + payload + ")");
		}

		private static Uri GetRedirectUrl(OpenIdRelyingParty rp, IAuthenticationRequest request, bool immediate) {
			request.Mode = immediate ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
			return request.RedirectingResponse.GetDirectUriRequest(rp.Channel);
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private static ActionResult CallbackUserAgentMethod(string methodCall) {
			Logger.OpenId.DebugFormat("Sending Javascript callback: {0}", methodCall);
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<html><body><script type='text/javascript' language='javascript'><!--");
			builder.AppendLine("//<![CDATA[");
			builder.Append(@"	var inPopup = !window.frameElement;
	var objSrc = inPopup ? window.opener : window.frameElement;
");

			// Something about calling objSrc.{0} can somehow cause FireFox to forget about the inPopup variable,
			// so we have to actually put the test for it ABOVE the call to objSrc.{0} so that it already 
			// whether to call window.self.close() after the call.
			string htmlFormat = @"	if (inPopup) {{
		objSrc.{0};
		window.self.close();
	}} else {{
		objSrc.{0};
	}}";
			builder.AppendFormat(CultureInfo.InvariantCulture, htmlFormat, methodCall);
			builder.AppendLine("//]]>--></script>");
			builder.AppendLine("</body></html>");
			return new ContentResult { Content = builder.ToString(), ContentType = "text/html" };
		}
	}
}
