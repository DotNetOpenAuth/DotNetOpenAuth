//-----------------------------------------------------------------------
// <copyright file="InfoCardSelector.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
//     Certain elements are Copyright (c) 2007 Dominick Baier.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.InfoCard.InfoCardSelector.ScriptResourceName, "text/javascript")]

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Drawing.Design;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using System.Xml;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The style to use for NOT displaying a hidden region.
	/// </summary>
	public enum RenderMode {
		/// <summary>
		/// A hidden region should be invisible while still occupying space in the page layout.
		/// </summary>
		Static,

		/// <summary>
		/// A hidden region should collapse so that it does not occupy space in the page layout.
		/// </summary>
		Dynamic
	}

	/// <summary>
	/// An Information Card selector ASP.NET control.
	/// </summary>
	[ParseChildren(true, "ClaimsRequested")]
	[PersistChildren(false)]
	[DefaultEvent("ReceivedToken")]
	[ToolboxData("<{0}:InfoCardSelector runat=\"server\"><ClaimsRequested><{0}:ClaimType Name=\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier\" /></ClaimsRequested><UnsupportedTemplate><p>Your browser does not support Information Cards.</p></UnsupportedTemplate></{0}:InfoCardSelector>")]
	[ContractVerification(true)]
	public class InfoCardSelector : CompositeControl, IPostBackEventHandler {
		/// <summary>
		/// The resource name for getting at the SupportingScript.js embedded manifest stream.
		/// </summary>
		internal const string ScriptResourceName = "DotNetOpenAuth.InfoCard.SupportingScript.js";

		#region Property constants

		/// <summary>
		/// Default value for the <see cref="RenderMode"/> property.
		/// </summary>
		private const RenderMode RenderModeDefault = RenderMode.Dynamic;

		/// <summary>
		/// Default value for the <see cref="AutoPostBack"/> property.
		/// </summary>
		private const bool AutoPostBackDefault = true;

		/// <summary>
		/// Default value for the <see cref="AutoPopup"/> property.
		/// </summary>
		private const bool AutoPopupDefault = false;

		/// <summary>
		/// Default value for the <see cref="PrivacyUrl"/> property.
		/// </summary>
		private const string PrivacyUrlDefault = "";

		/// <summary>
		/// Default value for the <see cref="PrivacyVersion"/> property.
		/// </summary>
		private const string PrivacyVersionDefault = "";

		/// <summary>
		/// Default value for the <see cref="InfoCardImage"/> property.
		/// </summary>
		private const InfoCardImageSize InfoCardImageDefault = InfoCardImage.DefaultImageSize;

		/// <summary>
		/// Default value for the <see cref="IssuerPolicy"/> property.
		/// </summary>
		private const string IssuerPolicyDefault = "";

		/// <summary>
		/// Default value for the <see cref="Issuer"/> property.
		/// </summary>
		private const string IssuerDefault = WellKnownIssuers.SelfIssued;

		/// <summary>
		/// The default value for the <see cref="TokenType"/> property.
		/// </summary>
		private const string TokenTypeDefault = "urn:oasis:names:tc:SAML:1.0:assertion";

		/// <summary>
		/// The viewstate key for storing the <see cref="Issuer" /> property.
		/// </summary>
		private const string IssuerViewStateKey = "Issuer";

		/// <summary>
		/// The viewstate key for storing the <see cref="IssuerPolicy" /> property.
		/// </summary>
		private const string IssuerPolicyViewStateKey = "IssuerPolicy";

		/// <summary>
		/// The viewstate key for storing the <see cref="AutoPopup" /> property.
		/// </summary>
		private const string AutoPopupViewStateKey = "AutoPopup";

		/// <summary>
		/// The viewstate key for storing the <see cref="ClaimsRequested" /> property.
		/// </summary>
		private const string ClaimsRequestedViewStateKey = "ClaimsRequested";

		/// <summary>
		/// The viewstate key for storing the <see cref="TokenType" /> property.
		/// </summary>
		private const string TokenTypeViewStateKey = "TokenType";

		/// <summary>
		/// The viewstate key for storing the <see cref="PrivacyUrl" /> property.
		/// </summary>
		private const string PrivacyUrlViewStateKey = "PrivacyUrl";

		/// <summary>
		/// The viewstate key for storing the <see cref="PrivacyVersion" /> property.
		/// </summary>
		private const string PrivacyVersionViewStateKey = "PrivacyVersion";

		/// <summary>
		/// The viewstate key for storing the <see cref="Audience" /> property.
		/// </summary>
		private const string AudienceViewStateKey = "Audience";

		/// <summary>
		/// The viewstate key for storing the <see cref="AutoPostBack" /> property.
		/// </summary>
		private const string AutoPostBackViewStateKey = "AutoPostBack";

		/// <summary>
		/// The viewstate key for storing the <see cref="ImageSize" /> property.
		/// </summary>
		private const string ImageSizeViewStateKey = "ImageSize";

		/// <summary>
		/// The viewstate key for storing the <see cref="RenderMode" /> property.
		/// </summary>
		private const string RenderModeViewStateKey = "RenderMode";

		#endregion

		#region Categories

		/// <summary>
		/// The "Behavior" property category.
		/// </summary>
		private const string BehaviorCategory = "Behavior";

		/// <summary>
		/// The "Appearance" property category.
		/// </summary>
		private const string AppearanceCategory = "Appearance";

		/// <summary>
		/// The "InfoCard" property category.
		/// </summary>
		private const string InfoCardCategory = "InfoCard";

		#endregion

		/// <summary>
		/// The panel containing the controls to display if InfoCard is supported in the user agent.
		/// </summary>
		private Panel infoCardSupportedPanel;

		/// <summary>
		/// The panel containing the controls to display if InfoCard is NOT supported in the user agent.
		/// </summary>
		private Panel infoCardNotSupportedPanel;

		/// <summary>
		/// Recalls whether the <see cref="Audience"/> property has been set yet,
		/// so its default can be set as soon as possible without overwriting
		/// an intentional value.
		/// </summary>
		private bool audienceSet;

		/// <summary>
		/// Initializes a new instance of the <see cref="InfoCardSelector"/> class.
		/// </summary>
		public InfoCardSelector() {
			this.ToolTip = InfoCardStrings.SelectorClickPrompt;
			Reporting.RecordFeatureUse(this);
		}

		/// <summary>
		/// Occurs when an InfoCard has been submitted but not decoded yet.
		/// </summary>
		[Category(InfoCardCategory)]
		public event EventHandler<ReceivingTokenEventArgs> ReceivingToken;

		/// <summary>
		/// Occurs when an InfoCard has been submitted and decoded.
		/// </summary>
		[Category(InfoCardCategory)]
		public event EventHandler<ReceivedTokenEventArgs> ReceivedToken;

		/// <summary>
		/// Occurs when an InfoCard token is submitted but an error occurs in processing.
		/// </summary>
		[Category(InfoCardCategory)]
		public event EventHandler<TokenProcessingErrorEventArgs> TokenProcessingError;

		#region Properties

		/// <summary>
		/// Gets the set of claims that are requested from the Information Card.
		/// </summary>
		[Description("Specifies the required and optional claims.")]
		[PersistenceMode(PersistenceMode.InnerProperty), Category(InfoCardCategory)]
		public Collection<ClaimType> ClaimsRequested {
			get {
				Contract.Ensures(Contract.Result<Collection<ClaimType>>() != null);
				if (this.ViewState[ClaimsRequestedViewStateKey] == null) {
					var claims = new Collection<ClaimType>();
					this.ViewState[ClaimsRequestedViewStateKey] = claims;
					return claims;
				} else {
					return (Collection<ClaimType>)this.ViewState[ClaimsRequestedViewStateKey];
				}
			}
		}

		/// <summary>
		/// Gets or sets the issuer URI.
		/// </summary>
		[Description("When receiving managed cards, this is the only Issuer whose cards will be accepted.")]
		[Category(InfoCardCategory), DefaultValue(IssuerDefault)]
		[TypeConverter(typeof(ComponentModel.IssuersSuggestions))]
		public string Issuer {
			get { return (string)this.ViewState[IssuerViewStateKey] ?? IssuerDefault; }
			set { this.ViewState[IssuerViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the issuer policy URI.
		/// </summary>
		[Description("Specifies the URI of the issuer MEX endpoint")]
		[Category(InfoCardCategory), DefaultValue(IssuerPolicyDefault)]
		public string IssuerPolicy {
			get { return (string)this.ViewState[IssuerPolicyViewStateKey] ?? IssuerPolicyDefault; }
			set { this.ViewState[IssuerPolicyViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the URL to this site's privacy policy.
		/// </summary>
		[Description("The URL to this site's privacy policy.")]
		[Category(InfoCardCategory), DefaultValue(PrivacyUrlDefault)]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "We construct a Uri to validate the format of the string.")]
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "That overload is NOT the same.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This can take ~/ paths.")]
		public string PrivacyUrl {
			get {
				return (string)this.ViewState[PrivacyUrlViewStateKey] ?? PrivacyUrlDefault;
			}

			set {
				ErrorUtilities.VerifyOperation(string.IsNullOrEmpty(value) || this.Page == null || this.DesignMode || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
				if (!string.IsNullOrEmpty(value)) {
					if (this.Page != null && !this.DesignMode) {
						// Validate new value by trying to construct a Uri based on it.
						new Uri(new HttpRequestWrapper(HttpContext.Current.Request).GetPublicFacingUrl(), this.Page.ResolveUrl(value)); // throws an exception on failure.
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
				}

				this.ViewState[PrivacyUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the version of the privacy policy file.
		/// </summary>
		[Description("Specifies the version of the privacy policy file")]
		[Category(InfoCardCategory), DefaultValue(PrivacyVersionDefault)]
		public string PrivacyVersion {
			get { return (string)this.ViewState[PrivacyVersionViewStateKey] ?? PrivacyVersionDefault; }
			set { this.ViewState[PrivacyVersionViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the URI that must be found for the SAML token's intended audience
		/// in order for the token to be processed.
		/// </summary>
		/// <value>Typically the URI of the page hosting the control, or <c>null</c> to disable audience verification.</value>
		/// <remarks>
		/// Disabling audience verification introduces a security risk 
		/// because tokens can be redirected to allow access to unintended resources.
		/// </remarks>
		[Description("Specifies the URI that must be found for the SAML token's intended audience.")]
		[Bindable(true), Category(InfoCardCategory)]
		[TypeConverter(typeof(ComponentModel.UriConverter))]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public Uri Audience {
			get {
				return (Uri)this.ViewState[AudienceViewStateKey];
			}

			set {
				this.ViewState[AudienceViewStateKey] = value;
				this.audienceSet = true;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a postback will automatically
		/// be invoked when the user selects an Information Card.
		/// </summary>
		[Description("Specifies if the pages automatically posts back after the user has selected a card")]
		[Category(BehaviorCategory), DefaultValue(AutoPostBackDefault)]
		public bool AutoPostBack {
			get { return (bool)(this.ViewState[AutoPostBackViewStateKey] ?? AutoPostBackDefault); }
			set { this.ViewState[AutoPostBackViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the size of the standard InfoCard image to display.
		/// </summary>
		/// <value>The default size is 114x80.</value>
		[Description("The size of the InfoCard image to use. Defaults to 114x80.")]
		[DefaultValue(InfoCardImageDefault), Category(AppearanceCategory)]
		public InfoCardImageSize ImageSize {
			get { return (InfoCardImageSize)(this.ViewState[ImageSizeViewStateKey] ?? InfoCardImageDefault); }
			set { this.ViewState[ImageSizeViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the template to display when the user agent lacks
		/// an Information Card selector.
		/// </summary>
		[Browsable(false), DefaultValue("")]
		[PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(InfoCardSelector))]
		public virtual ITemplate UnsupportedTemplate { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a hidden region (either
		/// the unsupported or supported InfoCard HTML)
		/// collapses or merely becomes invisible when it is not to be displayed.
		/// </summary>
		[Description("Whether the hidden region collapses or merely becomes invisible.")]
		[Category(AppearanceCategory), DefaultValue(RenderModeDefault)]
		public RenderMode RenderMode {
			get { return (RenderMode)(this.ViewState[RenderModeViewStateKey] ?? RenderModeDefault); }
			set { this.ViewState[RenderModeViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the identity selector will be triggered at page load.
		/// </summary>
		[Description("Controls whether the InfoCard selector automatically appears when the page is loaded.")]
		[Category(BehaviorCategory), DefaultValue(AutoPopupDefault)]
		public bool AutoPopup {
			get { return (bool)(this.ViewState[AutoPopupViewStateKey] ?? AutoPopupDefault); }
			set { this.ViewState[AutoPopupViewStateKey] = value; }
		}

		#endregion

		/// <summary>
		/// Gets the name of the hidden field that is used to transport the token back to the server.
		/// </summary>
		private string HiddenFieldName {
			get { return this.ClientID + "_tokenxml"; }
		}

		/// <summary>
		/// Gets the id of the OBJECT tag that creates the InfoCard Selector.
		/// </summary>
		private string SelectorObjectId {
			get { return this.ClientID + "_cs"; }
		}

		/// <summary>
		/// Gets the XML token, which will be encrypted if it was received over SSL.
		/// </summary>
		private string TokenXml {
			get { return this.Page.Request.Form[this.HiddenFieldName]; }
		}

		/// <summary>
		/// Gets or sets the type of token the page is prepared to receive.
		/// </summary>
		[Description("Specifies the token type. Defaults to SAML 1.0")]
		[DefaultValue(TokenTypeDefault), Category(InfoCardCategory)]
		private string TokenType {
			get { return (string)this.ViewState[TokenTypeViewStateKey] ?? TokenTypeDefault; }
			set { this.ViewState[TokenTypeViewStateKey] = value; }
		}

		/// <summary>
		/// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
		/// </summary>
		/// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
			this.RaisePostBackEvent(eventArgument);
		}

		/// <summary>
		/// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
		/// </summary>
		/// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Predefined signature.")]
		protected virtual void RaisePostBackEvent(string eventArgument) {
			if (!string.IsNullOrEmpty(this.TokenXml)) {
				try {
					ReceivingTokenEventArgs receivingArgs = this.OnReceivingToken(this.TokenXml);

					if (!receivingArgs.Cancel) {
						try {
							Token token = Token.Read(this.TokenXml, this.Audience, receivingArgs.DecryptingTokens);
							this.OnReceivedToken(token);
						} catch (InformationCardException ex) {
							this.OnTokenProcessingError(this.TokenXml, ex);
						}
					}
				} catch (XmlException ex) {
					this.OnTokenProcessingError(this.TokenXml, ex);
				}
			}
		}

		/// <summary>
		/// Fires the <see cref="ReceivingToken"/> event.
		/// </summary>
		/// <param name="tokenXml">The token XML, prior to any processing.</param>
		/// <returns>The event arguments sent to the event handlers.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "decryptor", Justification = "By design")]
		protected virtual ReceivingTokenEventArgs OnReceivingToken(string tokenXml) {
			Requires.NotNull(tokenXml, "tokenXml");

			var args = new ReceivingTokenEventArgs(tokenXml);
			var receivingToken = this.ReceivingToken;
			if (receivingToken != null) {
				receivingToken(this, args);
			}

			return args;
		}

		/// <summary>
		/// Fires the <see cref="ReceivedToken"/> event.
		/// </summary>
		/// <param name="token">The token, if it was decrypted.</param>
		protected virtual void OnReceivedToken(Token token) {
			Requires.NotNull(token, "token");

			var receivedInfoCard = this.ReceivedToken;
			if (receivedInfoCard != null) {
				receivedInfoCard(this, new ReceivedTokenEventArgs(token));
			}
		}

		/// <summary>
		/// Fires the <see cref="TokenProcessingError"/> event.
		/// </summary>
		/// <param name="unprocessedToken">The unprocessed token.</param>
		/// <param name="ex">The exception generated while processing the token.</param>
		protected virtual void OnTokenProcessingError(string unprocessedToken, Exception ex) {
			Requires.NotNull(unprocessedToken, "unprocessedToken");
			Requires.NotNull(ex, "ex");

			var tokenProcessingError = this.TokenProcessingError;
			if (tokenProcessingError != null) {
				TokenProcessingErrorEventArgs args = new TokenProcessingErrorEventArgs(unprocessedToken, ex);
				tokenProcessingError(this, args);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnInit(EventArgs e) {
			// Give a default for the Audience property that allows for 
			// the aspx page to have preset it, and ViewState
			// to initialize it (even to null) after this.
			if (!this.audienceSet && !this.DesignMode) {
				this.Audience = this.Page.Request.Url;
			}

			base.OnInit(e);
			this.Page.LoadComplete += delegate { this.EnsureChildControls(); };
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls() {
			base.CreateChildControls();

			this.Page.ClientScript.RegisterHiddenField(this.HiddenFieldName, string.Empty);

			this.Controls.Add(this.infoCardSupportedPanel = this.CreateInfoCardSupportedPanel());
			this.Controls.Add(this.infoCardNotSupportedPanel = this.CreateInfoCardUnsupportedPanel());

			this.RenderSupportingScript();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (!this.DesignMode) {
				// The Cardspace selector will display an ugly error to the user if
				// the privacy URL is present but the privacy version is not.
				ErrorUtilities.VerifyOperation(string.IsNullOrEmpty(this.PrivacyUrl) || !string.IsNullOrEmpty(this.PrivacyVersion), InfoCardStrings.PrivacyVersionRequiredWithPrivacyUrl);
			}

			this.RegisterInfoCardSelectorObjectScript();
		}

		/// <summary>
		/// Creates a control that renders to &lt;Param Name="{0}" Value="{1}" /&gt;
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <returns>The control that renders to the Param tag.</returns>
		private static string CreateParamJs(string name, string value) {
			Contract.Ensures(Contract.Result<string>() != null);
			string scriptFormat = @"	objp = document.createElement('param');
	objp.name = {0};
	objp.value = {1};
	obj.appendChild(objp);
";
			return string.Format(
				CultureInfo.InvariantCulture,
				scriptFormat,
			MessagingUtilities.GetSafeJavascriptValue(name),
			MessagingUtilities.GetSafeJavascriptValue(value));
		}

		/// <summary>
		/// Creates the panel whose contents are displayed to the user
		/// on a user agent that has an Information Card selector.
		/// </summary>
		/// <returns>The Panel control</returns>
		[Pure]
		private Panel CreateInfoCardSupportedPanel() {
			Contract.Ensures(Contract.Result<Panel>() != null);

			Panel supportedPanel = new Panel();

			try {
				if (!this.DesignMode) {
					// At the user agent, assume InfoCard is not supported until
					// the JavaScript discovers otherwise and reveals this panel.
					supportedPanel.Style[HtmlTextWriterStyle.Display] = "none";
				}

				supportedPanel.Controls.Add(this.CreateInfoCardImage());

				// trigger the selector at page load?
				if (this.AutoPopup && !this.Page.IsPostBack) {
					this.Page.ClientScript.RegisterStartupScript(
						typeof(InfoCardSelector),
						"selector_load_trigger",
						this.GetInfoCardSelectorActivationScript(true),
						true);
				}
				return supportedPanel;
			} catch {
				supportedPanel.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Gets the InfoCard selector activation script.
		/// </summary>
		/// <param name="alwaysPostback">Whether a postback should always immediately follow the selector, even if <see cref="AutoPostBack"/> is <c>false</c>.</param>
		/// <returns>The javascript to inject into the surrounding context.</returns>
		private string GetInfoCardSelectorActivationScript(bool alwaysPostback) {
			// generate call do __doPostback
			PostBackOptions options = new PostBackOptions(this);
			string postback = string.Empty;
			if (alwaysPostback || this.AutoPostBack) {
				postback = this.Page.ClientScript.GetPostBackEventReference(options) + ";";
			}

			// generate the onclick script for the image
			string invokeScript = string.Format(
				CultureInfo.InvariantCulture,
				@"if (document.infoCard.activate('{0}', '{1}')) {{ {2} }}",
				this.SelectorObjectId,
				this.HiddenFieldName,
				postback);

			return invokeScript;
		}

		/// <summary>
		/// Creates the panel whose contents are displayed to the user
		/// on a user agent that does not have an Information Card selector.
		/// </summary>
		/// <returns>The Panel control.</returns>
		[Pure]
		private Panel CreateInfoCardUnsupportedPanel() {
			Contract.Ensures(Contract.Result<Panel>() != null);

			Panel unsupportedPanel = new Panel();
			try {
				if (this.UnsupportedTemplate != null) {
					this.UnsupportedTemplate.InstantiateIn(unsupportedPanel);
				}
				return unsupportedPanel;
			} catch {
				unsupportedPanel.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Adds the javascript that adds the info card selector &lt;object&gt; HTML tag to the page.
		/// </summary>
		[Pure]
		private void RegisterInfoCardSelectorObjectScript() {
			string scriptFormat = @"{{
	var obj = document.createElement('object');
	obj.type = 'application/x-informationcard';
	obj.id = {0};
	obj.style.display = 'none';
";
			StringBuilder script = new StringBuilder();
			script.AppendFormat(
				CultureInfo.InvariantCulture,
				scriptFormat,
				MessagingUtilities.GetSafeJavascriptValue(this.ClientID + "_cs"));

			if (!string.IsNullOrEmpty(this.Issuer)) {
				script.AppendLine(CreateParamJs("issuer", this.Issuer));
			}

			if (!string.IsNullOrEmpty(this.IssuerPolicy)) {
				script.AppendLine(CreateParamJs("issuerPolicy", this.IssuerPolicy));
			}

			if (!string.IsNullOrEmpty(this.TokenType)) {
				script.AppendLine(CreateParamJs("tokenType", this.TokenType));
			}

			string requiredClaims, optionalClaims;
			this.GetRequestedClaims(out requiredClaims, out optionalClaims);
			ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(requiredClaims) || !string.IsNullOrEmpty(optionalClaims), InfoCardStrings.EmptyClaimListNotAllowed);
			if (!string.IsNullOrEmpty(requiredClaims)) {
				script.AppendLine(CreateParamJs("requiredClaims", requiredClaims));
			}
			if (!string.IsNullOrEmpty(optionalClaims)) {
				script.AppendLine(CreateParamJs("optionalClaims", optionalClaims));
			}

			if (!string.IsNullOrEmpty(this.PrivacyUrl)) {
				string privacyUrl = this.DesignMode ? this.PrivacyUrl : new Uri(Page.Request.Url, Page.ResolveUrl(this.PrivacyUrl)).AbsoluteUri;
				script.AppendLine(CreateParamJs("privacyUrl", privacyUrl));
			}

			if (!string.IsNullOrEmpty(this.PrivacyVersion)) {
				script.AppendLine(CreateParamJs("privacyVersion", this.PrivacyVersion));
			}

			script.AppendLine(@"if (document.infoCard.isSupported()) { document.getElementsByTagName('head')[0].appendChild(obj); }
}");

			this.Page.ClientScript.RegisterClientScriptBlock(typeof(InfoCardSelector), this.ClientID + "tag", script.ToString(), true);
		}

		/// <summary>
		/// Creates the info card clickable image.
		/// </summary>
		/// <returns>An Image object.</returns>
		[Pure]
		private Image CreateInfoCardImage() {
			// add clickable image
			Image image = new Image();
			try {
				image.ImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(InfoCardSelector), InfoCardImage.GetImageManifestResourceStreamName(this.ImageSize));
				image.AlternateText = InfoCardStrings.SelectorClickPrompt;
				image.ToolTip = this.ToolTip;
				image.Style[HtmlTextWriterStyle.Cursor] = "hand";

				image.Attributes["onclick"] = this.GetInfoCardSelectorActivationScript(false);
				return image;
			} catch {
				image.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Compiles lists of requested/required claims that should accompany
		/// any submitted Information Card.
		/// </summary>
		/// <param name="required">A space-delimited list of claim type URIs for claims that must be included in a submitted Information Card.</param>
		/// <param name="optional">A space-delimited list of claim type URIs for claims that may optionally be included in a submitted Information Card.</param>
		[Pure]
		private void GetRequestedClaims(out string required, out string optional) {
			Requires.ValidState(this.ClaimsRequested != null);
			Contract.Ensures(Contract.ValueAtReturn<string>(out required) != null);
			Contract.Ensures(Contract.ValueAtReturn<string>(out optional) != null);

			var nonEmptyClaimTypes = this.ClaimsRequested.Where(c => c.Name != null);

			var optionalClaims = from claim in nonEmptyClaimTypes
								 where claim.IsOptional
								 select claim.Name;
			var requiredClaims = from claim in nonEmptyClaimTypes
								 where !claim.IsOptional
								 select claim.Name;

			string[] requiredClaimsArray = requiredClaims.ToArray();
			string[] optionalClaimsArray = optionalClaims.ToArray();
			required = string.Join(" ", requiredClaimsArray);
			optional = string.Join(" ", optionalClaimsArray);
			Contract.Assume(required != null);
			Contract.Assume(optional != null);
		}

		/// <summary>
		/// Adds Javascript snippets to the page to help the Information Card selector do its work,
		/// or to downgrade gracefully if the user agent lacks an Information Card selector.
		/// </summary>
		private void RenderSupportingScript() {
			Requires.ValidState(this.infoCardSupportedPanel != null);

			this.Page.ClientScript.RegisterClientScriptResource(typeof(InfoCardSelector), ScriptResourceName);

			if (this.RenderMode == RenderMode.Static) {
				this.Page.ClientScript.RegisterStartupScript(
					typeof(InfoCardSelector),
					"SelectorSupportingScript_" + this.ClientID,
					string.Format(CultureInfo.InvariantCulture, "document.infoCard.checkStatic('{0}', '{1}');", this.infoCardSupportedPanel.ClientID, this.infoCardNotSupportedPanel.ClientID),
					true);
			} else if (RenderMode == RenderMode.Dynamic) {
				this.Page.ClientScript.RegisterStartupScript(
					typeof(InfoCardSelector),
					"SelectorSupportingScript_" + this.ClientID,
					string.Format(CultureInfo.InvariantCulture, "document.infoCard.checkDynamic('{0}', '{1}');", this.infoCardSupportedPanel.ClientID, this.infoCardNotSupportedPanel.ClientID),
					true);
			}
		}
	}
}
