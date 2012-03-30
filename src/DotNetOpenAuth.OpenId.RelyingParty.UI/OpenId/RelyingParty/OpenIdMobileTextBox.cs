//-----------------------------------------------------------------------
// <copyright file="OpenIdMobileTextBox.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdMobileTextBox.EmbeddedLogoResourceName, "image/gif")]

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.MobileControls;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// An ASP.NET control for mobile devices that provides a minimal text box that is OpenID-aware.
	/// </summary>
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdMobileTextBox runat=\"server\" />")]
	public class OpenIdMobileTextBox : TextBox {
		/// <summary>
		/// The name of the manifest stream containing the
		/// OpenID logo that is placed inside the text box.
		/// </summary>
		internal const string EmbeddedLogoResourceName = OpenIdTextBox.EmbeddedLogoResourceName;

		/// <summary>
		/// Default value of <see cref="UsePersistentCookie"/>.
		/// </summary>
		protected const bool UsePersistentCookieDefault = false;

		#region Property category constants

		/// <summary>
		/// The "Appearance" category for properties.
		/// </summary>
		private const string AppearanceCategory = "Appearance";

		/// <summary>
		/// The "Simple Registration" category for properties.
		/// </summary>
		private const string ProfileCategory = "Simple Registration";

		/// <summary>
		/// The "Behavior" category for properties.
		/// </summary>
		private const string BehaviorCategory = "Behavior";

		#endregion

		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestEmail"/> property.
		/// </summary>
		private const string RequestEmailViewStateKey = "RequestEmail";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestNickname"/> property.
		/// </summary>
		private const string RequestNicknameViewStateKey = "RequestNickname";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestPostalCode"/> property.
		/// </summary>
		private const string RequestPostalCodeViewStateKey = "RequestPostalCode";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestCountry"/> property.
		/// </summary>
		private const string RequestCountryViewStateKey = "RequestCountry";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const string RequireSslViewStateKey = "RequireSsl";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestLanguage"/> property.
		/// </summary>
		private const string RequestLanguageViewStateKey = "RequestLanguage";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestTimeZone"/> property.
		/// </summary>
		private const string RequestTimeZoneViewStateKey = "RequestTimeZone";

		/// <summary>
		/// The viewstate key to use for the <see cref="EnableRequestProfile"/> property.
		/// </summary>
		private const string EnableRequestProfileViewStateKey = "EnableRequestProfile";

		/// <summary>
		/// The viewstate key to use for the <see cref="PolicyUrl"/> property.
		/// </summary>
		private const string PolicyUrlViewStateKey = "PolicyUrl";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestFullName"/> property.
		/// </summary>
		private const string RequestFullNameViewStateKey = "RequestFullName";

		/// <summary>
		/// The viewstate key to use for the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieViewStateKey = "UsePersistentCookie";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestGender"/> property.
		/// </summary>
		private const string RequestGenderViewStateKey = "RequestGender";

		/// <summary>
		/// The viewstate key to use for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlViewStateKey = "ReturnToUrl";

		/// <summary>
		/// The viewstate key to use for the <see cref="Stateless"/> property.
		/// </summary>
		private const string StatelessViewStateKey = "Stateless";

		/// <summary>
		/// The viewstate key to use for the <see cref="ImmediateMode"/> property.
		/// </summary>
		private const string ImmediateModeViewStateKey = "ImmediateMode";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestBirthDate"/> property.
		/// </summary>
		private const string RequestBirthDateViewStateKey = "RequestBirthDate";

		/// <summary>
		/// The viewstate key to use for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlViewStateKey = "RealmUrl";

		#endregion

		#region Property defaults

		/// <summary>
		/// The default value for the <see cref="EnableRequestProfile"/> property.
		/// </summary>
		private const bool EnableRequestProfileDefault = true;

		/// <summary>
		/// The default value for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const bool RequireSslDefault = false;

		/// <summary>
		/// The default value for the <see cref="ImmediateMode"/> property.
		/// </summary>
		private const bool ImmediateModeDefault = false;

		/// <summary>
		/// The default value for the <see cref="Stateless"/> property.
		/// </summary>
		private const bool StatelessDefault = false;

		/// <summary>
		/// The default value for the <see cref="PolicyUrl"/> property.
		/// </summary>
		private const string PolicyUrlDefault = "";

		/// <summary>
		/// The default value for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlDefault = "";

		/// <summary>
		/// The default value for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlDefault = "~/";

		/// <summary>
		/// The default value for the <see cref="RequestEmail"/> property.
		/// </summary>
		private const DemandLevel RequestEmailDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestPostalCode"/> property.
		/// </summary>
		private const DemandLevel RequestPostalCodeDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestCountry"/> property.
		/// </summary>
		private const DemandLevel RequestCountryDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestLanguage"/> property.
		/// </summary>
		private const DemandLevel RequestLanguageDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestTimeZone"/> property.
		/// </summary>
		private const DemandLevel RequestTimeZoneDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestNickname"/> property.
		/// </summary>
		private const DemandLevel RequestNicknameDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestFullName"/> property.
		/// </summary>
		private const DemandLevel RequestFullNameDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestBirthDate"/> property.
		/// </summary>
		private const DemandLevel RequestBirthDateDefault = DemandLevel.NoRequest;

		/// <summary>
		/// The default value for the <see cref="RequestGender"/> property.
		/// </summary>
		private const DemandLevel RequestGenderDefault = DemandLevel.NoRequest;

		#endregion

		/// <summary>
		/// The callback parameter for use with persisting the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieCallbackKey = "OpenIdTextBox_UsePersistentCookie";

		/// <summary>
		/// Backing field for the <see cref="RelyingParty"/> property.
		/// </summary>
		private OpenIdRelyingParty relyingParty;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdMobileTextBox"/> class.
		/// </summary>
		public OpenIdMobileTextBox() {
			Reporting.RecordFeatureUse(this);
		}

		#region Events

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

		/// <summary>
		/// Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.
		/// </summary>
		[Description("Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.")]
		public event EventHandler<OpenIdEventArgs> SetupRequired;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using Realm.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId", Justification = "Using ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(RealmUrlDefault), Category(BehaviorCategory)]
		[Description("The OpenID Realm of the relying party web site.")]
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
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(ReturnToUrlDefault), Category(BehaviorCategory)]
		[Description("The OpenID ReturnTo of the relying party web site.")]
		public string ReturnToUrl {
			get {
				return (string)(ViewState[ReturnToUrlViewStateKey] ?? ReturnToUrlDefault);
			}

			set {
				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Uri based on it.
					new Uri(this.RelyingParty.Channel.GetRequestFromContext().GetPublicFacingUrl(), this.Page.ResolveUrl(value)); // throws an exception on failure.
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

				ViewState[ReturnToUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use immediate mode in the 
		/// OpenID protocol.
		/// </summary>
		/// <value>
		/// True if a Provider should reply immediately to the authentication request
		/// without interacting with the user.  False if the Provider can take time
		/// to authenticate the user in order to complete an authentication attempt.
		/// </value>
		/// <remarks>
		/// Setting this to true is sometimes useful in AJAX scenarios.  Setting this to
		/// true can cause failed authentications when the user truly controls an
		/// Identifier, but must complete an authentication step with the Provider before
		/// the Provider will approve the login from this relying party.
		/// </remarks>
		[Bindable(true), DefaultValue(ImmediateModeDefault), Category(BehaviorCategory)]
		[Description("Whether the Provider should respond immediately to an authentication attempt without interacting with the user.")]
		public bool ImmediateMode {
			get { return (bool)(ViewState[ImmediateModeViewStateKey] ?? ImmediateModeDefault); }
			set { ViewState[ImmediateModeViewStateKey] = value; }
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
		/// Gets or sets your level of interest in receiving the user's nickname from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestNicknameDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's nickname from the Provider.")]
		public DemandLevel RequestNickname {
			get { return (DemandLevel)(ViewState[RequestNicknameViewStateKey] ?? RequestNicknameDefault); }
			set { ViewState[RequestNicknameViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's email address from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestEmailDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's email address from the Provider.")]
		public DemandLevel RequestEmail {
			get { return (DemandLevel)(ViewState[RequestEmailViewStateKey] ?? RequestEmailDefault); }
			set { ViewState[RequestEmailViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's full name from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestFullNameDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's full name from the Provider")]
		public DemandLevel RequestFullName {
			get { return (DemandLevel)(ViewState[RequestFullNameViewStateKey] ?? RequestFullNameDefault); }
			set { ViewState[RequestFullNameViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's birthdate from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestBirthDateDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's birthdate from the Provider.")]
		public DemandLevel RequestBirthDate {
			get { return (DemandLevel)(ViewState[RequestBirthDateViewStateKey] ?? RequestBirthDateDefault); }
			set { ViewState[RequestBirthDateViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's gender from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestGenderDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's gender from the Provider.")]
		public DemandLevel RequestGender {
			get { return (DemandLevel)(ViewState[RequestGenderViewStateKey] ?? RequestGenderDefault); }
			set { ViewState[RequestGenderViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's postal code from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestPostalCodeDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's postal code from the Provider.")]
		public DemandLevel RequestPostalCode {
			get { return (DemandLevel)(ViewState[RequestPostalCodeViewStateKey] ?? RequestPostalCodeDefault); }
			set { ViewState[RequestPostalCodeViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's country from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(ProfileCategory)]
		[DefaultValue(RequestCountryDefault)]
		[Description("Your level of interest in receiving the user's country from the Provider.")]
		public DemandLevel RequestCountry {
			get { return (DemandLevel)(ViewState[RequestCountryViewStateKey] ?? RequestCountryDefault); }
			set { ViewState[RequestCountryViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's preferred language from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestLanguageDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's preferred language from the Provider.")]
		public DemandLevel RequestLanguage {
			get { return (DemandLevel)(ViewState[RequestLanguageViewStateKey] ?? RequestLanguageDefault); }
			set { ViewState[RequestLanguageViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets your level of interest in receiving the user's time zone from the Provider.
		/// </summary>
		[Bindable(true), DefaultValue(RequestTimeZoneDefault), Category(ProfileCategory)]
		[Description("Your level of interest in receiving the user's time zone from the Provider.")]
		public DemandLevel RequestTimeZone {
			get { return (DemandLevel)(ViewState[RequestTimeZoneViewStateKey] ?? RequestTimeZoneDefault); }
			set { ViewState[RequestTimeZoneViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the URL to your privacy policy page that describes how 
		/// claims will be used and/or shared.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(PolicyUrlDefault), Category(ProfileCategory)]
		[Description("The URL to your privacy policy page that describes how claims will be used and/or shared.")]
		public string PolicyUrl {
			get {
				return (string)ViewState[PolicyUrlViewStateKey] ?? PolicyUrlDefault;
			}

			set {
				UriUtil.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[PolicyUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use OpenID extensions
		/// to retrieve profile data of the authenticating user.
		/// </summary>
		[Bindable(true), DefaultValue(EnableRequestProfileDefault), Category(ProfileCategory)]
		[Description("Turns the entire Simple Registration extension on or off.")]
		public bool EnableRequestProfile {
			get { return (bool)(ViewState[EnableRequestProfileViewStateKey] ?? EnableRequestProfileDefault); }
			set { ViewState[EnableRequestProfileViewStateKey] = value; }
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
		/// Gets or sets the type of the custom application store to use, or <c>null</c> to use the default.
		/// </summary>
		/// <remarks>
		/// If set, this property must be set in each Page Load event
		/// as it is not persisted across postbacks.
		/// </remarks>
		public IOpenIdApplicationStore CustomApplicationStore { get; set; }

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
		/// Gets or sets the OpenID authentication request that is about to be sent.
		/// </summary>
		protected IAuthenticationRequest Request { get; set; }

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		public void LogOn() {
			if (this.Request == null) {
				this.CreateRequest(); // sets this.Request
			}

			if (this.Request != null) {
				this.Request.RedirectToProvider();
			}
		}

		/// <summary>
		/// Constructs the authentication request and returns it.
		/// </summary>
		/// <returns>The instantiated authentication request.</returns>
		/// <remarks>
		/// 	<para>This method need not be called before calling the <see cref="LogOn"/> method,
		/// but is offered in the event that adding extensions to the request is desired.</para>
		/// 	<para>The Simple Registration extension arguments are added to the request
		/// before returning if <see cref="EnableRequestProfile"/> is set to true.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		public IAuthenticationRequest CreateRequest() {
			Requires.ValidState(this.Request == null, OpenIdStrings.CreateRequestAlreadyCalled);
			Requires.ValidState(!string.IsNullOrEmpty(this.Text), OpenIdStrings.OpenIdTextBoxEmpty);

			try {
				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = OpenIdUtilities.GetResolvedRealm(this.Page, this.RealmUrl, this.RelyingParty.Channel.GetRequestFromContext());
				realm.Scheme = Page.Request.Url.Scheme;
				realm.Port = Page.Request.Url.Port;

				// Initiate openid request
				// We use TryParse here to avoid throwing an exception which 
				// might slip through our validator control if it is disabled.
				Identifier userSuppliedIdentifier;
				if (Identifier.TryParse(this.Text, out userSuppliedIdentifier)) {
					Realm typedRealm = new Realm(realm);
					if (string.IsNullOrEmpty(this.ReturnToUrl)) {
						this.Request = this.RelyingParty.CreateRequest(userSuppliedIdentifier, typedRealm);
					} else {
						Uri returnTo = new Uri(this.RelyingParty.Channel.GetRequestFromContext().GetPublicFacingUrl(), this.ReturnToUrl);
						this.Request = this.RelyingParty.CreateRequest(userSuppliedIdentifier, typedRealm, returnTo);
					}
					this.Request.Mode = this.ImmediateMode ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
					if (this.EnableRequestProfile) {
						this.AddProfileArgs(this.Request);
					}

					// Add state that needs to survive across the redirect.
					this.Request.SetUntrustedCallbackArgument(UsePersistentCookieCallbackKey, this.UsePersistentCookie.ToString(CultureInfo.InvariantCulture));
				} else {
					Logger.OpenId.WarnFormat("An invalid identifier was entered ({0}), but not caught by any validation routine.", this.Text);
					this.Request = null;
				}
			} catch (ProtocolException ex) {
				this.OnFailed(new FailedAuthenticationResponse(ex));
			}

			return this.Request;
		}

		/// <summary>
		/// Checks for incoming OpenID authentication responses and fires appropriate events.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Page.IsPostBack) {
				return;
			}

			var response = this.RelyingParty.GetResponse();
			if (response != null) {
				string persistentString = response.GetUntrustedCallbackArgument(UsePersistentCookieCallbackKey);
				bool persistentBool;
				if (persistentString != null && bool.TryParse(persistentString, out persistentBool)) {
					this.UsePersistentCookie = persistentBool;
				}

				switch (response.Status) {
					case AuthenticationStatus.Canceled:
						this.OnCanceled(response);
						break;
					case AuthenticationStatus.Authenticated:
						this.OnLoggedIn(response);
						break;
					case AuthenticationStatus.SetupRequired:
						this.OnSetupRequired(response);
						break;
					case AuthenticationStatus.Failed:
						this.OnFailed(response);
						break;
					default:
						throw new InvalidOperationException("Unexpected response status code.");
				}
			}
		}

		#region Events

		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnLoggedIn(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
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
		/// Fires the <see cref="Failed"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnFailed(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.Failed, "Firing Failed event for the wrong response type.");

			var failed = this.Failed;
			if (failed != null) {
				failed(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Fires the <see cref="Canceled"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnCanceled(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.Canceled, "Firing Canceled event for the wrong response type.");

			var canceled = this.Canceled;
			if (canceled != null) {
				canceled(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Fires the <see cref="SetupRequired"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnSetupRequired(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			ErrorUtilities.VerifyInternal(response.Status == AuthenticationStatus.SetupRequired, "Firing SetupRequired event for the wrong response type.");

			// Why are we firing Failed when we're OnSetupRequired?  Backward compatibility.
			var setupRequired = this.SetupRequired;
			if (setupRequired != null) {
				setupRequired(this, new OpenIdEventArgs(response));
			}
		}

		#endregion

		/// <summary>
		/// Adds extensions to a given authentication request to ask the Provider
		/// for user profile data.
		/// </summary>
		/// <param name="request">The authentication request to add the extensions to.</param>
		private void AddProfileArgs(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");

			request.AddExtension(new ClaimsRequest() {
				Nickname = this.RequestNickname,
				Email = this.RequestEmail,
				FullName = this.RequestFullName,
				BirthDate = this.RequestBirthDate,
				Gender = this.RequestGender,
				PostalCode = this.RequestPostalCode,
				Country = this.RequestCountry,
				Language = this.RequestLanguage,
				TimeZone = this.RequestTimeZone,
				PolicyUrl = string.IsNullOrEmpty(this.PolicyUrl) ?
					null : new Uri(this.RelyingParty.Channel.GetRequestFromContext().GetPublicFacingUrl(), this.Page.ResolveUrl(this.PolicyUrl)),
			});
		}

		/// <summary>
		/// Creates the relying party instance used to generate authentication requests.
		/// </summary>
		/// <returns>The instantiated relying party.</returns>
		private OpenIdRelyingParty CreateRelyingParty() {
			// If we're in stateful mode, first use the explicitly given one on this control if there
			// is one.  Then try the configuration file specified one.  Finally, use the default
			// in-memory one that's built into OpenIdRelyingParty.
			IOpenIdApplicationStore store = this.Stateless ? null :
				(this.CustomApplicationStore ?? OpenIdElement.Configuration.RelyingParty.ApplicationStore.CreateInstance(OpenIdRelyingParty.HttpApplicationStore));
			var rp = new OpenIdRelyingParty(store);
			try {
				// Only set RequireSsl to true, as we don't want to override 
				// a .config setting of true with false.
				if (this.RequireSsl) {
					rp.SecuritySettings.RequireSsl = true;
				}
				return rp;
			} catch {
				rp.Dispose();
				throw;
			}
		}
	}
}
