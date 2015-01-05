//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyAjaxControlBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.EmbeddedAjaxJavascriptResource, "text/javascript")]

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Script.Serialization;
	using System.Web.UI;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using Validation;

	/// <summary>
	/// A common base class for OpenID Relying Party controls.
	/// </summary>
	public abstract class OpenIdRelyingPartyAjaxControlBase : OpenIdRelyingPartyControlBase, ICallbackEventHandler {
		/// <summary>
		/// The manifest resource name of the javascript file to include on the hosting page.
		/// </summary>
		internal const string EmbeddedAjaxJavascriptResource = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.js";

		/// <summary>
		/// The "dnoa.op_endpoint" string.
		/// </summary>
		internal const string OPEndpointParameterName = OpenIdUtilities.CustomParameterPrefix + "op_endpoint";

		/// <summary>
		/// The "dnoa.claimed_id" string.
		/// </summary>
		internal const string ClaimedIdParameterName = OpenIdUtilities.CustomParameterPrefix + "claimed_id";

		/// <summary>
		/// The name of the javascript field that stores the maximum time a positive assertion is
		/// good for before it must be refreshed.
		/// </summary>
		internal const string MaxPositiveAssertionLifetimeJsName = "window.dnoa_internal.maxPositiveAssertionLifetime";

		/// <summary>
		/// The name of the javascript function that will initiate an asynchronous callback.
		/// </summary>
		protected internal const string CallbackJSFunctionAsync = "window.dnoa_internal.callbackAsync";

		/// <summary>
		/// The name of the javascript function that will initiate a synchronous callback.
		/// </summary>
		protected const string CallbackJSFunction = "window.dnoa_internal.callback";

		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for storing the value of a successful authentication.
		/// </summary>
		private const string AuthDataViewStateKey = "AuthData";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="GetAuthenticationResponseAsync"/> method.
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
		/// The authentication response that just came in.
		/// </summary>
		private IAuthenticationResponse authenticationResponse;

		/// <summary>
		/// Stores the result of an AJAX discovery request while it is waiting
		/// to be picked up by ASP.NET on the way down to the user agent.
		/// </summary>
		private string discoveryResult;

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
		/// Gets or sets the <see cref="OpenIdRelyingParty"/> instance to use.
		/// </summary>
		/// <value>
		/// The default value is an <see cref="OpenIdRelyingParty"/> instance initialized according to the web.config file.
		/// </value>
		/// <remarks>
		/// A performance optimization would be to store off the
		/// instance as a static member in your web site and set it
		/// to this property in your <see cref="Control.Load">Page.Load</see>
		/// event since instantiating these instances can be expensive on
		/// heavily trafficked web pages.
		/// </remarks>
		public override OpenIdRelyingParty RelyingParty {
			get {
				return base.RelyingParty;
			}

			set {
				// Make sure we get an AJAX-ready instance.
				ErrorUtilities.VerifyArgument(value is OpenIdAjaxRelyingParty, OpenIdStrings.TypeMustImplementX, typeof(OpenIdAjaxRelyingParty).Name);
				base.RelyingParty = value;
			}
		}

		/// <summary>
		/// Gets the relying party as its AJAX type.
		/// </summary>
		protected OpenIdAjaxRelyingParty AjaxRelyingParty {
			get { return (OpenIdAjaxRelyingParty)this.RelyingParty; }
		}

		/// <summary>
		/// Gets the name of the open id auth data form key (for the value as stored at the user agent as a FORM field).
		/// </summary>
		/// <value>Usually a concatenation of the control's name and <c>"_openidAuthData"</c>.</value>
		protected abstract string OpenIdAuthDataFormKey { get; }

		/// <summary>
		/// Gets or sets a value indicating whether an authentication in the page's view state
		/// has already been processed and appropriate events fired.
		/// </summary>
		private bool AuthenticationProcessedAlready {
			get { return (bool)(ViewState[AuthenticationProcessedAlreadyViewStateKey] ?? false); }
			set { ViewState[AuthenticationProcessedAlreadyViewStateKey] = value; }
		}

		/// <summary>
		/// Gets the completed authentication response.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message.</returns>
		public async Task<IAuthenticationResponse> GetAuthenticationResponseAsync(CancellationToken cancellationToken) {
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

					HttpRequestBase clientResponseInfo = new HttpRequestInfo("GET", new Uri(formAuthData));
					this.authenticationResponse = await this.RelyingParty.GetResponseAsync(clientResponseInfo, cancellationToken);
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
			Requires.NotNullOrEmpty(propertyName, "propertyName");
			this.RelyingParty.RegisterClientScriptExtension<T>(propertyName);
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
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
			Justification = "We want to preserve the signature of the interface.")]
		protected virtual void RaiseCallbackEvent(string eventArgument) {
			string userSuppliedIdentifier = eventArgument;

			ErrorUtilities.VerifyNonZeroLength(userSuppliedIdentifier, "userSuppliedIdentifier");
			Logger.OpenId.InfoFormat("AJAX discovery on {0} requested.", userSuppliedIdentifier);

			this.Identifier = userSuppliedIdentifier;

			this.Page.RegisterAsyncTask(new PageAsyncTask(async ct => {
				var serializer = new JavaScriptSerializer();
				IEnumerable<IAuthenticationRequest> requests = await this.CreateRequestsAsync(this.Identifier, ct);
				this.discoveryResult = serializer.Serialize(await this.AjaxRelyingParty.AsJsonDiscoveryResultAsync(requests, ct));
			}));
		}

		/// <summary>
		/// Creates the relying party instance used to generate authentication requests.
		/// </summary>
		/// <param name="store">The store to pass to the relying party constructor.</param>
		/// <returns>The instantiated relying party.</returns>
		protected override OpenIdRelyingParty CreateRelyingParty(ICryptoKeyAndNonceStore store) {
			return new OpenIdAjaxRelyingParty(store);
		}

		/// <summary>
		/// Pre-discovers an identifier and makes the results available to the
		/// user agent for javascript as soon as the page loads.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		protected Task PreloadDiscoveryAsync(Identifier identifier, CancellationToken cancellationToken) {
			return this.PreloadDiscoveryAsync(new[] { identifier }, cancellationToken);
		}

		/// <summary>
		/// Pre-discovers a given set of identifiers and makes the results available to the
		/// user agent for javascript as soon as the page loads.
		/// </summary>
		/// <param name="identifiers">The identifiers to perform discovery on.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		protected async Task PreloadDiscoveryAsync(IEnumerable<Identifier> identifiers, CancellationToken cancellationToken) {
			var requests = await Task.WhenAll(identifiers.Select(id => this.CreateRequestsAsync(id, cancellationToken)));
			string script = await this.AjaxRelyingParty.AsAjaxPreloadedDiscoveryResultAsync(requests.SelectMany(r => r), cancellationToken);
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
				this.Page.RegisterAsyncTask(new PageAsyncTask(async ct => {
					var response = await this.GetAuthenticationResponseAsync(ct);
					if (response != null && !this.AuthenticationProcessedAlready) {
						// Only process messages targeted at this control.
						// Note that Stateless mode causes no receiver to be indicated.
						string receiver = response.GetUntrustedCallbackArgument(ReturnToReceivingControlId);
						if (receiver == null || receiver == this.ClientID) {
							this.ProcessResponse(response);
							this.AuthenticationProcessedAlready = true;
						}
					}
				}));
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
			double assertionLifetimeInMilliseconds = Math.Min(TimeSpan.FromMinutes(5).TotalMilliseconds, Math.Min(OpenIdElement.Configuration.MaxAuthenticationTime.TotalMilliseconds, DotNetOpenAuthSection.Messaging.MaximumMessageLifetime.TotalMilliseconds));
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
			Assumes.True(writer != null, "Missing contract.");
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		protected override async Task ScriptClosingPopupOrIFrameAsync(CancellationToken cancellationToken) {
			Action<AuthenticationStatus> callback = status => {
				if (status == AuthenticationStatus.Authenticated) {
					this.OnUnconfirmedPositiveAssertion(); // event handler will fill the clientScriptExtensions collection.
				}
			};

			HttpResponseMessage response = await this.RelyingParty.ProcessResponseFromPopupAsync(
				new HttpRequestWrapper(this.Context.Request).AsHttpRequestMessage(),
				callback,
				cancellationToken);

			await response.SendAsync(new HttpContextWrapper(this.Context), cancellationToken);
			this.Context.Response.End();
		}

		/// <summary>
		/// Constructs a function that will initiate an AJAX callback.
		/// </summary>
		/// <param name="async">if set to <c>true</c> causes the AJAX callback to be a little more asynchronous.  Note that <c>false</c> does not mean the call is absolutely synchronous.</param>
		/// <returns>The string defining a javascript anonymous function that initiates a callback.</returns>
		private string GetJsCallbackConvenienceFunction(bool @async) {
			string argumentParameterName = "argument";
			string callbackResultParameterName = "resultFunction";
			string callbackErrorCallbackParameterName = "errorCallback";
			string callback = Page.ClientScript.GetCallbackEventReference(
				this,
				argumentParameterName,
				callbackResultParameterName,
				argumentParameterName,
				callbackErrorCallbackParameterName,
				@async);
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
		/// Sets the window.aspnetapppath variable on the user agent so that cookies can be set with the proper path.
		/// </summary>
		private void SetWebAppPathOnUserAgent() {
			string script = "window.aspnetapppath = " + MessagingUtilities.GetSafeJavascriptValue(this.Page.Request.ApplicationPath) + ";";
			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyAjaxControlBase), "webapppath", script, true);
		}
	}
}
