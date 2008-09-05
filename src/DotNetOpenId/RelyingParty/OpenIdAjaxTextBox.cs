using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenId.Extensions;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedDotNetOpenIdLogoResourceName, "image/gif")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedSpinnerResourceName, "image/gif")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName, "image/png")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginFailureResourceName, "image/png")]

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// An ASP.NET control that provides a minimal text box that is OpenID-aware and uses AJAX for
	/// a premium login experience.
	/// </summary>
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdAjaxTextBox runat=\"server\" />")]
	public class OpenIdAjaxTextBox : WebControl {
		internal const string EmbeddedScriptResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxTextBox.js";
		internal const string EmbeddedDotNetOpenIdLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.dotnetopenid_16x16.gif";
		internal const string EmbeddedSpinnerResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.spinner.gif";
		internal const string EmbeddedLoginSuccessResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.login_success.png";
		internal const string EmbeddedLoginFailureResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.login_failure.png";

		#region Properties

		const string authenticationResponseViewStateKey = "AuthenticationResponse";
		const string authDataViewStateKey = "AuthData";
		IAuthenticationResponse authenticationResponse;
		/// <summary>
		/// Gets the completed authentication response.
		/// </summary>
		public IAuthenticationResponse AuthenticationResponse {
			get {
				if (authenticationResponse == null) {
					// We will either validate a new response and return a live AuthenticationResponse
					// or we will try to deserialize a previous IAuthenticationResponse (snapshot)
					// from viewstate and return that.
					IAuthenticationResponse viewstateResponse = ViewState[authenticationResponseViewStateKey] as IAuthenticationResponse;
					string viewstateAuthData = ViewState[authDataViewStateKey] as string;
					string formAuthData = Page.Request.Form["openidAuthData"];

					// First see if there is fresh auth data to be processed into a response.
					if (formAuthData != null && !string.Equals(viewstateAuthData, formAuthData, StringComparison.Ordinal)) {
						ViewState[authDataViewStateKey] = formAuthData;

						Uri authUri = new Uri(formAuthData ?? viewstateAuthData);
						var authDataFields = HttpUtility.ParseQueryString(authUri.Query);
						var rp = new OpenIdRelyingParty(OpenIdRelyingParty.HttpApplicationStore,
							authUri, authDataFields);
						authenticationResponse = rp.Response;

						// Save out the authentication response to viewstate so we can find it on
						// a subsequent postback.
						ViewState[authenticationResponseViewStateKey] = new AuthenticationResponseSnapshot(authenticationResponse);
					} else {
						authenticationResponse = viewstateResponse;
					}
				}
				return authenticationResponse;
			}
		}

		const string textViewStateKey = "Text";
		/// <summary>
		/// Gets/sets the value in the text field, completely unprocessed or normalized.
		/// </summary>
		[Bindable(true), DefaultValue(""), Category("Appearance")]
		[Description("The value in the text field, completely unprocessed or normalized.")]
		public string Text {
			get { return (string)(ViewState[textViewStateKey] ?? string.Empty); }
			set { ViewState[textViewStateKey] = value ?? string.Empty; }
		}

		const string columnsViewStateKey = "Columns";
		const int columnsDefault = 40;
		/// <summary>
		/// The width of the text box in characters.
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(columnsDefault)]
		[Description("The width of the text box in characters.")]
		public int Columns {
			get { return (int)(ViewState[columnsViewStateKey] ?? columnsDefault); }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				ViewState[columnsViewStateKey] = value;
			}
		}

		const string tabIndexViewStateKey = "TabIndex";
		/// <summary>
		/// Default value for <see cref="TabIndex"/> property.
		/// </summary>
		const short tabIndexDefault = 0;
		/// <summary>
		/// The tab index of the text box control.  Use 0 to omit an explicit tabindex.
		/// </summary>
		[Bindable(true), Category("Behavior"), DefaultValue(tabIndexDefault)]
		[Description("The tab index of the text box control.  Use 0 to omit an explicit tabindex.")]
		public override short TabIndex {
			get { return (short)(ViewState[tabIndexViewStateKey] ?? tabIndexDefault); }
			set { ViewState[tabIndexViewStateKey] = value; }
		}

		const string nameViewStateKey = "Name";
		const string nameDefault = "openid_identifier";
		/// <summary>
		/// Gets/sets the HTML name to assign to the text field.
		/// </summary>
		[Bindable(true), DefaultValue(nameDefault), Category("Misc")]
		[Description("The HTML name to assign to the text field.")]
		public string Name {
			get { return (string)(ViewState[nameViewStateKey] ?? nameDefault); }
			set {
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				ViewState[nameViewStateKey] = value ?? string.Empty;
			}
		}

		const string timeoutViewStateKey = "Timeout";
		readonly TimeSpan timeoutDefault = TimeSpan.FromSeconds(8);
		/// <summary>
		/// Gets/sets the time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.
		/// </summary>
		[Browsable(true), DefaultValue(typeof(TimeSpan), "00:00:08"), Category("Behavior")]
		[Description("The time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.")]
		public TimeSpan Timeout {
			get { return (TimeSpan)(ViewState[timeoutViewStateKey] ?? timeoutDefault); }
			set {
				if (value.TotalMilliseconds <= 0) throw new ArgumentOutOfRangeException("value");
				ViewState[timeoutViewStateKey] = value;
			}
		}

		const string logonTextViewStateKey = "LoginText";
		const string logonTextDefault = "LOG IN";
		/// <summary>
		/// Gets/sets the text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(logonTextDefault), Localizable(true), Category("Appearance")]
		[Description("The text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.")]
		public string LogOnText {
			get { return (string)(ViewState[logonTextViewStateKey] ?? logonTextDefault); }
			set {
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				ViewState[logonTextViewStateKey] = value ?? string.Empty;
			}
		}

		const string logonToolTipViewStateKey = "LoginToolTip";
		const string logonToolTipDefault = "Click here to log in using a pop-up window.";
		/// <summary>
		/// Gets/sets the rool tip text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(logonToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.")]
		public string LogOnToolTip {
			get { return (string)(ViewState[logonToolTipViewStateKey] ?? logonToolTipDefault); }
			set { ViewState[logonToolTipViewStateKey] = value ?? string.Empty; }
		}

		const string retryTextViewStateKey = "RetryText";
		const string retryTextDefault = "RETRY";
		/// <summary>
		/// Gets/sets the text that appears on the RETRY button in cases where authentication times out.
		/// </summary>
		[Bindable(true), DefaultValue(retryTextDefault), Localizable(true), Category("Appearance")]
		[Description("The text that appears on the RETRY button in cases where authentication times out.")]
		public string RetryText {
			get { return (string)(ViewState[retryTextViewStateKey] ?? retryTextDefault); }
			set {
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				ViewState[retryTextViewStateKey] = value ?? string.Empty;
			}
		}

		const string retryToolTipViewStateKey = "RetryToolTip";
		const string retryToolTipDefault = "Retry a failed identifier discovery.";
		/// <summary>
		/// Gets/sets the tool tip text that appears on the RETRY button in cases where authentication times out.
		/// </summary>
		[Bindable(true), DefaultValue(retryToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears on the RETRY button in cases where authentication times out.")]
		public string RetryToolTip {
			get { return (string)(ViewState[retryToolTipViewStateKey] ?? retryToolTipDefault); }
			set { ViewState[retryToolTipViewStateKey] = value ?? string.Empty; }
		}

		const string authenticationSucceededToolTipViewStateKey = "AuthenticationSucceededToolTip";
		const string authenticationSucceededToolTipDefault = "Authenticated.";
		/// <summary>
		/// Gets/sets the tool tip text that appears when authentication succeeds.
		/// </summary>
		[Bindable(true), DefaultValue(authenticationSucceededToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears when authentication succeeds.")]
		public string AuthenticationSucceededToolTip {
			get { return (string)(ViewState[authenticationSucceededToolTipViewStateKey] ?? authenticationSucceededToolTipDefault); }
			set { ViewState[authenticationSucceededToolTipViewStateKey] = value ?? string.Empty; }
		}

		const string authenticationFailedToolTipViewStateKey = "AuthenticationFailedToolTip";
		const string authenticationFailedToolTipDefault = "Authentication failed.";
		/// <summary>
		/// Gets/sets the tool tip text that appears when authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(authenticationFailedToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears when authentication fails.")]
		public string AuthenticationFailedToolTip {
			get { return (string)(ViewState[authenticationFailedToolTipViewStateKey] ?? authenticationFailedToolTipDefault); }
			set { ViewState[authenticationFailedToolTipViewStateKey] = value ?? string.Empty; }
		}

		const string busyToolTipViewStateKey = "BusyToolTip";
		const string busyToolTipDefault = "Discovering/authenticating";
		/// <summary>
		/// Gets/sets the tool tip text that appears over the text box when it is discovering and authenticating.
		/// </summary>
		[Bindable(true), DefaultValue(busyToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears over the text box when it is discovering and authenticating.")]
		public string BusyToolTip {
			get { return (string)(ViewState[busyToolTipViewStateKey] ?? busyToolTipDefault); }
			set { ViewState[busyToolTipViewStateKey] = value ?? string.Empty; }
		}

		const string identifierRequiredMessageViewStateKey = "BusyToolTip";
		const string identifierRequiredMessageDefault = "Please correct errors in OpenID identifier and allow login to complete before submitting.";
		/// <summary>
		/// Gets/sets the message that is displayed if a postback is about to occur before the identifier has been supplied.
		/// </summary>
		[Bindable(true), DefaultValue(identifierRequiredMessageDefault), Localizable(true), Category("Appearance")]
		[Description("The message that is displayed if a postback is about to occur before the identifier has been supplied.")]
		public string IdentifierRequiredMessage {
			get { return (string)(ViewState[identifierRequiredMessageViewStateKey] ?? identifierRequiredMessageDefault); }
			set { ViewState[identifierRequiredMessageViewStateKey] = value ?? string.Empty; }
		}

		const string logOnInProgressMessageViewStateKey = "BusyToolTip";
		const string logOnInProgressMessageDefault = "Please wait for login to complete.";
		/// <summary>
		/// Gets/sets the message that is displayed if a postback is attempted while login is in process.
		/// </summary>
		[Bindable(true), DefaultValue(logOnInProgressMessageDefault), Localizable(true), Category("Appearance")]
		[Description("The message that is displayed if a postback is attempted while login is in process.")]
		public string LogOnInProgressMessage {
			get { return (string)(ViewState[logOnInProgressMessageViewStateKey] ?? logOnInProgressMessageDefault); }
			set { ViewState[logOnInProgressMessageViewStateKey] = value ?? string.Empty; }
		}

		#endregion

		#region Properties to hide
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle {
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override FontInfo Font {
			get { return null; }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Height {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Width {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string ToolTip {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		bool focusCalled;
		/// <summary>
		/// Places focus on the text box when the page is rendered on the browser.
		/// </summary>
		public override void Focus() {
			focusCalled = true;
			// we don't emit the code to focus the control immediately, in case the control
			// is never rendered to the page because its Visible property is false or that
			// of any of its parent containers.
		}

		#region Events

		/// <summary>
		/// Fired when the user has typed in their identifier, discovery was successful
		/// and a login attempt is about to begin.
		/// </summary>
		[Description("Fired when the user has typed in their identifier, discovery was successful and a login attempt is about to begin.")]
		public event EventHandler<OpenIdEventArgs> LoggingIn;
		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		protected virtual void OnLoggingIn(IAuthenticationRequest request) {
			var loggingIn = LoggingIn;
			if (loggingIn != null) {
				loggingIn(this, new OpenIdEventArgs(request));
			}
		}

		/// <summary>
		/// Fired when a Provider sends back a positive assertion to this control,
		/// but the authentication has not yet been verified.
		/// </summary>
		/// <remarks>
		/// <b>No security critical decisions should be made within event handlers
		/// for this event</b> as the authenticity of the assertion has not been
		/// verified yet.  All security related code should go in the event handler
		/// for the <see cref="LoggedIn"/> event.
		/// </remarks>
		[Description("Fired when a Provider sends back a positive assertion to this control, but the authentication has not yet been verified.")]
		public event EventHandler<OpenIdEventArgs> UnconfirmedPositiveAssertion;
		/// <summary>
		/// Fires the <see cref="UnconfirmedPositiveAssertion"/> event.
		/// </summary>
		protected virtual void OnUnconfirmedPositiveAssertion() {
			var unconfirmedPositiveAssertion = UnconfirmedPositiveAssertion;
			if (unconfirmedPositiveAssertion != null) {
				unconfirmedPositiveAssertion(this, null);
			}
		}

		/// <summary>
		/// Fired when authentication has completed successfully.
		/// </summary>
		[Description("Fired when authentication has completed successfully.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		protected virtual void OnLoggedIn(IAuthenticationResponse response) {
			var loggedIn = LoggedIn;
			if (loggedIn != null) {
				loggedIn(this, new OpenIdEventArgs(response));
			}
		}

		const string onClientAssertionReceivedViewStateKey = "OnClientAssertionReceived";
		/// <summary>
		/// Gets or sets the client-side script that executes when an authentication
		/// assertion is received (but before it is verified).
		/// </summary>
		/// <remarks>
		/// <para>In the context of the executing javascript set in this property, the 
		/// local variable <i>sender</i> is set to the openid_identifier input box
		/// that is executing this code.  
		/// This variable has a getClaimedIdentifier() method that may be used to
		/// identify the user who is being authenticated.</para>
		/// <para>It is <b>very</b> important to note that when this code executes,
		/// the authentication has not been verified and may have been spoofed.
		/// No security-sensitive operations should take place in this javascript code.
		/// The authentication is verified on the server by the time the 
		/// <see cref="LoggedIn"/> server-side event fires.</para>
		/// </remarks>
		[Description("Gets or sets the client-side script that executes when an authentication assertion is received (but before it is verified).")]
		[Bindable(true), DefaultValue(""), Category("Behavior")]
		public string OnClientAssertionReceived {
			get { return ViewState[onClientAssertionReceivedViewStateKey] as string; }
			set { ViewState[onClientAssertionReceivedViewStateKey] = value; }
		}

		#endregion

		Dictionary<Type, string> clientScriptExtensions = new Dictionary<Type, string>();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void RegisterClientScriptExtension<T>(string propertyName) where T : IClientScriptExtensionResponse {
			if (String.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
			if (clientScriptExtensions.ContainsValue(propertyName)) {
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
					Strings.ClientScriptExtensionPropertyNameCollision, propertyName), "propertyName");
			}
			foreach (var ext in clientScriptExtensions.Keys) {
				if (ext == typeof(T)) {
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
						Strings.ClientScriptExtensionTypeCollision, typeof(T).FullName));
				}
			}
			clientScriptExtensions.Add(typeof(T), propertyName);
		}

		/// <summary>
		/// Prepares the control for loading.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Page.IsPostBack) {
				// If the control was temporarily hidden, it won't be in the Form data,
				// and we'll just implicitly keep the last Text setting.
				if (Page.Request.Form[Name] != null) {
					Text = Page.Request.Form[Name];
				}

				// If there is a response, and it is fresh (live object, not a snapshot object)...
				if (AuthenticationResponse != null && AuthenticationResponse is AuthenticationResponse) {
					switch (AuthenticationResponse.Status) {
						case AuthenticationStatus.Authenticated:
							OnLoggedIn(AuthenticationResponse);
							break;
						default:
							break;
					}
				}
			} else {
				string userSuppliedIdentifier = Page.Request.QueryString["dotnetopenid.userSuppliedIdentifier"];
				if (!string.IsNullOrEmpty(userSuppliedIdentifier)) {
					Logger.Info("AJAX (iframe) request detected.");
					if (Page.Request.QueryString["dotnetopenid.phase"] == "2") {
						OnUnconfirmedPositiveAssertion();
						reportDiscoveryResult();
					} else {
						performDiscovery(userSuppliedIdentifier);
					}
				}
			}
		}

		private void prepareClientJavascript() {
			// Import the .js file where most of the code is.
			Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdAjaxTextBox), EmbeddedScriptResourceName);
			// Call into the .js file with initialization information.
			StringBuilder startupScript = new StringBuilder();
			startupScript.AppendLine("<script language='javascript'>");
			startupScript.AppendFormat("var box = document.getElementsByName('{0}')[0];{1}", Name, Environment.NewLine);
			if (focusCalled) {
				startupScript.AppendLine("box.focus();");
			}
			startupScript.AppendFormat(CultureInfo.InvariantCulture,
				"initAjaxOpenId(box, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15});{16}",
				Util.GetSafeJavascriptValue(Page.ClientScript.GetWebResourceUrl(GetType(), OpenIdTextBox.EmbeddedLogoResourceName)),
				Util.GetSafeJavascriptValue(Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedDotNetOpenIdLogoResourceName)),
				Util.GetSafeJavascriptValue(Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedSpinnerResourceName)),
				Util.GetSafeJavascriptValue(Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedLoginSuccessResourceName)),
				Util.GetSafeJavascriptValue(Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedLoginFailureResourceName)),
				Timeout.TotalMilliseconds,
				string.IsNullOrEmpty(OnClientAssertionReceived) ? "null" : "'" + OnClientAssertionReceived.Replace(@"\", @"\\").Replace("'", @"\'") + "'",
				Util.GetSafeJavascriptValue(LogOnText),
				Util.GetSafeJavascriptValue(LogOnToolTip),
				Util.GetSafeJavascriptValue(RetryText),
				Util.GetSafeJavascriptValue(RetryToolTip),
				Util.GetSafeJavascriptValue(BusyToolTip),
				Util.GetSafeJavascriptValue(IdentifierRequiredMessage),
				Util.GetSafeJavascriptValue(LogOnInProgressMessage),
				Util.GetSafeJavascriptValue(AuthenticationSucceededToolTip),
				Util.GetSafeJavascriptValue(AuthenticationFailedToolTip),
				Environment.NewLine);

			if (AuthenticationResponse != null && AuthenticationResponse.Status == AuthenticationStatus.Authenticated) {
				startupScript.AppendFormat("box.dnoi_internal.openidAuthResult('{0}');{1}", ViewState[authDataViewStateKey].ToString().Replace("'", "\\'"), Environment.NewLine);
			}
			startupScript.AppendLine("</script>");

			Page.ClientScript.RegisterStartupScript(GetType(), "ajaxstartup", startupScript.ToString());
			Page.ClientScript.RegisterOnSubmitStatement(GetType(), "loginvalidation", string.Format(CultureInfo.InvariantCulture, @"
var openidbox = document.getElementsByName('{0}')[0];
if (!openidbox.dnoi_internal.onSubmit()) {{ return false; }}
", Name));
		}

		private void performDiscovery(string userSuppliedIdentifier) {
			Logger.InfoFormat("Discovery on {0} requested.", userSuppliedIdentifier);
			OpenIdRelyingParty rp = new OpenIdRelyingParty();

			try {
				IAuthenticationRequest req = rp.CreateRequest(userSuppliedIdentifier);
				req.AddCallbackArguments("dotnetopenid.phase", "2");
				if (Page.Request.QueryString["dotnetopenid.immediate"] == "true") {
					req.Mode = AuthenticationRequestMode.Immediate;
				}
				OnLoggingIn(req);
				req.RedirectToProvider();
			} catch (OpenIdException ex) {
				callbackUserAgentMethod("dnoi_internal.openidDiscoveryFailure('" + ex.Message.Replace("'", "\\'") + "')");
			}
		}

		private void reportDiscoveryResult() {
			Logger.InfoFormat("AJAX (iframe) callback from OP: {0}", Page.Request.Url);
			List<string> assignments = new List<string>();

			OpenIdRelyingParty rp = new OpenIdRelyingParty();
			var f = Util.NameValueCollectionToDictionary(HttpUtility.ParseQueryString(Page.Request.Url.Query));
			var authResponse = RelyingParty.AuthenticationResponse.Parse(f, rp, Page.Request.Url, false);
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				foreach (var pair in clientScriptExtensions) {
					string js = authResponse.GetExtensionClientScript(pair.Key);
					if (string.IsNullOrEmpty(js)) {
						js = "null";
					}
					assignments.Add(pair.Value + " = " + js);
				}
			}

			callbackUserAgentMethod("dnoi_internal.openidAuthResult(document.URL)", assignments.ToArray());
		}

		/// <summary>
		/// Prepares to render the control.
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			prepareClientJavascript();
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			// We surround the textbox with a span so that the .js file can inject a
			// login button within the text box with easy placement.
			writer.WriteBeginTag("span");
			writer.Write(" style='");
			writer.WriteStyleAttribute("position", "relative");
			writer.Write("'>");

			writer.WriteBeginTag("input");
			writer.WriteAttribute("name", Name);
			writer.WriteAttribute("value", Text);
			writer.WriteAttribute("size", Columns.ToString(CultureInfo.InvariantCulture));
			if (TabIndex > 0) {
				writer.WriteAttribute("tabindex", TabIndex.ToString(CultureInfo.InvariantCulture));
			}
			if (!Enabled) {
				writer.WriteAttribute("disabled", "true");
			}
			if (!string.IsNullOrEmpty(CssClass)) {
				writer.WriteAttribute("class", CssClass);
			}
			writer.Write(" style='");
			writer.WriteStyleAttribute("padding-left", "18px");
			writer.WriteStyleAttribute("border-style", "solid");
			writer.WriteStyleAttribute("border-width", "1px");
			writer.WriteStyleAttribute("border-color", "lightgray");
			writer.Write("'");
			writer.Write(" />");

			writer.WriteEndTag("span");
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private void callbackUserAgentMethod(string methodCall) {
			callbackUserAgentMethod(methodCall, null);
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		/// <param name="preAssignments">An optional list of assignments to make to the input box object before placing the method call.</param>
		private void callbackUserAgentMethod(string methodCall, string[] preAssignments) {
			Logger.InfoFormat("Sending Javascript callback: {0}", methodCall);
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
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture,
@"	if (inPopup) {{
	objSrc.{0};
	window.self.close();
}} else {{
	objSrc.{0};
}}
</script></body></html>", methodCall));
			Page.Response.End();
		}
	}
}
