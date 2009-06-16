//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyAjaxControlBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.EmbeddedAjaxJavascriptResource, "text/javascript")]

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Drawing.Design;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.UI;

	/// <summary>
	/// A common base class for OpenID Relying Party controls.
	/// </summary>
	internal abstract class OpenIdRelyingPartyAjaxControlBase : OpenIdRelyingPartyControlBase, ICallbackEventHandler {
		/// <summary>
		/// The manifest resource name of the javascript file to include on the hosting page.
		/// </summary>
		internal const string EmbeddedAjaxJavascriptResource = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.js";

		/// <summary>
		/// The name of the javascript function that will initiate a synchronous callback.
		/// </summary>
		protected const string CallbackJsFunction = "window.dnoa_internal.callback";

		/// <summary>
		/// The name of the javascript function that will initiate an asynchronous callback.
		/// </summary>
		protected const string CallbackJsFunctionAsync = "window.dnoa_internal.callbackAsync";

		/// <summary>
		/// Stores the result of a AJAX callback discovery.
		/// </summary>
		private string discoveryResult;

		/// <summary>
		/// A dictionary of extension response types and the javascript member 
		/// name to map them to on the user agent.
		/// </summary>
		private Dictionary<Type, string> clientScriptExtensions = new Dictionary<Type, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyAjaxControlBase"/> class.
		/// </summary>
		protected OpenIdRelyingPartyAjaxControlBase() {
			// The AJAX login style always uses popups (or invisible iframes).
			this.Popup = PopupBehavior.Always;
		}

		/// <summary>
		/// Gets or sets a value indicating when to use a popup window to complete the login experience.
		/// </summary>
		/// <value>The default value is <see cref="PopupBehavior.Never"/>.</value>
		[Bindable(false), Browsable(false)]
		public override PopupBehavior Popup {
			get { return base.Popup; }
			set { ErrorUtilities.VerifySupported(value == base.Popup, OpenIdStrings.PropertyValueNotSupported); }
		}

		#region ICallbackEventHandler Members

		/// <summary>
		/// Returns the result of discovery on some Identifier passed to <see cref="ICallbackEventHandler.RaiseCallbackEvent"/>.
		/// </summary>
		/// <returns>The result of the callback.</returns>
		/// <value>A whitespace delimited list of URLs that can be used to initiate authentication.</value>
		string ICallbackEventHandler.GetCallbackResult() {
			this.Page.Response.ContentType = "text/javascript";
			return this.discoveryResult;
		}

		/// <summary>
		/// Performs discovery on some OpenID Identifier.  Called directly from the user agent via
		/// AJAX callback mechanisms.
		/// </summary>
		/// <param name="eventArgument">The identifier to perform discovery on.</param>
		void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument) {
			string userSuppliedIdentifier = eventArgument;

			ErrorUtilities.VerifyNonZeroLength(userSuppliedIdentifier, "userSuppliedIdentifier");
			Logger.OpenId.InfoFormat("AJAX discovery on {0} requested.", userSuppliedIdentifier);

			// We prepare a JSON object with this interface:
			// class jsonResponse {
			//    string claimedIdentifier;
			//    Array requests; // never null
			//    string error; // null if no error
			// }
			// Each element in the requests array looks like this:
			// class jsonAuthRequest {
			//    string endpoint;  // URL to the OP endpoint
			//    string immediate; // URL to initiate an immediate request
			//    string setup;     // URL to initiate a setup request.
			// }
			StringBuilder discoveryResultBuilder = new StringBuilder();
			discoveryResultBuilder.Append("{");
			try {
				this.Identifier = userSuppliedIdentifier;
				IEnumerable<IAuthenticationRequest> requests = this.CreateRequests().CacheGeneratedResults();
				if (requests.Any()) {
					discoveryResultBuilder.AppendFormat("claimedIdentifier: {0},", MessagingUtilities.GetSafeJavascriptValue(requests.First().ClaimedIdentifier));
					discoveryResultBuilder.Append("requests: [");
					foreach (IAuthenticationRequest request in requests) {
						this.OnLoggingIn(request);
						discoveryResultBuilder.Append("{");
						discoveryResultBuilder.AppendFormat("endpoint: {0},", MessagingUtilities.GetSafeJavascriptValue(request.Provider.Uri.AbsoluteUri));
						request.Mode = AuthenticationRequestMode.Immediate;
						OutgoingWebResponse response = request.RedirectingResponse;
						discoveryResultBuilder.AppendFormat("immediate: {0},", MessagingUtilities.GetSafeJavascriptValue(response.GetDirectUriRequest(this.RelyingParty.Channel).AbsoluteUri));
						request.Mode = AuthenticationRequestMode.Setup;
						response = request.RedirectingResponse;
						discoveryResultBuilder.AppendFormat("setup: {0}", MessagingUtilities.GetSafeJavascriptValue(response.GetDirectUriRequest(this.RelyingParty.Channel).AbsoluteUri));
						discoveryResultBuilder.Append("},");
					}
					discoveryResultBuilder.Length -= 1; // trim off last comma
					discoveryResultBuilder.Append("]");
				} else {
					discoveryResultBuilder.Append("requests: new Array(),");
					discoveryResultBuilder.AppendFormat("error: {0}", MessagingUtilities.GetSafeJavascriptValue(OpenIdStrings.OpenIdEndpointNotFound));
				}
			} catch (ProtocolException ex) {
				discoveryResultBuilder.Append("requests: new Array(),");
				discoveryResultBuilder.AppendFormat("error: {0}", MessagingUtilities.GetSafeJavascriptValue(ex.Message));
			}
			discoveryResultBuilder.Append("}");
			this.discoveryResult = discoveryResultBuilder.ToString();
		}

		#endregion

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <returns>A sequence of authentication requests, any one of which may be 
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier"/>.</returns>
		protected override IEnumerable<IAuthenticationRequest> CreateRequests() {
			Contract.Requires(this.Identifier != null, OpenIdStrings.NoIdentifierSet);
			ErrorUtilities.VerifyOperation(this.Identifier != null, OpenIdStrings.NoIdentifierSet);

			// We delegate all our logic to another method, since invoking base. methods
			// within an iterator method results in unverifiable code.
			return this.CreateRequestsCore(base.CreateRequests());
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdRelyingPartyAjaxControlBase), EmbeddedAjaxJavascriptResource);

			StringBuilder initScript = new StringBuilder();

			initScript.AppendLine(CallbackJsFunctionAsync + " = " + this.GetJsCallbackConvenienceFunction(true));
			initScript.AppendLine(CallbackJsFunction + " = " + this.GetJsCallbackConvenienceFunction(false));

			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyControlBase), "initializer", initScript.ToString(), true);
		}

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="requests">The authentication requests to prepare.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier"/>.
		/// </returns>
		private IEnumerable<IAuthenticationRequest> CreateRequestsCore(IEnumerable<IAuthenticationRequest> requests) {
			Contract.Requires(requests != null);

			// Configure each generated request.
			int reqIndex = 0;
			foreach (var req in requests) {
				req.AddCallbackArguments("index", (reqIndex++).ToString(CultureInfo.InvariantCulture));

				if (req.Provider.IsExtensionSupported<UIRequest>()) {
					// Provide a hint for the client javascript about whether the OP supports the UI extension.
					// This is so the window can be made the correct size for the extension.
					// If the OP doesn't advertise support for the extension, the javascript will use
					// a bigger popup window.
					req.AddCallbackArguments("dotnetopenid.popupUISupported", "1");
				}

				// If the ReturnToUrl was explicitly set, we'll need to reset our first parameter
				if (string.IsNullOrEmpty(HttpUtility.ParseQueryString(req.ReturnToUrl.Query)["dotnetopenid.userSuppliedIdentifier"])) {
					req.AddCallbackArguments("dotnetopenid.userSuppliedIdentifier", this.Identifier);
				}

				// Our javascript needs to let the user know which endpoint responded.  So we force it here.
				// This gives us the info even for 1.0 OPs and 2.0 setup_required responses.
				req.AddCallbackArguments("dotnetopenid.op_endpoint", req.Provider.Uri.AbsoluteUri);
				req.AddCallbackArguments("dotnetopenid.claimed_id", (string)req.ClaimedIdentifier ?? string.Empty);

				// We append a # at the end so that if the OP happens to support it,
				// the OpenID response "query string" is appended after the hash rather than before, resulting in the
				// browser being super-speedy in closing the popup window since it doesn't try to pull a newer version
				// of the static resource down from the server merely because of a changed URL.
				// http://www.nabble.com/Re:-Defining-how-OpenID-should-behave-with-fragments-in-the-return_to-url-p22694227.html
				////TODO:

				yield return req;
			}
		}

		/// <summary>
		/// Constructs a function that will initiate an AJAX callback.
		/// </summary>
		/// <param name="async">if set to <c>true</c> causes the AJAX callback to be a little more asynchronous.  Note that <c>false</c> does not mean the call is absolutely synchronous.</param>
		/// <returns>The string defining a javascript anonymous function that initiates a callback.</returns>
		private string GetJsCallbackConvenienceFunction(bool async) {
			string argumentParameterName = "argument";
			string callbackResultParameterName = "resultFunction";
			string callbackErrorCallbackParameterName = "errorCallback";
			string callback = Page.ClientScript.GetCallbackEventReference(
				this,
				argumentParameterName,
				callbackResultParameterName,
				argumentParameterName,
				callbackErrorCallbackParameterName,
				async);
			return string.Format(
				CultureInfo.InvariantCulture,
				"function({1}, {2}, {3}) {{{0}\treturn {4};{0}}};",
				Environment.NewLine,
				argumentParameterName,
				callbackResultParameterName,
				callbackErrorCallbackParameterName,
				callback);
		}

		/// <summary>
		/// Notifies the user agent via an AJAX response of a completed authentication attempt.
		/// </summary>
		private void ReportAuthenticationResult() {
			Logger.OpenId.InfoFormat("AJAX (iframe) callback from OP: {0}", this.Page.Request.Url);
			List<string> assignments = new List<string>();

			var authResponse = this.RelyingParty.GetResponse();
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				this.OnLoggedIn(authResponse);
				foreach (var pair in this.clientScriptExtensions) {
					IClientScriptExtensionResponse extension = (IClientScriptExtensionResponse)authResponse.GetExtension(pair.Key);
					if (extension == null) {
						continue;
					}
					var positiveResponse = (PositiveAuthenticationResponse)authResponse;
					string js = extension.InitializeJavaScriptData(positiveResponse.Response);
					if (string.IsNullOrEmpty(js)) {
						js = "null";
					}
					assignments.Add(pair.Value + " = " + js);
				}
			}

			this.CallbackUserAgentMethod("dnoi_internal.processAuthorizationResult(document.URL)", assignments.ToArray());
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private void CallbackUserAgentMethod(string methodCall) {
			this.CallbackUserAgentMethod(methodCall, null);
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		/// <param name="preAssignments">An optional list of assignments to make to the input box object before placing the method call.</param>
		private void CallbackUserAgentMethod(string methodCall, string[] preAssignments) {
			Logger.OpenId.InfoFormat("Sending Javascript callback: {0}", methodCall);
			Page.Response.Write(@"<html><body><script language='javascript'>
	var inPopup = !window.frameElement;
	var objSrc = inPopup ? window.opener.waiting_openidBox : window.frameElement.openidBox;
");
			if (preAssignments != null) {
				foreach (string assignment in preAssignments) {
					Page.Response.Write(string.Format(CultureInfo.InvariantCulture, "	objSrc.{0};\n", assignment));
				}
			}

			// Something about calling objSrc.{0} can somehow cause FireFox to forget about the inPopup variable,
			// so we have to actually put the test for it ABOVE the call to objSrc.{0} so that it already 
			// whether to call window.self.close() after the call.
			string htmlFormat = @"	if (inPopup) {{
	objSrc.{0};
	window.self.close();
}} else {{
	objSrc.{0};
}}
</script></body></html>";
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture, htmlFormat, methodCall));
			Page.Response.End();
		}
	}
}
