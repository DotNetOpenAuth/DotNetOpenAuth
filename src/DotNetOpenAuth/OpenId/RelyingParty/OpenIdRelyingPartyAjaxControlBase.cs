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
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;

	/// <summary>
	/// A common base class for OpenID Relying Party controls.
	/// </summary>
	public abstract class OpenIdRelyingPartyAjaxControlBase : OpenIdRelyingPartyControlBase, ICallbackEventHandler {
		/// <summary>
		/// The manifest resource name of the javascript file to include on the hosting page.
		/// </summary>
		internal const string EmbeddedAjaxJavascriptResource = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.js";

		/// <summary>
		/// The name of the javascript function that will initiate a synchronous callback.
		/// </summary>
		protected const string CallbackJSFunction = "window.dnoa_internal.callback";

		/// <summary>
		/// The name of the javascript function that will initiate an asynchronous callback.
		/// </summary>
		protected const string CallbackJSFunctionAsync = "window.dnoa_internal.callbackAsync";

		/// <summary>
		/// The name of the javascript field that stores the maximum time a positive assertion is
		/// good for before it must be refreshed.
		/// </summary>
		private const string MaxPositiveAssertionLifetimeJsName = "window.dnoa_internal.maxPositiveAssertionLifetime";

		/// <summary>
		/// The "dnoa.op_endpoint" string.
		/// </summary>
		private const string OPEndpointParameterName = OpenIdUtilities.CustomParameterPrefix + "op_endpoint";

		/// <summary>
		/// The "dnoa.claimed_id" string.
		/// </summary>
		private const string ClaimedIdParameterName = OpenIdUtilities.CustomParameterPrefix + "claimed_id";

		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for storing the value of a successful authentication.
		/// </summary>
		private const string AuthDataViewStateKey = "AuthData";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticationResponse"/> property.
		/// </summary>
		private const string AuthenticationResponseViewStateKey = "AuthenticationResponse";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticationProcessedAlready"/> property.
		/// </summary>
		private const string AuthenticationProcessedAlreadyViewStateKey = "AuthenticationProcessedAlready";

		#endregion

		/// <summary>
		/// Default value of the <see cref="Popup"/> property.
		/// </summary>
		private const PopupBehavior PopupDefault = PopupBehavior.Always;

		/// <summary>
		/// Default value of <see cref="LogOnMode"/> property..
		/// </summary>
		private const LogOnSiteNotification LogOnModeDefault = LogOnSiteNotification.None;

		/// <summary>
		/// Backing field for the <see cref="RelyingPartyNonVerifying"/> property.
		/// </summary>
		private static OpenIdRelyingParty relyingPartyNonVerifying;

		/// <summary>
		/// The authentication response that just came in.
		/// </summary>
		private IAuthenticationResponse authenticationResponse;

		/// <summary>
		/// Stores the result of an AJAX discovery request while it is waiting
		/// to be picked up by ASP.NET on the way down to the user agent.
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
			base.Popup = PopupDefault;

			// The expected use case for the AJAX login box is for comments... not logging in.
			this.LogOnMode = LogOnModeDefault;
		}

		/// <summary>
		/// Fired when a Provider sends back a positive assertion to this control,
		/// but the authentication has not yet been verified.
		/// </summary>
		/// <remarks>
		/// <b>No security critical decisions should be made within event handlers
		/// for this event</b> as the authenticity of the assertion has not been
		/// verified yet.  All security related code should go in the event handler
		/// for the <see cref="OpenIdRelyingPartyControlBase.LoggedIn"/> event.
		/// </remarks>
		[Description("Fired when a Provider sends back a positive assertion to this control, but the authentication has not yet been verified.")]
		public event EventHandler<OpenIdEventArgs> UnconfirmedPositiveAssertion;

		/// <summary>
		/// Gets or sets a value indicating when to use a popup window to complete the login experience.
		/// </summary>
		/// <value>The default value is <see cref="PopupBehavior.Never"/>.</value>
		[Bindable(false), Browsable(false), DefaultValue(PopupDefault)]
		public override PopupBehavior Popup {
			get { return base.Popup; }
			set { ErrorUtilities.VerifySupported(value == base.Popup, OpenIdStrings.PropertyValueNotSupported); }
		}

		/// <summary>
		/// Gets or sets the way a completed login is communicated to the rest of the web site.
		/// </summary>
		[Bindable(true), DefaultValue(LogOnModeDefault), Category(BehaviorCategory)]
		[Description("The way a completed login is communicated to the rest of the web site.")]
		public override LogOnSiteNotification LogOnMode { // override to set new DefaultValue
			get { return base.LogOnMode; }
			set { base.LogOnMode = value; }
		}

		/// <summary>
		/// Gets the completed authentication response.
		/// </summary>
		public IAuthenticationResponse AuthenticationResponse {
			get {
				if (this.authenticationResponse == null) {
					// We will either validate a new response and return a live AuthenticationResponse
					// or we will try to deserialize a previous IAuthenticationResponse (snapshot)
					// from viewstate and return that.
					IAuthenticationResponse viewstateResponse = this.ViewState[AuthenticationResponseViewStateKey] as IAuthenticationResponse;
					string viewstateAuthData = this.ViewState[AuthDataViewStateKey] as string;
					string formAuthData = this.Page.Request.Form[this.OpenIdAuthDataFormKey];

					// First see if there is fresh auth data to be processed into a response.
					if (!string.IsNullOrEmpty(formAuthData) && !string.Equals(viewstateAuthData, formAuthData, StringComparison.Ordinal)) {
						this.ViewState[AuthDataViewStateKey] = formAuthData;

						Uri authUri = new Uri(formAuthData);
						HttpRequestInfo clientResponseInfo = new HttpRequestInfo {
							UrlBeforeRewriting = authUri,
						};

						this.authenticationResponse = this.RelyingParty.GetResponse(clientResponseInfo);
						Logger.Controls.DebugFormat(
							"The {0} control checked for an authentication response and found: {1}",
							this.ID,
							this.authenticationResponse.Status);
						this.AuthenticationProcessedAlready = false;

						// Save out the authentication response to viewstate so we can find it on
						// a subsequent postback.
						this.ViewState[AuthenticationResponseViewStateKey] = new PositiveAuthenticationResponseSnapshot(this.authenticationResponse);
					} else {
						this.authenticationResponse = viewstateResponse;
					}
				}

				return this.authenticationResponse;
			}
		}

		/// <summary>
		/// Gets the name of the open id auth data form key (for the value as stored at the user agent as a FORM field).
		/// </summary>
		/// <value>Usually a concatenation of the control's name and <c>"_openidAuthData"</c>.</value>
		protected abstract string OpenIdAuthDataFormKey { get; }

		/// <summary>
		/// Gets the relying party to use when verification of incoming messages is NOT wanted.
		/// </summary>
		private static OpenIdRelyingParty RelyingPartyNonVerifying {
			get {
				if (relyingPartyNonVerifying == null) {
					relyingPartyNonVerifying = OpenIdRelyingParty.CreateNonVerifying();
				}
				return relyingPartyNonVerifying;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether an authentication in the page's view state
		/// has already been processed and appropriate events fired.
		/// </summary>
		private bool AuthenticationProcessedAlready {
			get { return (bool)(ViewState[AuthenticationProcessedAlreadyViewStateKey] ?? false); }
			set { ViewState[AuthenticationProcessedAlreadyViewStateKey] = value; }
		}

		/// <summary>
		/// Allows an OpenID extension to read data out of an unverified positive authentication assertion
		/// and send it down to the client browser so that Javascript running on the page can perform
		/// some preprocessing on the extension data.
		/// </summary>
		/// <typeparam name="T">The extension <i>response</i> type that will read data from the assertion.</typeparam>
		/// <param name="propertyName">The property name on the openid_identifier input box object that will be used to store the extension data.  For example: sreg</param>
		/// <remarks>
		/// This method should be called from the <see cref="UnconfirmedPositiveAssertion"/> event handler.
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

		#region ICallbackEventHandler Members

		/// <summary>
		/// Returns the result of discovery on some Identifier passed to <see cref="ICallbackEventHandler.RaiseCallbackEvent"/>.
		/// </summary>
		/// <returns>The result of the callback.</returns>
		/// <value>A whitespace delimited list of URLs that can be used to initiate authentication.</value>
		string ICallbackEventHandler.GetCallbackResult() {
			return this.GetCallbackResult();
		}

		/// <summary>
		/// Performs discovery on some OpenID Identifier.  Called directly from the user agent via
		/// AJAX callback mechanisms.
		/// </summary>
		/// <param name="eventArgument">The identifier to perform discovery on.</param>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "We want to preserve the signature of the interface.")]
		void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument) {
			this.RaiseCallbackEvent(eventArgument);
		}

		#endregion

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="identifier">The identifier to create a request for.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier"/>.
		/// </returns>
		protected internal override IEnumerable<IAuthenticationRequest> CreateRequests(Identifier identifier) {
			// If this control is actually a member of another OpenID RP control,
			// delegate creation of requests to the parent control.
			var parentOwner = this.ParentControls.OfType<OpenIdRelyingPartyControlBase>().FirstOrDefault();
			if (parentOwner != null) {
				return parentOwner.CreateRequests(identifier);
			} else {
				// We delegate all our logic to another method, since invoking base. methods
				// within an iterator method results in unverifiable code.
				return this.CreateRequestsCore(base.CreateRequests(identifier));
			}
		}

		/// <summary>
		/// Returns the results of a callback event that targets a control.
		/// </summary>
		/// <returns>The result of the callback.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "We want to preserve the signature of the interface.")]
		protected virtual string GetCallbackResult() {
			this.Page.Response.ContentType = "text/javascript";
			return this.discoveryResult;
		}

		/// <summary>
		/// Processes a callback event that targets a control.
		/// </summary>
		/// <param name="eventArgument">A string that represents an event argument to pass to the event handler.</param>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "We want to preserve the signature of the interface.")]
		protected virtual void RaiseCallbackEvent(string eventArgument) {
			string userSuppliedIdentifier = eventArgument;

			ErrorUtilities.VerifyNonZeroLength(userSuppliedIdentifier, "userSuppliedIdentifier");
			Logger.OpenId.InfoFormat("AJAX discovery on {0} requested.", userSuppliedIdentifier);

			this.Identifier = userSuppliedIdentifier;
			this.discoveryResult = this.SerializeDiscoveryAsJson(this.Identifier);
		}

		/// <summary>
		/// Pre-discovers an identifier and makes the results available to the
		/// user agent for javascript as soon as the page loads.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		protected void PreloadDiscovery(Identifier identifier) {
			this.PreloadDiscovery(new[] { identifier });
		}

		/// <summary>
		/// Pre-discovers a given set of identifiers and makes the results available to the
		/// user agent for javascript as soon as the page loads.
		/// </summary>
		/// <param name="identifiers">The identifiers to perform discovery on.</param>
		protected void PreloadDiscovery(IEnumerable<Identifier> identifiers) {
			string discoveryResults = this.SerializeDiscoveryAsJson(identifiers);
			string script = "window.dnoa_internal.loadPreloadedDiscoveryResults(" + discoveryResults + ");";
			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyAjaxControlBase), this.ClientID, script, true);
		}

		/// <summary>
		/// Fires the <see cref="UnconfirmedPositiveAssertion"/> event.
		/// </summary>
		protected virtual void OnUnconfirmedPositiveAssertion() {
			var unconfirmedPositiveAssertion = this.UnconfirmedPositiveAssertion;
			if (unconfirmedPositiveAssertion != null) {
				unconfirmedPositiveAssertion(this, null);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			// Our parent control ignores all OpenID messages included in a postback,
			// but our AJAX controls hide an old OpenID message in a postback payload,
			// so we deserialize it and process it when appropriate.
			if (this.Page.IsPostBack) {
				if (this.AuthenticationResponse != null && !this.AuthenticationProcessedAlready) {
					// Only process messages targeted at this control.
					// Note that Stateless mode causes no receiver to be indicated.
					string receiver = this.AuthenticationResponse.GetUntrustedCallbackArgument(ReturnToReceivingControlId);
					if (receiver == null || receiver == this.ClientID) {
						this.ProcessResponse(this.AuthenticationResponse);
						this.AuthenticationProcessedAlready = true;
					}
				}
			}
		}

		/// <summary>
		/// Called when the <see cref="Identifier"/> property is changed.
		/// </summary>
		protected override void OnIdentifierChanged() {
			base.OnIdentifierChanged();

			// Since the identifier changed, make sure we reset any cached authentication on the user agent.
			this.ViewState.Remove(AuthDataViewStateKey);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.SetWebAppPathOnUserAgent();
			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdRelyingPartyAjaxControlBase), EmbeddedAjaxJavascriptResource);

			StringBuilder initScript = new StringBuilder();

			initScript.AppendLine(CallbackJSFunctionAsync + " = " + this.GetJsCallbackConvenienceFunction(true));
			initScript.AppendLine(CallbackJSFunction + " = " + this.GetJsCallbackConvenienceFunction(false));

			// Positive assertions can last no longer than this library is willing to consider them valid,
			// and when they come with OP private associations they last no longer than the OP is willing
			// to consider them valid.  We assume the OP will hold them valid for at least five minutes.
			double assertionLifetimeInMilliseconds = Math.Min(TimeSpan.FromMinutes(5).TotalMilliseconds, Math.Min(DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime.TotalMilliseconds, DotNetOpenAuthSection.Configuration.Messaging.MaximumMessageLifetime.TotalMilliseconds));
			initScript.AppendLine(MaxPositiveAssertionLifetimeJsName + " = " + assertionLifetimeInMilliseconds.ToString(CultureInfo.InvariantCulture) + ";");

			// We register this callback code explicitly with a specific type rather than the derived-type of the control
			// to ensure that this discovery callback function is only set ONCE for the HTML document.
			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyControlBase), "initializer", initScript.ToString(), true);
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(HtmlTextWriter writer) {
			base.Render(writer);

			// Emit a hidden field to let the javascript on the user agent know if an
			// authentication has already successfully taken place.
			string viewstateAuthData = this.ViewState[AuthDataViewStateKey] as string;
			if (!string.IsNullOrEmpty(viewstateAuthData)) {
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.OpenIdAuthDataFormKey);
				writer.AddAttribute(HtmlTextWriterAttribute.Value, viewstateAuthData, true);
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
			}
		}

		/// <summary>
		/// Notifies the user agent via an AJAX response of a completed authentication attempt.
		/// </summary>
		protected override void ScriptClosingPopupOrIFrame() {
			Logger.OpenId.DebugFormat("AJAX (iframe) callback from OP: {0}", this.Page.Request.Url);
			string extensionsJson = null;

			var authResponse = RelyingPartyNonVerifying.GetResponse();
			Logger.Controls.DebugFormat(
				"The {0} control checked for an authentication response from a popup window or iframe using a non-verifying RP and found: {1}",
				this.ID,
				authResponse.Status);
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				this.OnUnconfirmedPositiveAssertion(); // event handler will fill the clientScriptExtensions collection.
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
			if (Page.Request.HttpMethod == "POST") {
				// Promote all form variables to the query string, but since it won't be passed
				// to any server (this is a javascript window-to-window transfer) the length of
				// it can be arbitrarily long, whereas it was POSTed here probably because it
				// was too long for HTTP transit.
				UriBuilder payloadUri = new UriBuilder(Page.Request.Url);
				payloadUri.AppendQueryArgs(Page.Request.Form.ToDictionary());
				payload = MessagingUtilities.GetSafeJavascriptValue(payloadUri.Uri.AbsoluteUri);
			}

			if (!string.IsNullOrEmpty(extensionsJson)) {
				payload += ", " + extensionsJson;
			}

			this.CallbackUserAgentMethod("dnoa_internal.processAuthorizationResult(" + payload + ")");
		}

		/// <summary>
		/// Serializes the discovery of multiple identifiers as a JSON object.
		/// </summary>
		/// <param name="identifiers">The identifiers to perform discovery on and create requests for.</param>
		/// <returns>The serialized JSON object.</returns>
		private string SerializeDiscoveryAsJson(IEnumerable<Identifier> identifiers) {
			ErrorUtilities.VerifyArgumentNotNull(identifiers, "identifiers");

			// We prepare a JSON object with this interface:
			// Array discoveryWrappers;
			// Where each element in the above array has this interface:
			// class discoveryWrapper {
			//    string userSuppliedIdentifier;
			//    jsonResponse discoveryResult; // contains result of call to SerializeDiscoveryAsJson(Identifier)
			// }
			StringBuilder discoveryResultBuilder = new StringBuilder();
			discoveryResultBuilder.Append("[");
			foreach (var identifier in identifiers) { // TODO: parallelize discovery on these identifiers
				discoveryResultBuilder.Append("{");
				discoveryResultBuilder.AppendFormat("userSuppliedIdentifier: {0},", MessagingUtilities.GetSafeJavascriptValue(identifier));
				discoveryResultBuilder.AppendFormat("discoveryResult: {0}", this.SerializeDiscoveryAsJson(identifier));
				discoveryResultBuilder.Append("},");
			}

			discoveryResultBuilder.Length -= 1; // trim last comma
			discoveryResultBuilder.Append("]");
			return discoveryResultBuilder.ToString();
		}

		/// <summary>
		/// Serializes the results of discovery and the created auth requests as a JSON object
		/// for the user agent to initiate.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <returns>The JSON string.</returns>
		private string SerializeDiscoveryAsJson(Identifier identifier) {
			ErrorUtilities.VerifyArgumentNotNull(identifier, "identifier");

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
				IEnumerable<IAuthenticationRequest> requests = this.CreateRequests(identifier).CacheGeneratedResults();
				if (requests.Any()) {
					discoveryResultBuilder.AppendFormat("claimedIdentifier: {0},", MessagingUtilities.GetSafeJavascriptValue(requests.First().ClaimedIdentifier));
					discoveryResultBuilder.Append("requests: [");
					foreach (IAuthenticationRequest request in requests) {
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
					discoveryResultBuilder.Append("requests: [],");
					discoveryResultBuilder.AppendFormat("error: {0}", MessagingUtilities.GetSafeJavascriptValue(OpenIdStrings.OpenIdEndpointNotFound));
				}
			} catch (ProtocolException ex) {
				discoveryResultBuilder.Append("requests: [],");
				discoveryResultBuilder.AppendFormat("error: {0}", MessagingUtilities.GetSafeJavascriptValue(ex.Message));
			}

			discoveryResultBuilder.Append("}");
			return discoveryResultBuilder.ToString();
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
			ErrorUtilities.VerifyArgumentNotNull(requests, "requests"); // NO CODE CONTRACTS! (yield return used here)

			// Configure each generated request.
			int reqIndex = 0;
			foreach (var req in requests) {
				req.SetUntrustedCallbackArgument("index", (reqIndex++).ToString(CultureInfo.InvariantCulture));

				// If the ReturnToUrl was explicitly set, we'll need to reset our first parameter
				if (string.IsNullOrEmpty(HttpUtility.ParseQueryString(req.ReturnToUrl.Query)[AuthenticationRequest.UserSuppliedIdentifierParameterName])) {
					Identifier userSuppliedIdentifier = ((AuthenticationRequest)req).Endpoint.UserSuppliedIdentifier;
					req.SetUntrustedCallbackArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName, userSuppliedIdentifier.OriginalString);
				}

				// Our javascript needs to let the user know which endpoint responded.  So we force it here.
				// This gives us the info even for 1.0 OPs and 2.0 setup_required responses.
				req.SetUntrustedCallbackArgument(OPEndpointParameterName, req.Provider.Uri.AbsoluteUri);
				req.SetUntrustedCallbackArgument(ClaimedIdParameterName, (string)req.ClaimedIdentifier ?? string.Empty);

				// Inform ourselves in return_to that we're in a popup or iframe.
				req.SetUntrustedCallbackArgument(UIPopupCallbackKey, "1");

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
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private void CallbackUserAgentMethod(string methodCall) {
			Logger.OpenId.DebugFormat("Sending Javascript callback: {0}", methodCall);
			Page.Response.Write(@"<html><body><script language='javascript'>
	var inPopup = !window.frameElement;
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
	}}
</script></body></html>";
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture, htmlFormat, methodCall));
			Page.Response.End();
		}

		/// <summary>
		/// Sets the window.aspnetapppath variable on the user agent so that cookies can be set with the proper path.
		/// </summary>
		private void SetWebAppPathOnUserAgent() {
			string script = "window.aspnetapppath = " + MessagingUtilities.GetSafeJavascriptValue(this.Page.Request.ApplicationPath) + ";";
			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyAjaxControlBase), "webapppath", script, true);
		}
	}
}
