//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxTextBox.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedDotNetOpenIdLogoResourceName, "image/gif")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedSpinnerResourceName, "image/gif")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName, "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginFailureResourceName, "image/png")]

#pragma warning disable 0809 // marking inherited, unsupported properties as obsolete to discourage their use

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;

	/// <summary>
	/// An ASP.NET control that provides a minimal text box that is OpenID-aware and uses AJAX for
	/// a premium login experience.
	/// </summary>
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdAjaxTextBox runat=\"server\" />")]
	public sealed class OpenIdAjaxTextBox : WebControl, ICallbackEventHandler {
		/// <summary>
		/// The name of the manifest stream containing the OpenIdAjaxTextBox.js file.
		/// </summary>
		internal const string EmbeddedScriptResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdAjaxTextBox.js";

		/// <summary>
		/// The name of the manifest stream containing the dotnetopenid_16x16.gif file.
		/// </summary>
		internal const string EmbeddedDotNetOpenIdLogoResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.dotnetopenid_16x16.gif";

		/// <summary>
		/// The name of the manifest stream containing the spinner.gif file.
		/// </summary>
		internal const string EmbeddedSpinnerResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.spinner.gif";

		/// <summary>
		/// The name of the manifest stream containing the login_success.png file.
		/// </summary>
		internal const string EmbeddedLoginSuccessResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.login_success.png";

		/// <summary>
		/// The name of the manifest stream containing the login_failure.png file.
		/// </summary>
		internal const string EmbeddedLoginFailureResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.login_failure.png";

		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Columns"/> property.
		/// </summary>
		private const string ColumnsViewStateKey = "Columns";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="OnClientAssertionReceived"/> property.
		/// </summary>
		private const string OnClientAssertionReceivedViewStateKey = "OnClientAssertionReceived";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticationResponse"/> property.
		/// </summary>
		private const string AuthenticationResponseViewStateKey = "AuthenticationResponse";

		/// <summary>
		/// The viewstate key to use for storing the value of the a successful authentication.
		/// </summary>
		private const string AuthDataViewStateKey = "AuthData";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticatedAsToolTip"/> property.
		/// </summary>
		private const string AuthenticatedAsToolTipViewStateKey = "AuthenticatedAsToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticationSucceededToolTip"/> property.
		/// </summary>
		private const string AuthenticationSucceededToolTipViewStateKey = "AuthenticationSucceededToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlViewStateKey = "ReturnToUrl";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlViewStateKey = "RealmUrl";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="LogOnInProgressMessage"/> property.
		/// </summary>
		private const string LogOnInProgressMessageViewStateKey = "BusyToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticationFailedToolTip"/> property.
		/// </summary>
		private const string AuthenticationFailedToolTipViewStateKey = "AuthenticationFailedToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="IdentifierRequiredMessage"/> property.
		/// </summary>
		private const string IdentifierRequiredMessageViewStateKey = "BusyToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="BusyToolTip"/> property.
		/// </summary>
		private const string BusyToolTipViewStateKey = "BusyToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="LogOnText"/> property.
		/// </summary>
		private const string LogOnTextViewStateKey = "LoginText";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Throttle"/> property.
		/// </summary>
		private const string ThrottleViewStateKey = "Throttle";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="LogOnToolTip"/> property.
		/// </summary>
		private const string LogOnToolTipViewStateKey = "LoginToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Name"/> property.
		/// </summary>
		private const string NameViewStateKey = "Name";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Timeout"/> property.
		/// </summary>
		private const string TimeoutViewStateKey = "Timeout";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Text"/> property.
		/// </summary>
		private const string TextViewStateKey = "Text";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="TabIndex"/> property.
		/// </summary>
		private const string TabIndexViewStateKey = "TabIndex";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="RetryToolTip"/> property.
		/// </summary>
		private const string RetryToolTipViewStateKey = "RetryToolTip";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="RetryText"/> property.
		/// </summary>
		private const string RetryTextViewStateKey = "RetryText";

		#endregion

		#region Property defaults

		/// <summary>
		/// The default value for the <see cref="Columns"/> property.
		/// </summary>
		private const int ColumnsDefault = 40;

		/// <summary>
		/// The default value for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlDefault = "";

		/// <summary>
		/// The default value for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlDefault = "~/";

		/// <summary>
		/// The default value for the <see cref="LogOnInProgressMessage"/> property.
		/// </summary>
		private const string LogOnInProgressMessageDefault = "Please wait for login to complete.";

		/// <summary>
		/// The default value for the <see cref="AuthenticationSucceededToolTip"/> property.
		/// </summary>
		private const string AuthenticationSucceededToolTipDefault = "Authenticated by {0}.";

		/// <summary>
		/// The default value for the <see cref="AuthenticatedAsToolTip"/> property.
		/// </summary>
		private const string AuthenticatedAsToolTipDefault = "Authenticated as {0}.";

		/// <summary>
		/// The default value for the <see cref="AuthenticationFailedToolTip"/> property.
		/// </summary>
		private const string AuthenticationFailedToolTipDefault = "Authentication failed.";

		/// <summary>
		/// The default value for the <see cref="Throttle"/> property.
		/// </summary>
		private const int ThrottleDefault = 3;

		/// <summary>
		/// The default value for the <see cref="LogOnText"/> property.
		/// </summary>
		private const string LogOnTextDefault = "LOG IN";

		/// <summary>
		/// The default value for the <see cref="BusyToolTip"/> property.
		/// </summary>
		private const string BusyToolTipDefault = "Discovering/authenticating";

		/// <summary>
		/// The default value for the <see cref="IdentifierRequiredMessage"/> property.
		/// </summary>
		private const string IdentifierRequiredMessageDefault = "Please correct errors in OpenID identifier and allow login to complete before submitting.";

		/// <summary>
		/// The default value for the <see cref="Name"/> property.
		/// </summary>
		private const string NameDefault = "openid_identifier";

		/// <summary>
		/// Default value for <see cref="TabIndex"/> property.
		/// </summary>
		private const short TabIndexDefault = 0;

		/// <summary>
		/// The default value for the <see cref="RetryToolTip"/> property.
		/// </summary>
		private const string RetryToolTipDefault = "Retry a failed identifier discovery.";

		/// <summary>
		/// The default value for the <see cref="LogOnToolTip"/> property.
		/// </summary>
		private const string LogOnToolTipDefault = "Click here to log in using a pop-up window.";

		/// <summary>
		/// The default value for the <see cref="RetryText"/> property.
		/// </summary>
		private const string RetryTextDefault = "RETRY";

		#endregion

		/// <summary>
		/// Tracks whether the text box should receive input focus when the page is rendered.
		/// </summary>
		private bool focusCalled;

		/// <summary>
		/// The authentication response that just came in.
		/// </summary>
		private IAuthenticationResponse authenticationResponse;

		/// <summary>
		/// A dictionary of extension response types and the javascript member 
		/// name to map them to on the user agent.
		/// </summary>
		private Dictionary<Type, string> clientScriptExtensions = new Dictionary<Type, string>();

		/// <summary>
		/// Stores the result of an AJAX discovery request while it is waiting
		/// to be picked up by ASP.NET on the way down to the user agent.
		/// </summary>
		private string discoveryResult;

		#region Events

		/// <summary>
		/// Fired when the user has typed in their identifier, discovery was successful
		/// and a login attempt is about to begin.
		/// </summary>
		[Description("Fired when the user has typed in their identifier, discovery was successful and a login attempt is about to begin.")]
		public event EventHandler<OpenIdEventArgs> LoggingIn;

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
		/// Fired when authentication has completed successfully.
		/// </summary>
		[Description("Fired when authentication has completed successfully.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;

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
			get { return this.ViewState[OnClientAssertionReceivedViewStateKey] as string; }
			set { this.ViewState[OnClientAssertionReceivedViewStateKey] = value; }
		}

		#endregion

		#region Properties

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
							Url = authUri,
						};
						using (var rp = CreateRelyingParty(true)) {
							this.authenticationResponse = rp.GetResponse(clientResponseInfo);

							// Save out the authentication response to viewstate so we can find it on
							// a subsequent postback.
							this.ViewState[AuthenticationResponseViewStateKey] = new AuthenticationResponseSnapshot(this.authenticationResponse);
						}
					} else {
						this.authenticationResponse = viewstateResponse;
					}
				}
				return this.authenticationResponse;
			}
		}

		/// <summary>
		/// Gets or sets the value in the text field, completely unprocessed or normalized.
		/// </summary>
		[Bindable(true), DefaultValue(""), Category("Appearance")]
		[Description("The value in the text field, completely unprocessed or normalized.")]
		public string Text {
			get { return (string)(this.ViewState[TextViewStateKey] ?? string.Empty); }
			set { this.ViewState[TextViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the width of the text box in characters.
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(ColumnsDefault)]
		[Description("The width of the text box in characters.")]
		public int Columns {
			get {
				return (int)(this.ViewState[ColumnsViewStateKey] ?? ColumnsDefault);
			}

			set {
				ErrorUtilities.VerifyArgumentInRange(value >= 0, "value");
				this.ViewState[ColumnsViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the tab index of the text box control.  Use 0 to omit an explicit tabindex.
		/// </summary>
		[Bindable(true), Category("Behavior"), DefaultValue(TabIndexDefault)]
		[Description("The tab index of the text box control.  Use 0 to omit an explicit tabindex.")]
		public override short TabIndex {
			get { return (short)(this.ViewState[TabIndexViewStateKey] ?? TabIndexDefault); }
			set { this.ViewState[TabIndexViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the HTML name to assign to the text field.
		/// </summary>
		[Bindable(true), DefaultValue(NameDefault), Category("Misc")]
		[Description("The HTML name to assign to the text field.")]
		public string Name {
			get {
				return (string)(this.ViewState[NameViewStateKey] ?? NameDefault);
			}

			set {
				ErrorUtilities.VerifyNonZeroLength(value, "value");
				this.ViewState[NameViewStateKey] = value ?? string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.
		/// </summary>
		[Browsable(true), DefaultValue(typeof(TimeSpan), "00:00:01"), Category("Behavior")]
		[Description("The time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.")]
		public TimeSpan Timeout {
			get {
				return (TimeSpan)(this.ViewState[TimeoutViewStateKey] ?? TimeoutDefault);
			}

			set {
				ErrorUtilities.VerifyArgumentInRange(value.TotalMilliseconds > 0, "value");
				this.ViewState[TimeoutViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of OpenID Providers to simultaneously try to authenticate with.
		/// </summary>
		[Browsable(true), DefaultValue(ThrottleDefault), Category("Behavior")]
		[Description("The maximum number of OpenID Providers to simultaneously try to authenticate with.")]
		public int Throttle {
			get {
				return (int)(this.ViewState[ThrottleViewStateKey] ?? ThrottleDefault);
			}

			set {
				ErrorUtilities.VerifyArgumentInRange(value > 0, "value");
				this.ViewState[ThrottleViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(LogOnTextDefault), Localizable(true), Category("Appearance")]
		[Description("The text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.")]
		public string LogOnText {
			get {
				return (string)(this.ViewState[LogOnTextViewStateKey] ?? LogOnTextDefault);
			}

			set {
				ErrorUtilities.VerifyNonZeroLength(value, "value");
				this.ViewState[LogOnTextViewStateKey] = value ?? string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the rool tip text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(LogOnToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.")]
		public string LogOnToolTip {
			get { return (string)(this.ViewState[LogOnToolTipViewStateKey] ?? LogOnToolTipDefault); }
			set { this.ViewState[LogOnToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the text that appears on the RETRY button in cases where authentication times out.
		/// </summary>
		[Bindable(true), DefaultValue(RetryTextDefault), Localizable(true), Category("Appearance")]
		[Description("The text that appears on the RETRY button in cases where authentication times out.")]
		public string RetryText {
			get {
				return (string)(this.ViewState[RetryTextViewStateKey] ?? RetryTextDefault);
			}

			set {
				ErrorUtilities.VerifyNonZeroLength(value, "value");
				this.ViewState[RetryTextViewStateKey] = value ?? string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears on the RETRY button in cases where authentication times out.
		/// </summary>
		[Bindable(true), DefaultValue(RetryToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears on the RETRY button in cases where authentication times out.")]
		public string RetryToolTip {
			get { return (string)(this.ViewState[RetryToolTipViewStateKey] ?? RetryToolTipDefault); }
			set { this.ViewState[RetryToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears when authentication succeeds.
		/// </summary>
		[Bindable(true), DefaultValue(AuthenticationSucceededToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears when authentication succeeds.")]
		public string AuthenticationSucceededToolTip {
			get { return (string)(this.ViewState[AuthenticationSucceededToolTipViewStateKey] ?? AuthenticationSucceededToolTipDefault); }
			set { this.ViewState[AuthenticationSucceededToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears on the green checkmark when authentication succeeds.
		/// </summary>
		[Bindable(true), DefaultValue(AuthenticatedAsToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears on the green checkmark when authentication succeeds.")]
		public string AuthenticatedAsToolTip {
			get { return (string)(this.ViewState[AuthenticatedAsToolTipViewStateKey] ?? AuthenticatedAsToolTipDefault); }
			set { this.ViewState[AuthenticatedAsToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears when authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(AuthenticationFailedToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears when authentication fails.")]
		public string AuthenticationFailedToolTip {
			get { return (string)(this.ViewState[AuthenticationFailedToolTipViewStateKey] ?? AuthenticationFailedToolTipDefault); }
			set { this.ViewState[AuthenticationFailedToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears over the text box when it is discovering and authenticating.
		/// </summary>
		[Bindable(true), DefaultValue(BusyToolTipDefault), Localizable(true), Category("Appearance")]
		[Description("The tool tip text that appears over the text box when it is discovering and authenticating.")]
		public string BusyToolTip {
			get { return (string)(this.ViewState[BusyToolTipViewStateKey] ?? BusyToolTipDefault); }
			set { this.ViewState[BusyToolTipViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the message that is displayed if a postback is about to occur before the identifier has been supplied.
		/// </summary>
		[Bindable(true), DefaultValue(IdentifierRequiredMessageDefault), Localizable(true), Category("Appearance")]
		[Description("The message that is displayed if a postback is about to occur before the identifier has been supplied.")]
		public string IdentifierRequiredMessage {
			get { return (string)(this.ViewState[IdentifierRequiredMessageViewStateKey] ?? IdentifierRequiredMessageDefault); }
			set { this.ViewState[IdentifierRequiredMessageViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the message that is displayed if a postback is attempted while login is in process.
		/// </summary>
		[Bindable(true), DefaultValue(LogOnInProgressMessageDefault), Localizable(true), Category("Appearance")]
		[Description("The message that is displayed if a postback is attempted while login is in process.")]
		public string LogOnInProgressMessage {
			get { return (string)(this.ViewState[LogOnInProgressMessageViewStateKey] ?? LogOnInProgressMessageDefault); }
			set { this.ViewState[LogOnInProgressMessageViewStateKey] = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Property grid on form designer only supports primitive types.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid on form designer only supports primitive types.")]
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(RealmUrlDefault)]
		[Description("The OpenID Realm of the relying party web site.")]
		public string RealmUrl {
			get {
				return (string)(this.ViewState[RealmUrlViewStateKey] ?? RealmUrlDefault);
			}

			set {
				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Realm object based on it.
					new Realm(OpenIdUtilities.GetResolvedRealm(Page, value)); // throws an exception on failure.
				} else {
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://")) {
						new Uri(value.Replace("*.", "")); // make sure it's fully-qualified, but ignore wildcards
					} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
						// this is valid too
					} else {
						throw new UriFormatException();
					}
				}
				this.ViewState[RealmUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the OpenID ReturnTo of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Property grid on form designer only supports primitive types.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid on form designer only supports primitive types.")]
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(ReturnToUrlDefault)]
		[Description("The OpenID ReturnTo of the relying party web site.")]
		public string ReturnToUrl {
			get {
				return (string)(this.ViewState[ReturnToUrlViewStateKey] ?? ReturnToUrlDefault);
			}

			set {
				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Uri based on it.
					new Uri(MessagingUtilities.GetRequestUrlFromContext(), Page.ResolveUrl(value)); // throws an exception on failure.
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

		#endregion

		#region Properties to hide

		/// <summary>
		/// Gets or sets the foreground color (typically the color of the text) of the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the foreground color of the control. The default is <see cref="F:System.Drawing.Color.Empty"/>.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the background color of the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the background color of the control. The default is <see cref="F:System.Drawing.Color.Empty"/>, which indicates that this property is not set.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the border color of the Web control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the border color of the control. The default is <see cref="F:System.Drawing.Color.Empty"/>, which indicates that this property is not set.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the border width of the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.UI.WebControls.Unit"/> that represents the border width of a Web server control. The default value is <see cref="F:System.Web.UI.WebControls.Unit.Empty"/>, which indicates that this property is not set.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The specified border width is a negative value.
		/// </exception>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override Unit BorderWidth {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the border style of the Web server control.
		/// </summary>
		/// <returns>
		/// One of the <see cref="T:System.Web.UI.WebControls.BorderStyle"/> enumeration values. The default is NotSet.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle {
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets the font properties associated with the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.UI.WebControls.FontInfo"/> that represents the font properties of the Web server control.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override FontInfo Font {
			get { return null; }
		}

		/// <summary>
		/// Gets or sets the height of the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.UI.WebControls.Unit"/> that represents the height of the control. The default is <see cref="F:System.Web.UI.WebControls.Unit.Empty"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The height was set to a negative value.
		/// </exception>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override Unit Height {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the width of the Web server control.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.UI.WebControls.Unit"/> that represents the width of the control. The default is <see cref="F:System.Web.UI.WebControls.Unit.Empty"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The width of the Web server control was set to a negative value.
		/// </exception>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override Unit Width {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the text displayed when the mouse pointer hovers over the Web server control.
		/// </summary>
		/// <returns>
		/// The text displayed when the mouse pointer hovers over the Web server control. The default is <see cref="F:System.String.Empty"/>.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override string ToolTip {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets the skin to apply to the control.
		/// </summary>
		/// <returns>
		/// The name of the skin to apply to the control. The default is <see cref="F:System.String.Empty"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The skin specified in the <see cref="P:System.Web.UI.WebControls.WebControl.SkinID"/> property does not exist in the theme.
		/// </exception>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether themes apply to this control.
		/// </summary>
		/// <returns>true to use themes; otherwise, false. The default is false.
		/// </returns>
		[Obsolete("This property does not do anything."), Browsable(false), Bindable(false)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		/// <summary>
		/// Gets the default value for the <see cref="Timeout"/> property.
		/// </summary>
		/// <value>8 seconds; or eternity if the debugger is attached.</value>
		private static TimeSpan TimeoutDefault {
			get {
				if (Debugger.IsAttached) {
					Logger.Warn("Debugger is attached.  Inflating default OpenIdAjaxTextbox.Timeout value to infinity.");
					return TimeSpan.MaxValue;
				} else {
					return TimeSpan.FromSeconds(8);
				}
			}
		}

		/// <summary>
		/// Gets the name of the open id auth data form key.
		/// </summary>
		/// <value>A concatenation of <see cref="Name"/> and <c>"_openidAuthData"</c>.</value>
		private string OpenIdAuthDataFormKey {
			get { return this.Name + "_openidAuthData"; }
		}

		/// <summary>
		/// Places focus on the text box when the page is rendered on the browser.
		/// </summary>
		public override void Focus() {
			// we don't emit the code to focus the control immediately, in case the control
			// is never rendered to the page because its Visible property is false or that
			// of any of its parent containers.
			this.focusCalled = true;
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
			ErrorUtilities.VerifyNonZeroLength(propertyName, "propertyName");
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
			Logger.InfoFormat("AJAX discovery on {0} requested.", userSuppliedIdentifier);

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
				List<IAuthenticationRequest> requests = this.CreateRequests(userSuppliedIdentifier, true);
				if (requests.Count > 0) {
					discoveryResultBuilder.AppendFormat("claimedIdentifier: {0},", MessagingUtilities.GetSafeJavascriptValue(requests[0].ClaimedIdentifier));
					discoveryResultBuilder.Append("requests: [");
					foreach (IAuthenticationRequest request in requests) {
						this.OnLoggingIn(request);
						discoveryResultBuilder.Append("{");
						discoveryResultBuilder.AppendFormat("endpoint: {0},", MessagingUtilities.GetSafeJavascriptValue(request.Provider.Uri.AbsoluteUri));
						request.Mode = AuthenticationRequestMode.Immediate;
						UserAgentResponse response = request.RedirectingResponse;
						discoveryResultBuilder.AppendFormat("immediate: {0},", MessagingUtilities.GetSafeJavascriptValue(response.DirectUriRequest.AbsoluteUri));
						request.Mode = AuthenticationRequestMode.Setup;
						response = request.RedirectingResponse;
						discoveryResultBuilder.AppendFormat("setup: {0}", MessagingUtilities.GetSafeJavascriptValue(response.DirectUriRequest.AbsoluteUri));
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
		/// Prepares the control for loading.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (this.Page.IsPostBack) {
				// If the control was temporarily hidden, it won't be in the Form data,
				// and we'll just implicitly keep the last Text setting.
				if (this.Page.Request.Form[this.Name] != null) {
					this.Text = this.Page.Request.Form[this.Name];
				}

				// If there is a response, and it is fresh (live object, not a snapshot object)...
				if (this.AuthenticationResponse != null && this.AuthenticationResponse.Status == AuthenticationStatus.Authenticated) {
					this.OnLoggedIn(this.AuthenticationResponse);
				}
			} else {
				NameValueCollection query = MessagingUtilities.GetQueryOrFormFromContext();
				string userSuppliedIdentifier = query["dotnetopenid.userSuppliedIdentifier"];
				if (!string.IsNullOrEmpty(userSuppliedIdentifier) && query["dotnetopenid.phase"] == "2") {
					this.ReportAuthenticationResult();
				}
			}
		}

		/// <summary>
		/// Prepares to render the control.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.PrepareClientJavascript();
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			// We surround the textbox with a span so that the .js file can inject a
			// login button within the text box with easy placement.
			writer.WriteBeginTag("span");
			writer.WriteAttribute("class", this.CssClass);
			writer.Write(" style='");
			writer.WriteStyleAttribute("position", "relative");
			writer.WriteStyleAttribute("font-size", "16px");
			writer.Write("'>");

			writer.WriteBeginTag("input");
			writer.WriteAttribute("name", this.Name);
			writer.WriteAttribute("id", this.ClientID);
			writer.WriteAttribute("value", this.Text, true);
			writer.WriteAttribute("size", this.Columns.ToString(CultureInfo.InvariantCulture));
			if (this.TabIndex > 0) {
				writer.WriteAttribute("tabindex", this.TabIndex.ToString(CultureInfo.InvariantCulture));
			}
			if (!this.Enabled) {
				writer.WriteAttribute("disabled", "true");
			}
			if (!string.IsNullOrEmpty(this.CssClass)) {
				writer.WriteAttribute("class", this.CssClass);
			}
			writer.Write(" style='");
			writer.WriteStyleAttribute("padding-left", "18px");
			writer.WriteStyleAttribute("border-style", "solid");
			writer.WriteStyleAttribute("border-width", "1px");
			writer.WriteStyleAttribute("border-color", "lightgray");
			writer.Write("'");
			writer.Write(" />");

			writer.WriteEndTag("span");

			// Emit a hidden field to let the javascript on the user agent know if an
			// authentication has already successfully taken place.
			string viewstateAuthData = this.ViewState[AuthDataViewStateKey] as string;
			if (!string.IsNullOrEmpty(viewstateAuthData)) {
				writer.WriteBeginTag("input");
				writer.WriteAttribute("type", "hidden");
				writer.WriteAttribute("name", this.OpenIdAuthDataFormKey);
				writer.WriteAttribute("value", viewstateAuthData, true);
				writer.Write(" />");
			}
		}

		/// <summary>
		/// Filters a sequence of OP endpoints so that an OP hostname only appears once in the list.
		/// </summary>
		/// <param name="requests">The authentication requests against those OP endpoints.</param>
		/// <returns>The filtered list.</returns>
		private static List<IAuthenticationRequest> RemoveDuplicateEndpoints(List<IAuthenticationRequest> requests) {
			var filteredRequests = new List<IAuthenticationRequest>(requests.Count);
			foreach (IAuthenticationRequest request in requests) {
				// We'll distinguish based on the host name only, which
				// admittedly is only a heuristic, but if we remove one that really wasn't a duplicate, well,
				// this multiple OP attempt thing was just a convenience feature anyway.
				if (!filteredRequests.Any(req => string.Equals(req.Provider.Uri.Host, request.Provider.Uri.Host, StringComparison.OrdinalIgnoreCase))) {
					filteredRequests.Add(request);
				}
			}

			return filteredRequests;
		}

		/// <summary>
		/// Creates the relying party.
		/// </summary>
		/// <param name="verifySignature">
		/// A value indicating whether message protections should be applied to the processed messages.
		/// Use <c>false</c> to postpone verification to a later time without invalidating nonces.
		/// </param>
		/// <returns>The newly instantiated relying party.</returns>
		private static OpenIdRelyingParty CreateRelyingParty(bool verifySignature) {
			return verifySignature ? new OpenIdRelyingParty() : OpenIdRelyingParty.CreateNonVerifying();
		}

		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		/// <param name="request">The request.</param>
		private void OnLoggingIn(IAuthenticationRequest request) {
			var loggingIn = this.LoggingIn;
			if (loggingIn != null) {
				loggingIn(this, new OpenIdEventArgs(request));
			}
		}

		/// <summary>
		/// Fires the <see cref="UnconfirmedPositiveAssertion"/> event.
		/// </summary>
		private void OnUnconfirmedPositiveAssertion() {
			var unconfirmedPositiveAssertion = this.UnconfirmedPositiveAssertion;
			if (unconfirmedPositiveAssertion != null) {
				unconfirmedPositiveAssertion(this, null);
			}
		}

		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		private void OnLoggedIn(IAuthenticationResponse response) {
			var loggedIn = this.LoggedIn;
			if (loggedIn != null) {
				loggedIn(this, new OpenIdEventArgs(response));
			}
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
		/// Assembles the javascript to send to the client and registers it with ASP.NET for transmission.
		/// </summary>
		private void PrepareClientJavascript() {
			string identifierParameterName = "identifier";
			string discoveryCallbackResultParameterName = "resultFunction";
			string discoveryErrorCallbackParameterName = "errorCallback";
			string discoveryCallback = Page.ClientScript.GetCallbackEventReference(
				this,
				identifierParameterName,
				discoveryCallbackResultParameterName,
				identifierParameterName,
				discoveryErrorCallbackParameterName,
				true);

			// Import the .js file where most of the code is.
			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdAjaxTextBox), EmbeddedScriptResourceName);

			// Call into the .js file with initialization information.
			StringBuilder startupScript = new StringBuilder();
			startupScript.AppendLine("<script language='javascript'>");
			startupScript.AppendFormat("var box = document.getElementsByName('{0}')[0];{1}", this.Name, Environment.NewLine);
			if (this.focusCalled) {
				startupScript.AppendLine("box.focus();");
			}
			startupScript.AppendFormat(
				CultureInfo.InvariantCulture,
				"initAjaxOpenId(box, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, function({18}, {19}, {20}) {{{21}}});{22}",
				MessagingUtilities.GetSafeJavascriptValue(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), OpenIdTextBox.EmbeddedLogoResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), EmbeddedDotNetOpenIdLogoResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), EmbeddedSpinnerResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), EmbeddedLoginSuccessResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), EmbeddedLoginFailureResourceName)),
				this.Throttle,
				this.Timeout.TotalMilliseconds,
				string.IsNullOrEmpty(this.OnClientAssertionReceived) ? "null" : "'" + this.OnClientAssertionReceived.Replace(@"\", @"\\").Replace("'", @"\'") + "'",
				MessagingUtilities.GetSafeJavascriptValue(this.LogOnText),
				MessagingUtilities.GetSafeJavascriptValue(this.LogOnToolTip),
				MessagingUtilities.GetSafeJavascriptValue(this.RetryText),
				MessagingUtilities.GetSafeJavascriptValue(this.RetryToolTip),
				MessagingUtilities.GetSafeJavascriptValue(this.BusyToolTip),
				MessagingUtilities.GetSafeJavascriptValue(this.IdentifierRequiredMessage),
				MessagingUtilities.GetSafeJavascriptValue(this.LogOnInProgressMessage),
				MessagingUtilities.GetSafeJavascriptValue(this.AuthenticationSucceededToolTip),
				MessagingUtilities.GetSafeJavascriptValue(this.AuthenticatedAsToolTip),
				MessagingUtilities.GetSafeJavascriptValue(this.AuthenticationFailedToolTip),
				identifierParameterName,
				discoveryCallbackResultParameterName,
				discoveryErrorCallbackParameterName,
				discoveryCallback,
				Environment.NewLine);

			startupScript.AppendLine("</script>");

			Page.ClientScript.RegisterStartupScript(this.GetType(), "ajaxstartup", startupScript.ToString());
			string htmlFormat = @"
var openidbox = document.getElementsByName('{0}')[0];
if (!openidbox.dnoi_internal.onSubmit()) {{ return false; }}
";
			Page.ClientScript.RegisterOnSubmitStatement(
				this.GetType(),
				"loginvalidation",
				string.Format(CultureInfo.InvariantCulture, htmlFormat, this.Name));
		}

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="immediate">A value indicating whether the authentication 
		/// requests should be initialized for use in invisible iframes for background authentication.</param>
		/// <returns>The list of authentication requests, any one of which may be 
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier"/>.</returns>
		private List<IAuthenticationRequest> CreateRequests(string userSuppliedIdentifier, bool immediate) {
			var requests = new List<IAuthenticationRequest>();

			using (OpenIdRelyingParty rp = CreateRelyingParty(true)) {
				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = OpenIdUtilities.GetResolvedRealm(this.Page, this.RealmUrl);
				realm.Scheme = Page.Request.Url.Scheme;
				realm.Port = Page.Request.Url.Port;

				// Initiate openid request
				// We use TryParse here to avoid throwing an exception which 
				// might slip through our validator control if it is disabled.
				Realm typedRealm = new Realm(realm);
				if (string.IsNullOrEmpty(this.ReturnToUrl)) {
					requests.AddRange(rp.CreateRequests(userSuppliedIdentifier, typedRealm));
				} else {
					Uri returnTo = new Uri(MessagingUtilities.GetRequestUrlFromContext(), this.ReturnToUrl);
					requests.AddRange(rp.CreateRequests(userSuppliedIdentifier, typedRealm, returnTo));
				}

				// Some OPs may be listed multiple times (one with HTTPS and the other with HTTP, for example).
				// Since we're gathering OPs to try one after the other, just take the first choice of each OP
				// and don't try it multiple times.
				requests = RemoveDuplicateEndpoints(requests);

				// Configure each generated request.
				int reqIndex = 0;
				foreach (var req in requests) {
					req.AddCallbackArguments("index", (reqIndex++).ToString(CultureInfo.InvariantCulture));

					// If the ReturnToUrl was explicitly set, we'll need to reset our first parameter
					if (string.IsNullOrEmpty(HttpUtility.ParseQueryString(req.ReturnToUrl.Query)["dotnetopenid.userSuppliedIdentifier"])) {
						req.AddCallbackArguments("dotnetopenid.userSuppliedIdentifier", userSuppliedIdentifier);
					}

					// Our javascript needs to let the user know which endpoint responded.  So we force it here.
					// This gives us the info even for 1.0 OPs and 2.0 setup_required responses.
					req.AddCallbackArguments("dotnetopenid.op_endpoint", req.Provider.Uri.AbsoluteUri);
					req.AddCallbackArguments("dotnetopenid.claimed_id", req.ClaimedIdentifier);
					req.AddCallbackArguments("dotnetopenid.phase", "2");
					if (immediate) {
						req.Mode = AuthenticationRequestMode.Immediate;
						((AuthenticationRequest)req).AssociationPreference = AssociationPreference.IfAlreadyEstablished;
					}
				}
			}

			return requests;
		}

		/// <summary>
		/// Notifies the user agent via an AJAX response of a completed authentication attempt.
		/// </summary>
		private void ReportAuthenticationResult() {
			Logger.InfoFormat("AJAX (iframe) callback from OP: {0}", this.Page.Request.Url);
			List<string> assignments = new List<string>();

			using (OpenIdRelyingParty rp = CreateRelyingParty(false)) {
				var authResponse = rp.GetResponse();
				if (authResponse.Status == AuthenticationStatus.Authenticated) {
					this.OnUnconfirmedPositiveAssertion();
					foreach (var pair in this.clientScriptExtensions) {
						IClientScriptExtensionResponse extension = (IClientScriptExtensionResponse)authResponse.GetExtension(pair.Key);
						var positiveResponse = (PositiveAuthenticationResponse)authResponse;
						string js = extension.InitializeJavaScriptData(positiveResponse.Response);
						if (string.IsNullOrEmpty(js)) {
							js = "null";
						}
						assignments.Add(pair.Value + " = " + js);
					}
				}
			}

			this.CallbackUserAgentMethod("dnoi_internal.processAuthorizationResult(document.URL)", assignments.ToArray());
		}
	}
}
