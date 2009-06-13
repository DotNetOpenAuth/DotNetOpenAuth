//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyControlBase.EmbeddedJavascriptResource, "text/javascript")]

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
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.UI;

	/// <summary>
	/// A common base class for OpenID Relying Party controls.
	/// </summary>
	public abstract class OpenIdRelyingPartyControlBase : Control, ICallbackEventHandler {
		/// <summary>
		/// The manifest resource name of the javascript file to include on the hosting page.
		/// </summary>
		internal const string EmbeddedJavascriptResource = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdRelyingPartyControlBase.js";

		/// <summary>
		/// The name of the javascript function that will initiate a synchronous callback.
		/// </summary>
		protected const string CallbackJsFunction = "window.dnoa_internal.callback";

		/// <summary>
		/// The name of the javascript function that will initiate an asynchronous callback.
		/// </summary>
		protected const string CallbackJsFunctionAsync = "window.dnoa_internal.callbackAsync";

		#region Property category constants

		/// <summary>
		/// The "Appearance" category for properties.
		/// </summary>
		protected const string AppearanceCategory = "Appearance";

		/// <summary>
		/// The "Behavior" category for properties.
		/// </summary>
		protected const string BehaviorCategory = "Behavior";

		#endregion

		#region Property default values

		/// <summary>
		/// The default value for the <see cref="Stateless"/> property.
		/// </summary>
		private const bool StatelessDefault = false;

		/// <summary>
		/// Default value of <see cref="UsePersistentCookie"/>.
		/// </summary>
		private const bool UsePersistentCookieDefault = false;

		/// <summary>
		/// The default value for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlDefault = "";

		/// <summary>
		/// The default value for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlDefault = "~/";

		/// <summary>
		/// The default value for the <see cref="Popup"/> property.
		/// </summary>
		private const PopupBehavior PopupDefault = PopupBehavior.Never;

		/// <summary>
		/// The default value for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const bool RequireSslDefault = false;

		#endregion

		#region Property view state keys

		/// <summary>
		/// The viewstate key to use for the <see cref="Stateless"/> property.
		/// </summary>
		private const string StatelessViewStateKey = "Stateless";

		/// <summary>
		/// The viewstate key to use for the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieViewStateKey = "UsePersistentCookie";

		/// <summary>
		/// The viewstate key to use for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlViewStateKey = "RealmUrl";

		/// <summary>
		/// The viewstate key to use for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlViewStateKey = "ReturnToUrl";

		/// <summary>
		/// The key under which the value for the <see cref="Identifier"/> property will be stored.
		/// </summary>
		private const string IdentifierViewStateKey = "Identifier";

		/// <summary>
		/// The viewstate key to use for the <see cref="Popup"/> property.
		/// </summary>
		private const string PopupViewStateKey = "Popup";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const string RequireSslViewStateKey = "RequireSsl";

		#endregion

		#region Callback parameter names

		/// <summary>
		/// The callback parameter for use with persisting the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieCallbackKey = "OpenIdTextBox_UsePersistentCookie";

		/// <summary>
		/// The callback parameter to use for recognizing when the callback is in a popup window.
		/// </summary>
		private const string UIPopupCallbackKey = OpenIdUtilities.CustomParameterPrefix + "uipopup";

		/// <summary>
		/// The callback parameter to use for recognizing when the callback is in the parent window.
		/// </summary>
		private const string UIPopupCallbackParentKey = OpenIdUtilities.CustomParameterPrefix + "uipopupParent";

		#endregion

		/// <summary>
		/// Stores the result of a AJAX callback discovery.
		/// </summary>
		private string discoveryResult;

		/// <summary>
		/// Backing field for the <see cref="RelyingParty"/> property.
		/// </summary>
		private OpenIdRelyingParty relyingParty;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyControlBase"/> class.
		/// </summary>
		protected OpenIdRelyingPartyControlBase() {
		}

		#region Events

		/// <summary>
		/// Fired after the user clicks the log in button, but before the authentication
		/// process begins.  Offers a chance for the web application to disallow based on 
		/// OpenID URL before redirecting the user to the OpenID Provider.
		/// </summary>
		[Description("Fired after the user clicks the log in button, but before the authentication process begins.  Offers a chance for the web application to disallow based on OpenID URL before redirecting the user to the OpenID Provider.")]
		public event EventHandler<OpenIdEventArgs> LoggingIn;

		/// <summary>
		/// Fired upon completion of a successful login.
		/// </summary>
		[Description("Fired upon completion of a successful login.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;

		/// <summary>
		/// Fired when a login attempt fails.
		/// </summary>
		[Description("Fired when a login attempt fails.")]
		public event EventHandler<OpenIdEventArgs> Failed;

		/// <summary>
		/// Fired when an authentication attempt is canceled at the OpenID Provider.
		/// </summary>
		[Description("Fired when an authentication attempt is canceled at the OpenID Provider.")]
		public event EventHandler<OpenIdEventArgs> Canceled;

		#endregion

		/// <summary>
		/// Gets or sets the <see cref="OpenIdRelyingParty"/> instance to use.
		/// </summary>
		/// <value>The default value is an <see cref="OpenIdRelyingParty"/> instance initialized according to the web.config file.</value>
		/// <remarks>
		/// A performance optimization would be to store off the 
		/// instance as a static member in your web site and set it
		/// to this property in your <see cref="Control.Load">Page.Load</see>
		/// event since instantiating these instances can be expensive on 
		/// heavily trafficked web pages.
		/// </remarks>
		[Browsable(false)]
		public OpenIdRelyingParty RelyingParty {
			get {
				if (this.relyingParty == null) {
					this.relyingParty = this.CreateRelyingParty();
				}
				return this.relyingParty;
			}

			set {
				this.relyingParty = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether stateless mode is used.
		/// </summary>
		[Bindable(true), DefaultValue(StatelessDefault), Category(BehaviorCategory)]
		[Description("Controls whether stateless mode is used.")]
		public bool Stateless {
			get { return (bool)(ViewState[StatelessViewStateKey] ?? StatelessDefault); }
			set { ViewState[StatelessViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(RealmUrlDefault), Category(BehaviorCategory)]
		[Description("The OpenID Realm of the relying party web site.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string RealmUrl {
			get {
				return (string)(ViewState[RealmUrlViewStateKey] ?? RealmUrlDefault);
			}

			set {
				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Realm object based on it.
					new Realm(OpenIdUtilities.GetResolvedRealm(this.Page, value, this.RelyingParty.Channel.GetRequestFromContext())); // throws an exception on failure.
				} else {
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://")) {
						new Uri(value.Replace("*.", string.Empty)); // make sure it's fully-qualified, but ignore wildcards
					} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
						// this is valid too
					} else {
						throw new UriFormatException();
					}
				}
				ViewState[RealmUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the OpenID ReturnTo of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Bindable property must be simple type")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(ReturnToUrlDefault), Category(BehaviorCategory)]
		[Description("The OpenID ReturnTo of the relying party web site.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string ReturnToUrl {
			get {
				return (string)(this.ViewState[ReturnToUrlViewStateKey] ?? ReturnToUrlDefault);
			}

			set {
				if (this.Page != null && !this.DesignMode) {
					// Validate new value by trying to construct a Uri based on it.
					new Uri(this.RelyingParty.Channel.GetRequestFromContext().UrlBeforeRewriting, this.Page.ResolveUrl(value)); // throws an exception on failure.
				} else {
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://")) {
						new Uri(value); // make sure it's fully-qualified, but ignore wildcards
					} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
						// this is valid too
					} else {
						throw new UriFormatException();
					}
				}

				this.ViewState[ReturnToUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to send a persistent cookie upon successful 
		/// login so the user does not have to log in upon returning to this site.
		/// </summary>
		[Bindable(true), DefaultValue(UsePersistentCookieDefault), Category(BehaviorCategory)]
		[Description("Whether to send a persistent cookie upon successful " +
			"login so the user does not have to log in upon returning to this site.")]
		public virtual bool UsePersistentCookie {
			get { return (bool)(this.ViewState[UsePersistentCookieViewStateKey] ?? UsePersistentCookieDefault); }
			set { this.ViewState[UsePersistentCookieViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating when to use a popup window to complete the login experience.
		/// </summary>
		/// <value>The default value is <see cref="PopupBehavior.Never"/>.</value>
		[Bindable(true), DefaultValue(PopupDefault), Category(BehaviorCategory)]
		[Description("When to use a popup window to complete the login experience.")]
		public PopupBehavior Popup {
			get { return (PopupBehavior)(ViewState[PopupViewStateKey] ?? PopupDefault); }
			set { ViewState[PopupViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to enforce on high security mode,
		/// which requires the full authentication pipeline to be protected by SSL.
		/// </summary>
		[Bindable(true), DefaultValue(RequireSslDefault), Category(BehaviorCategory)]
		[Description("Turns on high security mode, requiring the full authentication pipeline to be protected by SSL.")]
		public bool RequireSsl {
			get { return (bool)(ViewState[RequireSslViewStateKey] ?? RequireSslDefault); }
			set { ViewState[RequireSslViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the URL to your privacy policy page that describes how 
		/// claims will be used and/or shared.
		/// </summary>
		[Bindable(true), Category(BehaviorCategory)]
		[Description("The OpenID Identifier that this button will use to initiate login.")]
		[TypeConverter(typeof(IdentifierConverter))]
		public Identifier Identifier {
			get { return (Identifier)ViewState[IdentifierViewStateKey]; }
			set { ViewState[IdentifierViewStateKey] = value; }
		}

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		public void LogOn() {
			IAuthenticationRequest request = this.CreateRequest();
			if (this.IsPopupAppropriate(request)) {
				this.ScriptPopupWindow(request);
			} else {
				request.RedirectToProvider();
			}
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
				List<IAuthenticationRequest> requests = new List<IAuthenticationRequest>(new [] { this.CreateRequest() });
				if (requests.Count > 0) {
					discoveryResultBuilder.AppendFormat("claimedIdentifier: {0},", MessagingUtilities.GetSafeJavascriptValue(requests[0].ClaimedIdentifier));
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
		/// Constructs the authentication request and returns it.
		/// </summary>
		/// <returns>The instantiated authentication request, or <c>null</c> if a failure occurred.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		protected virtual IAuthenticationRequest CreateRequest() {
			Contract.Requires(this.Identifier != null, OpenIdStrings.OpenIdTextBoxEmpty);
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Identifier), OpenIdStrings.OpenIdTextBoxEmpty);
			IAuthenticationRequest request;

			try {
				// Approximate the returnTo (either based on the customize property or the page URL)
				// so we can use it to help with Realm resolution.
				var requestContext = this.RelyingParty.Channel.GetRequestFromContext();
				Uri returnToApproximation = this.ReturnToUrl != null ? new Uri(requestContext.UrlBeforeRewriting, this.ReturnToUrl) : this.Page.Request.Url;

				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = OpenIdUtilities.GetResolvedRealm(this.Page, this.RealmUrl, this.RelyingParty.Channel.GetRequestFromContext());
				realm.Scheme = returnToApproximation.Scheme;
				realm.Port = returnToApproximation.Port;

				// Initiate openid request
				// We use TryParse here to avoid throwing an exception which 
				// might slip through our validator control if it is disabled.
				Realm typedRealm = new Realm(realm);
				if (string.IsNullOrEmpty(this.ReturnToUrl)) {
					request = this.RelyingParty.CreateRequest(this.Identifier, typedRealm);
				} else {
					// Since the user actually gave us a return_to value,
					// the "approximation" is exactly what we want.
					request = this.RelyingParty.CreateRequest(this.Identifier, typedRealm, returnToApproximation);
				}

				if (this.IsPopupAppropriate(request)) {
					// Inform the OP that it will appear in a popup window.
					request.AddExtension(new UIRequest());
				}

				// Add state that needs to survive across the redirect.
				if (!this.Stateless) {
					request.AddCallbackArguments(UsePersistentCookieCallbackKey, this.UsePersistentCookie.ToString(CultureInfo.InvariantCulture));
				}

				this.OnLoggingIn(request);
			} catch (ProtocolException ex) {
				this.OnFailed(new FailedAuthenticationResponse(ex));
				return null;
			}

			return request;
		}

		/// <summary>
		/// Raises the <see cref="E:Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Page.IsPostBack) {
				// OpenID responses NEVER come in the form of a postback.
				return;
			}

			var response = this.RelyingParty.GetResponse();
			if (response != null) {
				string persistentString = response.GetCallbackArgument(UsePersistentCookieCallbackKey);
				bool persistentBool;
				if (persistentString != null && bool.TryParse(persistentString, out persistentBool)) {
					this.UsePersistentCookie = persistentBool;
				}

				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						this.OnLoggedIn(response);
						break;
					case AuthenticationStatus.Canceled:
						this.OnCanceled(response);
						break;
					case AuthenticationStatus.Failed:
						this.OnFailed(response);
						break;
					case AuthenticationStatus.SetupRequired:
					case AuthenticationStatus.ExtensionsOnly:
					default:
						// The NotApplicable (extension-only assertion) is NOT one that we support
						// in this control because that scenario is primarily interesting to RPs
						// that are asking a specific OP, and it is not user-initiated as this textbox
						// is designed for.
						throw new InvalidOperationException(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}
			}
		}

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
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdRelyingPartyControlBase), EmbeddedJavascriptResource);

			StringBuilder initScript = new StringBuilder();

			initScript.AppendLine(CallbackJsFunctionAsync + " = " + GetJsCallbackConvenienceFunction(true));
			initScript.AppendLine(CallbackJsFunction + " = " + GetJsCallbackConvenienceFunction(false));

			this.Page.ClientScript.RegisterClientScriptBlock(typeof(OpenIdRelyingPartyControlBase), "initializer", initScript.ToString(), true);
		}

		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnLoggedIn(IAuthenticationResponse response) {
			Contract.Requires(response != null);
			Contract.Requires(response.Status == AuthenticationStatus.Authenticated);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.Authenticated, "Firing OnLoggedIn event without an authenticated response.");

			var loggedIn = this.LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(response);
			if (loggedIn != null) {
				loggedIn(this, args);
			}

			if (!args.Cancel) {
				FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, this.UsePersistentCookie);
			}
		}

		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// Returns whether the login should proceed.  False if some event handler canceled the request.
		/// </returns>
		protected virtual bool OnLoggingIn(IAuthenticationRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			EventHandler<OpenIdEventArgs> loggingIn = this.LoggingIn;

			OpenIdEventArgs args = new OpenIdEventArgs(request);
			if (loggingIn != null) {
				loggingIn(this, args);
			}

			return !args.Cancel;
		}

		/// <summary>
		/// Fires the <see cref="Canceled"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnCanceled(IAuthenticationResponse response) {
			Contract.Requires(response != null);
			Contract.Requires(response.Status == AuthenticationStatus.Canceled);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.Canceled, "Firing Canceled event for the wrong response type.");

			var canceled = this.Canceled;
			if (canceled != null) {
				canceled(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Fires the <see cref="Failed"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnFailed(IAuthenticationResponse response) {
			Contract.Requires(response != null);
			Contract.Requires(response.Status == AuthenticationStatus.Failed);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.Failed, "Firing Failed event for the wrong response type.");

			var failed = this.Failed;
			if (failed != null) {
				failed(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Creates the relying party instance used to generate authentication requests.
		/// </summary>
		/// <returns>The instantiated relying party.</returns>
		protected virtual OpenIdRelyingParty CreateRelyingParty() {
			IRelyingPartyApplicationStore store = this.Stateless ? null : DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.ApplicationStore.CreateInstance(OpenIdRelyingParty.HttpApplicationStore);
			var rp = new OpenIdRelyingParty(store);

			// Only set RequireSsl to true, as we don't want to override 
			// a .config setting of true with false.
			if (this.RequireSsl) {
				rp.SecuritySettings.RequireSsl = true;
			}

			return rp;
		}

		/// <summary>
		/// Detects whether a popup window should be used to show the Provider's UI
		/// and applies the UI extension to the request when appropriate.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// 	<c>true</c> if a popup should be used; <c>false</c> otherwise.
		/// </returns>
		private bool IsPopupAppropriate(IAuthenticationRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			return this.Popup == PopupBehavior.Always || request.Provider.IsExtensionSupported<UIRequest>();
		}

		/// <summary>
		/// Wires the return page to immediately display a popup window with the Provider in it.
		/// </summary>
		/// <param name="request">The request.</param>
		private void ScriptPopupWindow(IAuthenticationRequest request) {
			Contract.Requires(request != null);
			Contract.Requires(this.RelyingParty != null);

			request.AddCallbackArguments(UIPopupCallbackKey, "1");

			StringBuilder startupScript = new StringBuilder();

			// Add a callback function that the popup window can call on this, the
			// parent window, to pass back the authentication result.
			startupScript.AppendLine("window.dnoa_internal = new Object();");
			startupScript.AppendLine("window.dnoa_internal.processAuthorizationResult = function(uri) { window.location = uri; };");
			startupScript.AppendLine("window.dnoa_internal.popupWindow = function() {");
				startupScript.AppendFormat(
					@"\tvar openidPopup = {0}",
					UIUtilities.GetWindowPopupScript(this.RelyingParty, request, "openidPopup"));
			startupScript.AppendLine("};");

			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "loginPopup", startupScript.ToString(), true);
		}

		/// <summary>
		/// Wires the popup window to close itself and pass the authentication result to the parent window.
		/// </summary>
		private void ScriptClosingPopup() {
			StringBuilder startupScript = new StringBuilder();
			startupScript.AppendLine("window.opener.dnoa_internal.processAuthorizationResult(document.URL + '&" + UIPopupCallbackParentKey + "=1');");
			startupScript.AppendLine("window.close();");

			this.Page.ClientScript.RegisterStartupScript(this.GetType(), "loginPopupClose", startupScript.ToString(), true);
		}
	}
}
