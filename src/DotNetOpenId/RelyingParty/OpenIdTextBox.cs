/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using DotNetOpenId.Extensions.SimpleRegistration;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdTextBox.EmbeddedLogoResourceName, "image/gif")]

namespace DotNetOpenId.RelyingParty
{
	/// <summary>
	/// An ASP.NET control that provides a minimal text box that is OpenID-aware.
	/// </summary>
	/// <remarks>
	/// This control offers greater UI flexibility than the <see cref="OpenIdLogin"/>
	/// control, but requires more work to be done by the hosting web site to 
	/// assemble a complete login experience.
	/// </remarks>
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdTextBox runat=\"server\" />")]
	public class OpenIdTextBox : CompositeControl, IEditableTextControl, ITextControl
	{
		/// <summary>
		/// Instantiates an <see cref="OpenIdTextBox"/> instance.
		/// </summary>
		public OpenIdTextBox()
		{
			InitializeControls();
		}

		internal const string EmbeddedLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.openid_login.gif";
		TextBox wrappedTextBox;
		/// <summary>
		/// Gets the <see cref="TextBox"/> control that this control wraps.
		/// </summary>
		protected TextBox WrappedTextBox
		{
			get { return wrappedTextBox; }
		}

		/// <summary>
		/// Creates the text box control.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Controls.Add(wrappedTextBox);
			if (ShouldBeFocused)
				WrappedTextBox.Focus();
		}

		/// <summary>
		/// Initializes the text box control.
		/// </summary>
		protected virtual void InitializeControls()
		{
			wrappedTextBox = new TextBox();
			wrappedTextBox.ID = "wrappedTextBox";
			wrappedTextBox.CssClass = cssClassDefault;
			wrappedTextBox.Columns = columnsDefault;
			wrappedTextBox.Text = text;
			wrappedTextBox.TabIndex = TabIndexDefault;
		}

		/// <summary>
		/// Whether the text box should receive input focus when the web page appears.
		/// </summary>
		protected bool ShouldBeFocused;
		/// <summary>
		/// Sets the input focus to start on the text box when the page appears
		/// in the user's browser.
		/// </summary>
		public override void Focus()
		{
			if (Controls.Count == 0)
				ShouldBeFocused = true;
			else
				WrappedTextBox.Focus();
		}

		const string appearanceCategory = "Appearance";
		const string profileCategory = "Simple Registration";
		const string behaviorCategory = "Behavior";

		#region Properties
		const string textDefault = "";
		string text = textDefault;
		/// <summary>
		/// The content of the text box.
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue("")]
		[Description("The content of the text box.")]
		public string Text
		{
			get { return (WrappedTextBox != null) ? WrappedTextBox.Text : text; }
			set
			{
				text = value;
				if (WrappedTextBox != null) WrappedTextBox.Text = value;
			}
		}

		const string realmUrlViewStateKey = "RealmUrl";
		const string realmUrlDefault = "~/";
		/// <summary>
		/// The OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.Realm"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(realmUrlDefault)]
		[Description("The OpenID Realm of the relying party web site.")]
		public string RealmUrl
		{
			get { return (string)(ViewState[realmUrlViewStateKey] ?? realmUrlDefault); }
			set
			{
				if (Page != null && !DesignMode)
				{
					// Validate new value by trying to construct a Realm object based on it.
					new Realm(Util.GetResolvedRealm(Page, value)); // throws an exception on failure.
				}
				else
				{
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://"))
					{
						new Uri(value.Replace("*.", "")); // make sure it's fully-qualified, but ignore wildcards
					}
					else if (value.StartsWith("~/", StringComparison.Ordinal))
					{
						// this is valid too
					}
					else
						throw new UriFormatException();
				}
				ViewState[realmUrlViewStateKey] = value; 
			}
		}

		const string returnToUrlViewStateKey = "ReturnToUrl";
		const string returnToUrlDefault = "";
		/// <summary>
		/// The OpenID ReturnTo of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.ReturnTo"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(returnToUrlDefault)]
		[Description("The OpenID ReturnTo of the relying party web site.")]
		public string ReturnToUrl {
			get { return (string)(ViewState[returnToUrlViewStateKey] ?? returnToUrlDefault); }
			set {
				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Uri based on it.
					new Uri(Util.GetRequestUrlFromContext(), Page.ResolveUrl(value)); // throws an exception on failure.
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
				ViewState[returnToUrlViewStateKey] = value;
			}
		}

		const string immediateModeViewStateKey = "ImmediateMode";
		const bool immediateModeDefault = false;
		/// <summary>
		/// True if a Provider should reply immediately to the authentication request
		/// without interacting with the user.  False if the Provider can take time
		/// to authenticate the user in order to complete an authentication attempt.
		/// </summary>
		/// <remarks>
		/// Setting this to true is sometimes useful in AJAX scenarios.  Setting this to
		/// true can cause failed authentications when the user truly controls an
		/// Identifier, but must complete an authentication step with the Provider before
		/// the Provider will approve the login from this relying party.
		/// </remarks>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(immediateModeDefault)]
		[Description("Whether the Provider should respond immediately to an authentication attempt without interacting with the user.")]
		public bool ImmediateMode {
			get { return (bool)(ViewState[immediateModeViewStateKey] ?? immediateModeDefault); }
			set { ViewState[immediateModeViewStateKey] = value; }
		}

		const string statelessViewStateKey = "Stateless";
		const bool statelessDefault = false;
		/// <summary>
		/// Controls whether stateless mode is used.
		/// </summary>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(statelessDefault)]
		[Description("Controls whether stateless mode is used.")]
		public bool Stateless {
			get { return (bool)(ViewState[statelessViewStateKey] ?? statelessDefault); }
			set { ViewState[statelessViewStateKey] = value; }
		}

		const string cssClassDefault = "openid";
		/// <summary>
		/// Gets/sets the CSS class assigned to the text box.
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(cssClassDefault)]
		[Description("The CSS class assigned to the text box.")]
		public override string CssClass
		{
			get { return WrappedTextBox.CssClass; }
			set { WrappedTextBox.CssClass = value; }
		}

		const string showLogoViewStateKey = "ShowLogo";
		const bool showLogoDefault = true;
		/// <summary>
		/// Gets/sets whether to show the OpenID logo in the text box.
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(showLogoDefault)]
		[Description("The visibility of the OpenID logo in the text box.")]
		public bool ShowLogo {
			get { return (bool)(ViewState[showLogoViewStateKey] ?? showLogoDefault); }
			set { ViewState[showLogoViewStateKey] = value; }
		}

		const string presetBorderViewStateKey = "PresetBorder";
		const bool presetBorderDefault = true;
		/// <summary>
		/// Gets/sets whether to use inline styling to force a solid gray border.
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(presetBorderDefault)]
		[Description("Whether to use inline styling to force a solid gray border.")]
		public bool PresetBorder {
			get { return (bool)(ViewState[presetBorderViewStateKey] ?? presetBorderDefault); }
			set { ViewState[presetBorderViewStateKey] = value; }
		}

		const string usePersistentCookieViewStateKey = "UsePersistentCookie";
		/// <summary>
		/// Default value of <see cref="UsePersistentCookie"/>.
		/// </summary>
		protected const bool UsePersistentCookieDefault = false;
		/// <summary>
		/// Whether to send a persistent cookie upon successful 
		/// login so the user does not have to log in upon returning to this site.
		/// </summary>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(UsePersistentCookieDefault)]
		[Description("Whether to send a persistent cookie upon successful " +
			"login so the user does not have to log in upon returning to this site.")]
		public virtual bool UsePersistentCookie
		{
			get { return (bool)(ViewState[usePersistentCookieViewStateKey] ?? UsePersistentCookieDefault); }
			set { ViewState[usePersistentCookieViewStateKey] = value; }
		}

		const int columnsDefault = 40;
		/// <summary>
		/// The width of the text box in characters.
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(columnsDefault)]
		[Description("The width of the text box in characters.")]
		public int Columns
		{
			get { return WrappedTextBox.Columns; }
			set { WrappedTextBox.Columns = value; }
		}

		const int maxLengthDefault = 40;
		/// <summary>
		/// Gets or sets the maximum number of characters the browser should allow
		/// </summary>
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(maxLengthDefault)]
		[Description("The maximum number of characters the browser should allow.")]
		public int MaxLength {
			get { return WrappedTextBox.MaxLength; }
			set { WrappedTextBox.MaxLength = value; }
		}

		/// <summary>
		/// Default value for <see cref="TabIndex"/> property.
		/// </summary>
		protected const short TabIndexDefault = 0;
		/// <summary>
		/// The tab index of the text box control.
		/// </summary>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(TabIndexDefault)]
		[Description("The tab index of the text box control.")]
		public override short TabIndex {
			get { return WrappedTextBox.TabIndex; }
			set { WrappedTextBox.TabIndex = value; }
		}

		const string requestNicknameViewStateKey = "RequestNickname";
		const DemandLevel requestNicknameDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's nickname from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestNicknameDefault)]
		[Description("Your level of interest in receiving the user's nickname from the Provider.")]
		public DemandLevel RequestNickname
		{
			get { return (DemandLevel)(ViewState[requestNicknameViewStateKey] ?? requestNicknameDefault); }
			set { ViewState[requestNicknameViewStateKey] = value; }
		}

		const string requestEmailViewStateKey = "RequestEmail";
		const DemandLevel requestEmailDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's email address from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestEmailDefault)]
		[Description("Your level of interest in receiving the user's email address from the Provider.")]
		public DemandLevel RequestEmail
		{
			get { return (DemandLevel)(ViewState[requestEmailViewStateKey] ?? requestEmailDefault); }
			set { ViewState[requestEmailViewStateKey] = value; }
		}

		const string requestFullNameViewStateKey = "RequestFullName";
		const DemandLevel requestFullNameDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's full name from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestFullNameDefault)]
		[Description("Your level of interest in receiving the user's full name from the Provider")]
		public DemandLevel RequestFullName
		{
			get { return (DemandLevel)(ViewState[requestFullNameViewStateKey] ?? requestFullNameDefault); }
			set { ViewState[requestFullNameViewStateKey] = value; }
		}

		const string requestBirthDateViewStateKey = "RequestBirthday";
		const DemandLevel requestBirthDateDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's birthdate from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestBirthDateDefault)]
		[Description("Your level of interest in receiving the user's birthdate from the Provider.")]
		public DemandLevel RequestBirthDate
		{
			get { return (DemandLevel)(ViewState[requestBirthDateViewStateKey] ?? requestBirthDateDefault); }
			set { ViewState[requestBirthDateViewStateKey] = value; }
		}

		const string requestGenderViewStateKey = "RequestGender";
		const DemandLevel requestGenderDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's gender from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestGenderDefault)]
		[Description("Your level of interest in receiving the user's gender from the Provider.")]
		public DemandLevel RequestGender
		{
			get { return (DemandLevel)(ViewState[requestGenderViewStateKey] ?? requestGenderDefault); }
			set { ViewState[requestGenderViewStateKey] = value; }
		}

		const string requestPostalCodeViewStateKey = "RequestPostalCode";
		const DemandLevel requestPostalCodeDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's postal code from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestPostalCodeDefault)]
		[Description("Your level of interest in receiving the user's postal code from the Provider.")]
		public DemandLevel RequestPostalCode
		{
			get { return (DemandLevel)(ViewState[requestPostalCodeViewStateKey] ?? requestPostalCodeDefault); }
			set { ViewState[requestPostalCodeViewStateKey] = value; }
		}

		const string requestCountryViewStateKey = "RequestCountry";
		const DemandLevel requestCountryDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's country from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestCountryDefault)]
		[Description("Your level of interest in receiving the user's country from the Provider.")]
		public DemandLevel RequestCountry
		{
			get { return (DemandLevel)(ViewState[requestCountryViewStateKey] ?? requestCountryDefault); }
			set { ViewState[requestCountryViewStateKey] = value; }
		}

		const string requestLanguageViewStateKey = "RequestLanguage";
		const DemandLevel requestLanguageDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's preferred language from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestLanguageDefault)]
		[Description("Your level of interest in receiving the user's preferred language from the Provider.")]
		public DemandLevel RequestLanguage
		{
			get { return (DemandLevel)(ViewState[requestLanguageViewStateKey] ?? requestLanguageDefault); }
			set { ViewState[requestLanguageViewStateKey] = value; }
		}

		const string requestTimeZoneViewStateKey = "RequestTimeZone";
		const DemandLevel requestTimeZoneDefault = DemandLevel.NoRequest;
		/// <summary>
		/// Gets/sets your level of interest in receiving the user's time zone from the Provider.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestTimeZoneDefault)]
		[Description("Your level of interest in receiving the user's time zone from the Provider.")]
		public DemandLevel RequestTimeZone
		{
			get { return (DemandLevel)(ViewState[requestTimeZoneViewStateKey] ?? requestTimeZoneDefault); }
			set { ViewState[requestTimeZoneViewStateKey] = value; }
		}

		const string policyUrlViewStateKey = "PolicyUrl";
		const string policyUrlDefault = "";
		/// <summary>
		/// Gets/sets the URL to your privacy policy page that describes how 
		/// claims will be used and/or shared.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(policyUrlDefault)]
		[Description("The URL to your privacy policy page that describes how claims will be used and/or shared.")]
		public string PolicyUrl
		{
			get { return (string)ViewState[policyUrlViewStateKey] ?? policyUrlDefault; }
			set {
				ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[policyUrlViewStateKey] = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri")]
		internal static void ValidateResolvableUrl(Page page, bool designMode, string value) {
			if (string.IsNullOrEmpty(value)) return;
			if (page != null && !designMode) {
				// Validate new value by trying to construct a Realm object based on it.
				new Uri(page.Request.Url, page.ResolveUrl(value)); // throws an exception on failure.
			} else {
				// We can't fully test it, but it should start with either ~/ or a protocol.
				if (Regex.IsMatch(value, @"^https?://")) {
					new Uri(value); // make sure it's fully-qualified, but ignore wildcards
				} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
					// this is valid too
				} else
					throw new UriFormatException();
			}
		}

		const string enableRequestProfileViewStateKey = "EnableRequestProfile";
		const bool enableRequestProfileDefault = true;
		/// <summary>
		/// Turns the entire Simple Registration extension on or off.
		/// </summary>
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(enableRequestProfileDefault)]
		[Description("Turns the entire Simple Registration extension on or off.")]
		public bool EnableRequestProfile
		{
			get { return (bool)(ViewState[enableRequestProfileViewStateKey] ?? enableRequestProfileDefault); }
			set { ViewState[enableRequestProfileViewStateKey] = value; }
		}

		const string requireSslViewStateKey = "RequireSsl";
		const bool requireSslDefault = false;
		/// <summary>
		/// Turns on high security mode, requiring the full authentication pipeline to be protected by SSL.
		/// </summary>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(requireSslDefault)]
		[Description("Turns on high security mode, requiring the full authentication pipeline to be protected by SSL.")]
		public bool RequireSsl {
			get { return (bool)(ViewState[requireSslViewStateKey] ?? requireSslDefault); }
			set { ViewState[requireSslViewStateKey] = value; }
		}

		/// <summary>
		/// A custom application store to use, or null to use the default.
		/// </summary>
		/// <remarks>
		/// If set, this property must be set in each Page Load event
		/// as it is not persisted across postbacks.
		/// </remarks>
		public IRelyingPartyApplicationStore CustomApplicationStore { get; set; }

		#endregion

		#region Properties to hide
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle
		{
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override FontInfo Font
		{
			get { return null; }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Height
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Width
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string ToolTip
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string SkinID
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming
		{
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		/// <summary>
		/// Checks for incoming OpenID authentication responses and fires appropriate events.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!Enabled || Page.IsPostBack) return;
			var consumer = createRelyingParty();
			if (consumer.Response != null) {
				switch (consumer.Response.Status) {
					case AuthenticationStatus.Canceled:
						OnCanceled(consumer.Response);
						break;
					case AuthenticationStatus.Authenticated:
						OnLoggedIn(consumer.Response);
						break;
					case AuthenticationStatus.SetupRequired:
						OnSetupRequired(consumer.Response);
						break;
					case AuthenticationStatus.Failed:
						OnFailed(consumer.Response);
						break;
					default:
						throw new InvalidOperationException("Unexpected response status code.");
				}
			}
		}

		private OpenIdRelyingParty createRelyingParty() {
			// If we're in stateful mode, first use the explicitly given one on this control if there
			// is one.  Then try the configuration file specified one.  Finally, use the default
			// in-memory one that's built into OpenIdRelyingParty.
			IRelyingPartyApplicationStore store = Stateless ? null :
				(CustomApplicationStore ?? OpenIdRelyingParty.Configuration.Store.CreateInstanceOfStore(OpenIdRelyingParty.HttpApplicationStore));
			Uri request = OpenIdRelyingParty.DefaultRequestUrl;
			NameValueCollection query = OpenIdRelyingParty.DefaultQuery;
			var rp = new OpenIdRelyingParty(store, request, query);
			// Only set RequireSsl to true, as we don't want to override 
			// a .config setting of true with false.
			if (RequireSsl) {
				rp.Settings.RequireSsl = true;
			}
			return rp;
		}

		/// <summary>
		/// Prepares the text box to be rendered.
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (ShowLogo) {
				string logoUrl = Page.ClientScript.GetWebResourceUrl(
					typeof(OpenIdTextBox), EmbeddedLogoResourceName);
				WrappedTextBox.Style[HtmlTextWriterStyle.BackgroundImage] = string.Format(
					CultureInfo.InvariantCulture, "url({0})", HttpUtility.HtmlEncode(logoUrl));
				WrappedTextBox.Style["background-repeat"] = "no-repeat";
				WrappedTextBox.Style["background-position"] = "0 50%";
				WrappedTextBox.Style[HtmlTextWriterStyle.PaddingLeft] = "18px";
			}

			if (PresetBorder) {
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderStyle] = "solid";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderWidth] = "1px";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderColor] = "lightgray";
			}
		}

		/// <summary>
		/// The OpenID authentication request that is about to be sent.
		/// </summary>
		protected IAuthenticationRequest Request;
		/// <summary>
		/// Constructs the authentication request and returns it.
		/// </summary>
		/// <remarks>
		/// <para>This method need not be called before calling the <see cref="LogOn"/> method,
		/// but is offered in the event that adding extensions to the request is desired.</para>
		/// <para>The Simple Registration extension arguments are added to the request 
		/// before returning if <see cref="EnableRequestProfile"/> is set to true.</para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public IAuthenticationRequest CreateRequest() {
			if (Request != null)
				throw new InvalidOperationException(Strings.CreateRequestAlreadyCalled);
			if (string.IsNullOrEmpty(Text))
				throw new InvalidOperationException(DotNetOpenId.Strings.OpenIdTextBoxEmpty);

			try {
				var consumer = createRelyingParty();

				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = Util.GetResolvedRealm(Page, RealmUrl);
				realm.Scheme = Page.Request.Url.Scheme;
				realm.Port = Page.Request.Url.Port;

				// Initiate openid request
				// We use TryParse here to avoid throwing an exception which 
				// might slip through our validator control if it is disabled.
				Identifier userSuppliedIdentifier;
				if (Identifier.TryParse(Text, out userSuppliedIdentifier)) {
					Realm typedRealm = new Realm(realm);
					if (string.IsNullOrEmpty(ReturnToUrl)) {
						Request = consumer.CreateRequest(userSuppliedIdentifier, typedRealm);
					} else {
						Uri returnTo = new Uri(Util.GetRequestUrlFromContext(), ReturnToUrl);
						Request = consumer.CreateRequest(userSuppliedIdentifier, typedRealm, returnTo);
					}
					Request.Mode = ImmediateMode ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
					if (EnableRequestProfile) addProfileArgs(Request);
				} else {
					Logger.WarnFormat("An invalid identifier was entered ({0}), but not caught by any validation routine.", Text);
					Request = null;
				}
			} catch (WebException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			} catch (OpenIdException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			}

			return Request;
		}

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public void LogOn()
		{
			if (Request == null)
				CreateRequest();
			if (Request != null)
				Request.RedirectToProvider();
		}

		void addProfileArgs(IAuthenticationRequest request)
		{
			request.AddExtension(new ClaimsRequest() {
				Nickname = RequestNickname,
				Email = RequestEmail,
				FullName = RequestFullName,
				BirthDate = RequestBirthDate,
				Gender = RequestGender,
				PostalCode = RequestPostalCode,
				Country = RequestCountry,
				Language = RequestLanguage,
				TimeZone = RequestTimeZone,
				PolicyUrl = string.IsNullOrEmpty(PolicyUrl) ?
					null : new Uri(Util.GetRequestUrlFromContext(), Page.ResolveUrl(PolicyUrl)),
			});
		}

		#region Events
		/// <summary>
		/// Fired upon completion of a successful login.
		/// </summary>
		[Description("Fired upon completion of a successful login.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		protected virtual void OnLoggedIn(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Authenticated);
			var loggedIn = LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(response);
			if (loggedIn != null)
				loggedIn(this, args);
			if (!args.Cancel)
				FormsAuthentication.RedirectFromLoginPage(
					response.ClaimedIdentifier.ToString(), UsePersistentCookie);
		}

		/// <summary>
		/// Fired when a login attempt fails.
		/// </summary>
		[Description("Fired when a login attempt fails.")]
		public event EventHandler<OpenIdEventArgs> Failed;
		/// <summary>
		/// Fires the <see cref="Failed"/> event.
		/// </summary>
		protected virtual void OnFailed(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Failed);

			var failed = Failed;
			if (failed != null)
				failed(this, new OpenIdEventArgs(response));
		}

		/// <summary>
		/// Fired when an authentication attempt is canceled at the OpenID Provider.
		/// </summary>
		[Description("Fired when an authentication attempt is canceled at the OpenID Provider.")]
		public event EventHandler<OpenIdEventArgs> Canceled;
		/// <summary>
		/// Fires the <see cref="Canceled"/> event.
		/// </summary>
		protected virtual void OnCanceled(IAuthenticationResponse response)
		{
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.Canceled);

			var canceled = Canceled;
			if (canceled != null)
				canceled(this, new OpenIdEventArgs(response));
		}

		/// <summary>
		/// Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.
		/// </summary>
		[Description("Fired when an Immediate authentication attempt fails, and the Provider suggests using non-Immediate mode.")]
		public event EventHandler<OpenIdEventArgs> SetupRequired;
		/// <summary>
		/// Fires the <see cref="SetupRequired"/> event.
		/// </summary>
		protected virtual void OnSetupRequired(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			Debug.Assert(response.Status == AuthenticationStatus.SetupRequired);
			// Why are we firing Failed when we're OnSetupRequired?  Backward compatibility.
			var setupRequired = SetupRequired;
			if (setupRequired != null)
				setupRequired(this, new OpenIdEventArgs(response));
		}

		#endregion

		#region IEditableTextControl Members

		/// <summary>
		/// Occurs when the content of the text box changes between posts to the server.
		/// </summary>
		public event EventHandler TextChanged {
			add { WrappedTextBox.TextChanged += value; }
			remove { WrappedTextBox.TextChanged -= value; }
		}

		#endregion
	}

	/// <summary>
	/// The event details passed to event handlers.
	/// </summary>
	public class OpenIdEventArgs : EventArgs {
		/// <summary>
		/// Constructs an object with minimal information of an incomplete or failed
		/// authentication attempt.
		/// </summary>
		internal OpenIdEventArgs(IAuthenticationRequest request) {
			if (request == null) throw new ArgumentNullException("request");
			Request = request;
			ClaimedIdentifier = request.ClaimedIdentifier;
			IsDirectedIdentity = request.IsDirectedIdentity;
		}
		/// <summary>
		/// Constructs an object with information on a completed authentication attempt
		/// (whether that attempt was successful or not).
		/// </summary>
		internal OpenIdEventArgs(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			Response = response;
			ClaimedIdentifier = response.ClaimedIdentifier;
		}
		/// <summary>
		/// Cancels the OpenID authentication and/or login process.
		/// </summary>
		public bool Cancel { get; set; }
		/// <summary>
		/// The Identifier the user is claiming to own.  Or null if the user
		/// is using Directed Identity.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }
		/// <summary>
		/// Whether the user has selected to let his Provider determine 
		/// the ClaimedIdentifier to use as part of successful authentication.
		/// </summary>
		public bool IsDirectedIdentity { get; private set; }

		/// <summary>
		/// Gets the details of the OpenID authentication request,
		/// and allows for adding extensions.
		/// </summary>
		public IAuthenticationRequest Request { get; private set; }
		/// <summary>
		/// Gets the details of the OpenID authentication response.
		/// </summary>
		public IAuthenticationResponse Response { get; private set; }
	}
}
