//-----------------------------------------------------------------------
// <copyright file="OpenIdTextBox.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdTextBox.EmbeddedLogoResourceName, "image/png")]

#pragma warning disable 0809 // marking inherited, unsupported properties as obsolete to discourage their use

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Globalization;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using Validation;

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
	public class OpenIdTextBox : OpenIdRelyingPartyControlBase, IEditableTextControl, ITextControl, IPostBackDataHandler {
		/// <summary>
		/// The name of the manifest stream containing the
		/// OpenID logo that is placed inside the text box.
		/// </summary>
		internal const string EmbeddedLogoResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.openid_login.png";

		/// <summary>
		/// Default value for <see cref="TabIndex"/> property.
		/// </summary>
		protected const short TabIndexDefault = 0;

		#region Property category constants

		/// <summary>
		/// The "Simple Registration" category for properties.
		/// </summary>
		private const string ProfileCategory = "Simple Registration";

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
		/// The viewstate key to use for the <see cref="PresetBorder"/> property.
		/// </summary>
		private const string PresetBorderViewStateKey = "PresetBorder";

		/// <summary>
		/// The viewstate key to use for the <see cref="ShowLogo"/> property.
		/// </summary>
		private const string ShowLogoViewStateKey = "ShowLogo";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestGender"/> property.
		/// </summary>
		private const string RequestGenderViewStateKey = "RequestGender";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequestBirthDate"/> property.
		/// </summary>
		private const string RequestBirthDateViewStateKey = "RequestBirthDate";

		/// <summary>
		/// The viewstate key to use for the <see cref="CssClass"/> property.
		/// </summary>
		private const string CssClassViewStateKey = "CssClass";

		/// <summary>
		/// The viewstate key to use for the <see cref="MaxLength"/> property.
		/// </summary>
		private const string MaxLengthViewStateKey = "MaxLength";

		/// <summary>
		/// The viewstate key to use for the <see cref="Columns"/> property.
		/// </summary>
		private const string ColumnsViewStateKey = "Columns";

		/// <summary>
		/// The viewstate key to use for the <see cref="TabIndex"/> property.
		/// </summary>
		private const string TabIndexViewStateKey = "TabIndex";

		/// <summary>
		/// The viewstate key to use for the <see cref="Enabled"/> property.
		/// </summary>
		private const string EnabledViewStateKey = "Enabled";

		/// <summary>
		/// The viewstate key to use for the <see cref="Name"/> property.
		/// </summary>
		private const string NameViewStateKey = "Name";

		/// <summary>
		/// The viewstate key to use for the <see cref="Text"/> property.
		/// </summary>
		private const string TextViewStateKey = "Text";

		#endregion

		#region Property defaults

		/// <summary>
		/// The default value for the <see cref="Columns"/> property.
		/// </summary>
		private const int ColumnsDefault = 40;

		/// <summary>
		/// The default value for the <see cref="MaxLength"/> property.
		/// </summary>
		private const int MaxLengthDefault = 40;

		/// <summary>
		/// The default value for the <see cref="Name"/> property.
		/// </summary>
		private const string NameDefault = "openid_identifier";

		/// <summary>
		/// The default value for the <see cref="EnableRequestProfile"/> property.
		/// </summary>
		private const bool EnableRequestProfileDefault = true;

		/// <summary>
		/// The default value for the <see cref="ShowLogo"/> property.
		/// </summary>
		private const bool ShowLogoDefault = true;

		/// <summary>
		/// The default value for the <see cref="PresetBorder"/> property.
		/// </summary>
		private const bool PresetBorderDefault = true;

		/// <summary>
		/// The default value for the <see cref="PolicyUrl"/> property.
		/// </summary>
		private const string PolicyUrlDefault = "";

		/// <summary>
		/// The default value for the <see cref="CssClass"/> property.
		/// </summary>
		private const string CssClassDefault = "openid";

		/// <summary>
		/// The default value for the <see cref="Text"/> property.
		/// </summary>
		private const string TextDefault = "";

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
		/// An empty sreg request, used to compare with others to see if they too are empty.
		/// </summary>
		private static readonly ClaimsRequest EmptyClaimsRequest = new ClaimsRequest();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdTextBox"/> class.
		/// </summary>
		public OpenIdTextBox() {
		}

		#region IEditableTextControl Members

		/// <summary>
		/// Occurs when the content of the text changes between posts to the server.
		/// </summary>
		public event EventHandler TextChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the content of the text box.
		/// </summary>
		[Bindable(true), DefaultValue(""), Category(AppearanceCategory)]
		[Description("The content of the text box.")]
		public string Text {
			get {
				return this.Identifier != null ? this.Identifier.OriginalString : (this.ViewState[TextViewStateKey] as string ?? string.Empty);
			}

			set {
				// Try to store it as a validated identifier,
				// but failing that at least store the text.
				Identifier id;
				if (Identifier.TryParse(value, out id)) {
					this.Identifier = id;
				} else {
					// Be sure to set the viewstate AFTER setting the Identifier,
					// since setting the Identifier clears the viewstate in OnIdentifierChanged.
					this.Identifier = null;
					this.ViewState[TextViewStateKey] = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the form name to use for this input field.
		/// </summary>
		[Bindable(true), DefaultValue(NameDefault), Category(BehaviorCategory)]
		[Description("The form name of this input field.")]
		public string Name {
			get { return (string)(this.ViewState[NameViewStateKey] ?? NameDefault); }
			set { this.ViewState[NameViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the CSS class assigned to the text box.
		/// </summary>
		[Bindable(true), DefaultValue(CssClassDefault), Category(AppearanceCategory)]
		[Description("The CSS class assigned to the text box.")]
		public string CssClass {
			get { return (string)this.ViewState[CssClassViewStateKey]; }
			set { this.ViewState[CssClassViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to show the OpenID logo in the text box.
		/// </summary>
		[Bindable(true), DefaultValue(ShowLogoDefault), Category(AppearanceCategory)]
		[Description("The visibility of the OpenID logo in the text box.")]
		public bool ShowLogo {
			get { return (bool)(this.ViewState[ShowLogoViewStateKey] ?? ShowLogoDefault); }
			set { this.ViewState[ShowLogoViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use inline styling to force a solid gray border.
		/// </summary>
		[Bindable(true), DefaultValue(PresetBorderDefault), Category(AppearanceCategory)]
		[Description("Whether to use inline styling to force a solid gray border.")]
		public bool PresetBorder {
			get { return (bool)(this.ViewState[PresetBorderViewStateKey] ?? PresetBorderDefault); }
			set { this.ViewState[PresetBorderViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the width of the text box in characters.
		/// </summary>
		[Bindable(true), DefaultValue(ColumnsDefault), Category(AppearanceCategory)]
		[Description("The width of the text box in characters.")]
		public int Columns {
			get { return (int)(this.ViewState[ColumnsViewStateKey] ?? ColumnsDefault); }
			set { this.ViewState[ColumnsViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum number of characters the browser should allow
		/// </summary>
		[Bindable(true), DefaultValue(MaxLengthDefault), Category(AppearanceCategory)]
		[Description("The maximum number of characters the browser should allow.")]
		public int MaxLength {
			get { return (int)(this.ViewState[MaxLengthViewStateKey] ?? MaxLengthDefault); }
			set { this.ViewState[MaxLengthViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the tab index of the Web server control.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The tab index of the Web server control. The default is 0, which indicates that this property is not set.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// The specified tab index is not between -32768 and 32767.
		/// </exception>
		[Bindable(true), DefaultValue(TabIndexDefault), Category(BehaviorCategory)]
		[Description("The tab index of the text box control.")]
		public virtual short TabIndex {
			get { return (short)(this.ViewState[TabIndexViewStateKey] ?? TabIndexDefault); }
			set { this.ViewState[TabIndexViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="OpenIdTextBox"/> is enabled
		/// in the browser for editing and will respond to incoming OpenID messages.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		[Bindable(true), DefaultValue(true), Category(BehaviorCategory)]
		[Description("Whether the control is editable in the browser and will respond to OpenID messages.")]
		public bool Enabled {
			get { return (bool)(this.ViewState[EnabledViewStateKey] ?? true); }
			set { this.ViewState[EnabledViewStateKey] = value; }
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
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
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

		#endregion

		#region IPostBackDataHandler Members

		/// <summary>
		/// When implemented by a class, processes postback data for an ASP.NET server control.
		/// </summary>
		/// <param name="postDataKey">The key identifier for the control.</param>
		/// <param name="postCollection">The collection of all incoming name values.</param>
		/// <returns>
		/// true if the server control's state changes as a result of the postback; otherwise, false.
		/// </returns>
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
			return this.LoadPostData(postDataKey, postCollection);
		}

		/// <summary>
		/// When implemented by a class, signals the server control to notify the ASP.NET application that the state of the control has changed.
		/// </summary>
		void IPostBackDataHandler.RaisePostDataChangedEvent() {
			this.RaisePostDataChangedEvent();
		}

		#endregion

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="identifier">The identifier to create a request for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier" />.
		/// </returns>
		protected internal override async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier identifier, CancellationToken cancellationToken) {
			ErrorUtilities.VerifyArgumentNotNull(identifier, "identifier");

			// We delegate all our logic to another method, since invoking base. methods
			// within an iterator method results in unverifiable code.
			return this.CreateRequestsCore(await base.CreateRequestsAsync(identifier, cancellationToken));
		}

		/// <summary>
		/// Checks for incoming OpenID authentication responses and fires appropriate events.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			if (!this.Enabled) {
				return;
			}

			this.Page.RegisterRequiresPostBack(this);
			base.OnLoad(e);
		}

		/// <summary>
		/// Called when the <see cref="Identifier"/> property is changed.
		/// </summary>
		protected override void OnIdentifierChanged() {
			this.ViewState.Remove(TextViewStateKey);
			base.OnIdentifierChanged();
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(HtmlTextWriter writer) {
			Assumes.True(writer != null, "Missing contract.");

			if (this.ShowLogo) {
				string logoUrl = Page.ClientScript.GetWebResourceUrl(
					typeof(OpenIdTextBox), EmbeddedLogoResourceName);
				writer.AddStyleAttribute(
					HtmlTextWriterStyle.BackgroundImage,
					string.Format(CultureInfo.InvariantCulture, "url({0})", HttpUtility.HtmlEncode(logoUrl)));
				writer.AddStyleAttribute("background-repeat", "no-repeat");
				writer.AddStyleAttribute("background-position", "0 50%");
				writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "18px");
			}

			if (this.PresetBorder) {
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "1px");
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "lightgray");
			}

			if (!string.IsNullOrEmpty(this.CssClass)) {
				writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, HttpUtility.HtmlEncode(this.Name));
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			writer.AddAttribute(HtmlTextWriterAttribute.Size, this.Columns.ToString(CultureInfo.InvariantCulture));
			writer.AddAttribute(HtmlTextWriterAttribute.Value, HttpUtility.HtmlEncode(this.Text));
			writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, this.TabIndex.ToString(CultureInfo.CurrentCulture));

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}

		/// <summary>
		/// When implemented by a class, processes postback data for an ASP.NET server control.
		/// </summary>
		/// <param name="postDataKey">The key identifier for the control.</param>
		/// <param name="postCollection">The collection of all incoming name values.</param>
		/// <returns>
		/// true if the server control's state changes as a result of the postback; otherwise, false.
		/// </returns>
		protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
			Assumes.True(postCollection != null, "Missing contract");

			// If the control was temporarily hidden, it won't be in the Form data,
			// and we'll just implicitly keep the last Text setting.
			if (postCollection[this.Name] != null) {
				this.Text = postCollection[this.Name];
				return true;
			}

			return false;
		}

		/// <summary>
		/// When implemented by a class, signals the server control to notify the ASP.NET application that the state of the control has changed.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Preserve signature of interface we're implementing.")]
		protected virtual void RaisePostDataChangedEvent() {
			this.OnTextChanged();
		}

		/// <summary>
		/// Called on a postback when the Text property has changed.
		/// </summary>
		protected virtual void OnTextChanged() {
			EventHandler textChanged = this.TextChanged;
			if (textChanged != null) {
				textChanged(this, EventArgs.Empty);
			}
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
			Requires.NotNull(requests, "requests");

			foreach (var request in requests) {
				if (this.EnableRequestProfile) {
					this.AddProfileArgs(request);
				}

				yield return request;
			}
		}

		/// <summary>
		/// Adds extensions to a given authentication request to ask the Provider
		/// for user profile data.
		/// </summary>
		/// <param name="request">The authentication request to add the extensions to.</param>
		private void AddProfileArgs(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");

			var sreg = new ClaimsRequest() {
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
					null : new Uri(new HttpRequestWrapper(this.Context.Request).GetPublicFacingUrl(), this.Page.ResolveUrl(this.PolicyUrl)),
			};

			// Only actually add the extension request if fields are actually being requested.
			if (!sreg.Equals(EmptyClaimsRequest)) {
				request.AddExtension(sreg);
			}
		}
	}
}
