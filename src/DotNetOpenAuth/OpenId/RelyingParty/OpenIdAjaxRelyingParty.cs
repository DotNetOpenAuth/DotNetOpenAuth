//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Mime;
	using System.Text;
	using System.Web;
	using System.Web.Script.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.UI;

	/// <summary>
	/// Provides the programmatic facilities to act as an AJAX-enabled OpenID relying party.
	/// </summary>
	public class OpenIdAjaxRelyingParty : OpenIdRelyingParty {
		/// <summary>
		/// The <see cref="OpenIdRelyingParty"/> instance used to process authentication responses
		/// without verifying the assertion or consuming nonces.
		/// </summary>
		private OpenIdRelyingParty nonVerifyingRelyingParty = OpenIdRelyingParty.CreateNonVerifying();

		/// <summary>
		/// A dictionary of extension response types and the javascript member 
		/// name to map them to on the user agent.
		/// </summary>
		private Dictionary<Type, string> clientScriptExtensions = new Dictionary<Type, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdAjaxRelyingParty"/> class.
		/// </summary>
		public OpenIdAjaxRelyingParty() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdAjaxRelyingParty"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store.  If <c>null</c>, the relying party will always operate in "dumb mode".</param>
		public OpenIdAjaxRelyingParty(IRelyingPartyApplicationStore applicationStore)
			: base(applicationStore) {
		}

		/// <summary>
		/// Performs discovery on some identifier on behalf of Javascript running on the browser.
		/// </summary>
		/// <param name="requests">The identifier discovery results to serialize as a JSON response.</param>
		/// <returns>
		/// The JSON result to return to the user agent.
		/// </returns>
		/// <remarks>
		/// We prepare a JSON object with this interface:
		/// <code>
		/// class jsonResponse {
		///    string claimedIdentifier;
		///    Array requests; // never null
		///    string error; // null if no error
		/// }
		/// </code>
		/// Each element in the requests array looks like this:
		/// <code>
		/// class jsonAuthRequest {
		///    string endpoint;  // URL to the OP endpoint
		///    string immediate; // URL to initiate an immediate request
		///    string setup;     // URL to initiate a setup request.
		/// }
		/// </code>
		/// </remarks>
		public OutgoingWebResponse AsAjaxDiscoveryResult(IEnumerable<IAuthenticationRequest> requests) {
			Contract.Requires<ArgumentNullException>(requests != null);

			string json = this.AsJsonDiscoveryResult(this.AsPopups(requests));
			OutgoingWebResponse response = new OutgoingWebResponse();
			response.Body = json;
			return response;
		}

		/// <summary>
		/// Allows an OpenID extension to read data out of an unverified positive authentication assertion
		/// and send it down to the client browser so that Javascript running on the page can perform
		/// some preprocessing on the extension data.
		/// </summary>
		/// <typeparam name="T">The extension <i>response</i> type that will read data from the assertion.</typeparam>
		/// <param name="propertyName">The property name on the openid_identifier input box object that will be used to store the extension data.  For example: sreg</param>
		/// <remarks>
		/// This method should be called before <see cref="ProcessAjaxOpenIdResponse()"/>.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
		public void RegisterClientScriptExtension<T>(string propertyName) where T : IClientScriptExtensionResponse {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(propertyName));
			ErrorUtilities.VerifyArgumentNamed(!this.clientScriptExtensions.ContainsValue(propertyName), "propertyName", OpenIdStrings.ClientScriptExtensionPropertyNameCollision, propertyName);
			foreach (var ext in this.clientScriptExtensions.Keys) {
				ErrorUtilities.VerifyArgument(ext != typeof(T), OpenIdStrings.ClientScriptExtensionTypeCollision, typeof(T).FullName);
			}
			this.clientScriptExtensions.Add(typeof(T), propertyName);
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <returns>The HTTP response to send to this HTTP request.</returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public OutgoingWebResponse ProcessAjaxOpenIdResponse() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			return this.ProcessAjaxOpenIdResponse(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The incoming HTTP request that is expected to carry an OpenID authentication response.</param>
		/// <returns>The HTTP response to send to this HTTP request.</returns>
		public OutgoingWebResponse ProcessAjaxOpenIdResponse(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);

			string extensionsJson = null;
			var authResponse = this.nonVerifyingRelyingParty.GetResponse();
			ErrorUtilities.VerifyProtocol(authResponse != null, "OpenID popup window or iframe did not recognize an OpenID response in the request.");
			Logger.OpenId.DebugFormat("AJAX (iframe) callback from OP: {0}", request.Url);
			Logger.Controls.DebugFormat(
				"The MVC controller checked for an authentication response from a popup window or iframe using a non-verifying RP and found: {0}",
				authResponse.Status);
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				var extensionsDictionary = new Dictionary<string, string>();
				foreach (var pair in this.clientScriptExtensions) {
					IClientScriptExtensionResponse extension = (IClientScriptExtensionResponse)authResponse.GetExtension(pair.Key);
					if (extension == null) {
						continue;
					}
					var positiveResponse = (PositiveAuthenticationResponse)authResponse;
					string js = extension.InitializeJavaScriptData(positiveResponse.Response);
					if (!string.IsNullOrEmpty(js)) {
						extensionsDictionary[pair.Value] = js;
					}
				}

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

			return InvokeParentPageScript("dnoa_internal.processAuthorizationResult(" + payload + ")");
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			this.nonVerifyingRelyingParty.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>
		/// Invokes a method on a parent frame or window and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the parent window, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		/// <returns>The entire HTTP response to send to the popup window or iframe to perform the invocation.</returns>
		private static OutgoingWebResponse InvokeParentPageScript(string methodCall) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(methodCall));

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

			var response = new OutgoingWebResponse();
			response.Body = builder.ToString();
			response.Headers.Add(HttpResponseHeader.ContentType, new ContentType("text/html").ToString());
			return response;
		}

		/// <summary>
		/// Prepares authentication requests for use as AJAX-initiated logins.
		/// </summary>
		/// <param name="requests">The authentication requests to prepare for AJAX.</param>
		/// <returns>The AJAX-ified authentication requests</returns>
		/// <remarks>
		/// The original requests are altered as part of executing this method.
		/// </remarks>
		private IEnumerable<IAuthenticationRequest> AsPopups(IEnumerable<IAuthenticationRequest> requests) {
			Contract.Requires<ArgumentNullException>(requests != null);

			// Some OPs may be listed multiple times (one with HTTPS and the other with HTTP, for example).
			// Since we're gathering OPs to try one after the other, just take the first choice of each OP
			// and don't try it multiple times.
			requests = requests.Distinct(DuplicateRequestedHostsComparer.Instance);

			// Configure each generated request.
			foreach (var req in requests) {
				// Inform ourselves in return_to that we're in a popup.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyControlBase.UIPopupCallbackKey, "1");

				if (req.DiscoveryResult.IsExtensionSupported<UIRequest>()) {
					// Inform the OP that we'll be using a popup window consistent with the UI extension.
					req.AddExtension(new UIRequest());

					// Provide a hint for the client javascript about whether the OP supports the UI extension.
					// This is so the window can be made the correct size for the extension.
					// If the OP doesn't advertise support for the extension, the javascript will use
					// a bigger popup window.
					req.SetUntrustedCallbackArgument(OpenIdRelyingPartyControlBase.PopupUISupportedJSHint, "1");
				}

				yield return req;
			}
		}

		/// <summary>
		/// Converts a sequence of authentication requests to a JSON object for seeding an AJAX-enabled login page.
		/// </summary>
		/// <param name="requests">The authentication requests.</param>
		/// <returns>A JSON string.</returns>
		private string AsJsonDiscoveryResult(IEnumerable<IAuthenticationRequest> requests) {
			requests = requests.CacheGeneratedResults();

			// Configure each generated request.
			int reqIndex = 0;
			foreach (var req in requests) {
				req.SetUntrustedCallbackArgument("index", (reqIndex++).ToString(CultureInfo.InvariantCulture));

				// If the ReturnToUrl was explicitly set, we'll need to reset our first parameter
				if (string.IsNullOrEmpty(HttpUtility.ParseQueryString(req.ReturnToUrl.Query)[AuthenticationRequest.UserSuppliedIdentifierParameterName])) {
					Identifier userSuppliedIdentifier = ((AuthenticationRequest)req).DiscoveryResult.UserSuppliedIdentifier;
					req.SetUntrustedCallbackArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName, userSuppliedIdentifier.OriginalString);
				}

				// Our javascript needs to let the user know which endpoint responded.  So we force it here.
				// This gives us the info even for 1.0 OPs and 2.0 setup_required responses.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.OPEndpointParameterName, req.Provider.Uri.AbsoluteUri);
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.ClaimedIdParameterName, (string)req.ClaimedIdentifier ?? string.Empty);

				// Inform ourselves in return_to that we're in a popup or iframe.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.UIPopupCallbackKey, "1");
			}

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			string json;
			if (requests.Any()) {
				json = serializer.Serialize(new {
					claimedIdentifier = requests.First().ClaimedIdentifier,
					requests = requests.Select(req => new {
						endpoint = req.Provider.Uri.AbsoluteUri,
						immediate = this.GetRedirectUrl(req, true),
						setup = this.GetRedirectUrl(req, false),
					}).ToArray()
				});
			} else {
				json = serializer.Serialize(new {
					requests = new object[0],
					error = OpenIdStrings.OpenIdEndpointNotFound,
				});
			}

			return json;
		}

		/// <summary>
		/// Gets the full URL that carries an OpenID message, even if it exceeds the normal maximum size of a URL,
		/// for purposes of sending to an AJAX component running in the browser.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="immediate"><c>true</c>to create a checkid_immediate request;
		/// <c>false</c> to create a checkid_setup request.</param>
		/// <returns>The absolute URL that carries the entire OpenID message.</returns>
		private Uri GetRedirectUrl(IAuthenticationRequest request, bool immediate) {
			Contract.Requires<ArgumentNullException>(request != null);

			request.Mode = immediate ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
			return request.RedirectingResponse.GetDirectUriRequest(this.Channel);
		}
	}
}
