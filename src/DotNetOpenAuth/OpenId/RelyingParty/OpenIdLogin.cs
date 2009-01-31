//-----------------------------------------------------------------------
// <copyright file="OpenIdLogin.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;

	/// <summary>
	/// An ASP.NET control providing a complete OpenID login experience.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Legacy code")]
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdLogin runat=\"server\" />")]
	public class OpenIdLogin : OpenIdTextBox {
		#region Property defaults

		/// <summary>
		/// The default value for the <see cref="RegisterToolTip"/> property.
		/// </summary>
		private const string RegisterToolTipDefault = "Sign up free for an OpenID with MyOpenID now.";

		/// <summary>
		/// The default value for the <see cref="RememberMeText"/> property.
		/// </summary>
		private const string RememberMeTextDefault = "Remember me";

		/// <summary>
		/// The default value for the <see cref="ButtonText"/> property.
		/// </summary>
		private const string ButtonTextDefault = "Login »";

		/// <summary>
		/// The default value for the <see cref="CanceledText"/> property.
		/// </summary>
		private const string CanceledTextDefault = "Login canceled.";

		/// <summary>
		/// The default value for the <see cref="FailedMessageText"/> property.
		/// </summary>
		private const string FailedMessageTextDefault = "Login failed: {0}";

		/// <summary>
		/// The default value for the <see cref="ExamplePrefix"/> property.
		/// </summary>
		private const string ExamplePrefixDefault = "Example:";

		/// <summary>
		/// The default value for the <see cref="ExampleUrl"/> property.
		/// </summary>
		private const string ExampleUrlDefault = "http://your.name.myopenid.com";

		/// <summary>
		/// The default value for the <see cref="LabelText"/> property.
		/// </summary>
		private const string LabelTextDefault = "OpenID Login:";

		/// <summary>
		/// The default value for the <see cref="RequiredText"/> property.
		/// </summary>
		private const string RequiredTextDefault = "Provide an OpenID first.";

		/// <summary>
		/// The default value for the <see cref="UriFormatText"/> property.
		/// </summary>
		private const string UriFormatTextDefault = "Invalid OpenID URL.";

		/// <summary>
		/// The default value for the <see cref="RegisterText"/> property.
		/// </summary>
		private const string RegisterTextDefault = "register";

		/// <summary>
		/// The default value for the <see cref="RegisterUrl"/> property.
		/// </summary>
		private const string RegisterUrlDefault = "https://www.myopenid.com/signup";

		/// <summary>
		/// The default value for the <see cref="ButtonToolTip"/> property.
		/// </summary>
		private const string ButtonToolTipDefault = "Account login";

		/// <summary>
		/// The default value for the <see cref="ValidationGroup"/> property.
		/// </summary>
		private const string ValidationGroupDefault = "OpenIdLogin";

		/// <summary>
		/// The default value for the <see cref="RegisterVisible"/> property.
		/// </summary>
		private const bool RegisterVisibleDefault = true;

		/// <summary>
		/// The default value for the <see cref="RememberMeVisible"/> property.
		/// </summary>
		private const bool RememberMeVisibleDefault = false;

		/// <summary>
		/// The default value for the <see cref="RememberMe"/> property.
		/// </summary>
		private const bool RememberMeDefault = UsePersistentCookieDefault;

		/// <summary>
		/// The default value for the <see cref="UriValidatorEnabled"/> property.
		/// </summary>
		private const bool UriValidatorEnabledDefault = true;

		#endregion

		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for the <see cref="FailedMessageText"/> property.
		/// </summary>
		private const string FailedMessageTextViewStateKey = "FailedMessageText";

		/// <summary>
		/// The viewstate key to use for the <see cref="CanceledText"/> property.
		/// </summary>
		private const string CanceledTextViewStateKey = "CanceledText";

		/// <summary>
		/// The viewstate key to use for the <see cref="IdSelectorIdentifier"/> property.
		/// </summary>
		private const string IdSelectorIdentifierViewStateKey = "IdSelectorIdentifier";

		#endregion

		/// <summary>
		/// The HTML to append to the <see cref="RequiredText"/> property value when rendering.
		/// </summary>
		private const string RequiredTextSuffix = "<br/>";

		/// <summary>
		/// The number to add to <see cref="TabIndex"/> to get the tab index of the textbox control.
		/// </summary>
		private const short TextBoxTabIndexOffset = 0;

		/// <summary>
		/// The number to add to <see cref="TabIndex"/> to get the tab index of the login button control.
		/// </summary>
		private const short LoginButtonTabIndexOffset = 1;

		/// <summary>
		/// The number to add to <see cref="TabIndex"/> to get the tab index of the remember me checkbox control.
		/// </summary>
		private const short RememberMeTabIndexOffset = 2;

		/// <summary>
		/// The number to add to <see cref="TabIndex"/> to get the tab index of the register link control.
		/// </summary>
		private const short RegisterTabIndexOffset = 3;

		#region Controls

		/// <summary>
		/// The control into which all other controls are added.
		/// </summary>
		private Panel panel;

		/// <summary>
		/// The Login button.
		/// </summary>
		private Button loginButton;

		/// <summary>
		/// The label that presents the text box.
		/// </summary>
		private HtmlGenericControl label;

		/// <summary>
		/// The validator that flags an empty text box.
		/// </summary>
		private RequiredFieldValidator requiredValidator;

		/// <summary>
		/// The validator that flags invalid formats of OpenID identifiers.
		/// </summary>
		private CustomValidator identifierFormatValidator;

		/// <summary>
		/// The label that precedes an example OpenID identifier.
		/// </summary>
		private Label examplePrefixLabel;

		/// <summary>
		/// The label that contains the example OpenID identifier.
		/// </summary>
		private Label exampleUrlLabel;

		/// <summary>
		/// A link to allow the user to create an account with a popular OpenID Provider.
		/// </summary>
		private HyperLink registerLink;

		/// <summary>
		/// The Remember Me checkbox.
		/// </summary>
		private CheckBox rememberMeCheckBox;

		/// <summary>
		/// The javascript snippet that activates the ID Selector javascript control.
		/// </summary>
		private Literal idselectorJavascript;

		/// <summary>
		/// The label that will display login failure messages.
		/// </summary>
		private Label errorLabel;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdLogin"/> class.
		/// </summary>
		public OpenIdLogin() {
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
		/// Fired when the Remember Me checkbox is changed by the user.
		/// </summary>
		[Description("Fires when the Remember Me checkbox is changed by the user.")]
		public event EventHandler RememberMeChanged;

		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the caption that appears before the text box.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(LabelTextDefault)]
		[Localizable(true)]
		[Description("The caption that appears before the text box.")]
		public string LabelText {
			get { return this.label.InnerText; }
			set { this.label.InnerText = value; }
		}

		/// <summary>
		/// Gets or sets the text that introduces the example OpenID url.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(ExamplePrefixDefault)]
		[Localizable(true)]
		[Description("The text that introduces the example OpenID url.")]
		public string ExamplePrefix {
			get { return this.examplePrefixLabel.Text; }
			set { this.examplePrefixLabel.Text = value; }
		}

		/// <summary>
		/// Gets or sets the example OpenID Identifier to display to the user.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid only supports primitive types.")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(ExampleUrlDefault)]
		[Localizable(true)]
		[Description("The example OpenID Identifier to display to the user.")]
		public string ExampleUrl {
			get { return this.exampleUrlLabel.Text; }
			set { this.exampleUrlLabel.Text = value; }
		}

		/// <summary>
		/// Gets or sets the text to display if the user attempts to login 
		/// without providing an Identifier.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RequiredTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user attempts to login without providing an Identifier.")]
		public string RequiredText {
			get { return this.requiredValidator.Text.Substring(0, this.requiredValidator.Text.Length - RequiredTextSuffix.Length); }
			set { this.requiredValidator.ErrorMessage = this.requiredValidator.Text = value + RequiredTextSuffix; }
		}

		/// <summary>
		/// Gets or sets the text to display if the user provides an invalid form for an Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid only supports primitive types.")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(UriFormatTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user provides an invalid form for an Identifier.")]
		public string UriFormatText {
			get { return this.identifierFormatValidator.Text.Substring(0, this.identifierFormatValidator.Text.Length - RequiredTextSuffix.Length); }
			set { this.identifierFormatValidator.ErrorMessage = this.identifierFormatValidator.Text = value + RequiredTextSuffix; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to perform Identifier 
		/// format validation prior to an authentication attempt.
		/// </summary>
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(UriValidatorEnabledDefault)]
		[Description("Whether to perform Identifier format validation prior to an authentication attempt.")]
		public bool UriValidatorEnabled {
			get { return this.identifierFormatValidator.Enabled; }
			set { this.identifierFormatValidator.Enabled = value; }
		}

		/// <summary>
		/// Gets or sets the text of the link users can click on to obtain an OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RegisterTextDefault)]
		[Localizable(true)]
		[Description("The text of the link users can click on to obtain an OpenID.")]
		public string RegisterText {
			get { return this.registerLink.Text; }
			set { this.registerLink.Text = value; }
		}

		/// <summary>
		/// Gets or sets the URL to link users to who click the link to obtain a new OpenID.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid only supports primitive types.")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RegisterUrlDefault)]
		[Localizable(true)]
		[Description("The URL to link users to who click the link to obtain a new OpenID.")]
		public string RegisterUrl {
			get { return this.registerLink.NavigateUrl; }
			set { this.registerLink.NavigateUrl = value; }
		}

		/// <summary>
		/// Gets or sets the text of the tooltip to display when the user hovers 
		/// over the link to obtain a new OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RegisterToolTipDefault)]
		[Localizable(true)]
		[Description("The text of the tooltip to display when the user hovers over the link to obtain a new OpenID.")]
		public string RegisterToolTip {
			get { return this.registerLink.ToolTip; }
			set { this.registerLink.ToolTip = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to display a link to 
		/// allow users to easily obtain a new OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RegisterVisibleDefault)]
		[Description("Whether to display a link to allow users to easily obtain a new OpenID.")]
		public bool RegisterVisible {
			get { return this.registerLink.Visible; }
			set { this.registerLink.Visible = value; }
		}

		/// <summary>
		/// Gets or sets the text that appears on the button that initiates login.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(ButtonTextDefault)]
		[Localizable(true)]
		[Description("The text that appears on the button that initiates login.")]
		public string ButtonText {
			get { return this.loginButton.Text; }
			set { this.loginButton.Text = value; }
		}

		/// <summary>
		/// Gets or sets the text of the "Remember Me" checkbox.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RememberMeTextDefault)]
		[Localizable(true)]
		[Description("The text of the \"Remember Me\" checkbox.")]
		public string RememberMeText {
			get { return this.rememberMeCheckBox.Text; }
			set { this.rememberMeCheckBox.Text = value; }
		}

		/// <summary>
		/// Gets or sets the message display in the event of a failed 
		/// authentication.  {0} may be used to insert the actual error.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(FailedMessageTextDefault)]
		[Localizable(true)]
		[Description("The message display in the event of a failed authentication.  {0} may be used to insert the actual error.")]
		public string FailedMessageText {
			get { return (string)ViewState[FailedMessageTextViewStateKey] ?? FailedMessageTextDefault; }
			set { ViewState[FailedMessageTextViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the text to display in the event of an authentication canceled at the Provider.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(CanceledTextDefault)]
		[Localizable(true)]
		[Description("The text to display in the event of an authentication canceled at the Provider.")]
		public string CanceledText {
			get { return (string)ViewState[CanceledTextViewStateKey] ?? CanceledTextDefault; }
			set { ViewState[CanceledTextViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the "Remember Me" checkbox should be displayed.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RememberMeVisibleDefault)]
		[Description("Whether the \"Remember Me\" checkbox should be displayed.")]
		public bool RememberMeVisible {
			get { return this.rememberMeCheckBox.Visible; }
			set { this.rememberMeCheckBox.Visible = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether a successful authentication should result in a persistent
		/// cookie being saved to the browser.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(UsePersistentCookieDefault)]
		[Description("Whether a successful authentication should result in a persistent cookie being saved to the browser.")]
		public bool RememberMe {
			get { return this.UsePersistentCookie; }
			set { this.UsePersistentCookie = value; }
		}

		/// <summary>
		/// Gets or sets the starting tab index to distribute across the controls.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+1", Justification = "Overflow would provide desired UI behavior.")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+2", Justification = "Overflow would provide desired UI behavior.")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+3", Justification = "Overflow would provide desired UI behavior.")]
		public override short TabIndex {
			get {
				return base.TabIndex;
			}

			set {
				unchecked {
					this.WrappedTextBox.TabIndex = (short)(value + TextBoxTabIndexOffset);
					this.loginButton.TabIndex = (short)(value + LoginButtonTabIndexOffset);
					this.rememberMeCheckBox.TabIndex = (short)(value + RememberMeTabIndexOffset);
					this.registerLink.TabIndex = (short)(value + RegisterTabIndexOffset);
				}
			}
		}

		/// <summary>
		/// Gets or sets the tooltip to display when the user hovers over the login button.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(ButtonToolTipDefault)]
		[Localizable(true)]
		[Description("The tooltip to display when the user hovers over the login button.")]
		public string ButtonToolTip {
			get { return this.loginButton.ToolTip; }
			set { this.loginButton.ToolTip = value; }
		}

		/// <summary>
		/// Gets or sets the validation group that the login button and text box validator belong to.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(ValidationGroupDefault)]
		[Description("The validation group that the login button and text box validator belong to.")]
		public string ValidationGroup {
			get {
				return this.requiredValidator.ValidationGroup;
			}

			set {
				this.requiredValidator.ValidationGroup = value;
				this.loginButton.ValidationGroup = value;
			}
		}

		/// <summary>
		/// Gets or sets the unique hash string that ends your idselector.com account.
		/// </summary>
		[Category("Behavior")]
		[Description("The unique hash string that ends your idselector.com account.")]
		public string IdSelectorIdentifier {
			get { return (string)(ViewState[IdSelectorIdentifierViewStateKey]); }
			set { ViewState[IdSelectorIdentifierViewStateKey] = value; }
		}

		#endregion

		#region Properties to hide

		/// <summary>
		/// Gets or sets a value indicating whether a FormsAuthentication 
		/// cookie should persist across user sessions.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override bool UsePersistentCookie {
			get {
				return base.UsePersistentCookie;
			}

			set {
				base.UsePersistentCookie = value;

				// use conditional here to prevent infinite recursion
				// with CheckedChanged event.
				if (this.rememberMeCheckBox.Checked != value) {
					this.rememberMeCheckBox.Checked = value;
				}
			}
		}

		#endregion

		/// <summary>
		/// Creates the child controls.
		/// </summary>
		protected override void CreateChildControls() {
			// Don't call base.CreateChildControls().  This would add the WrappedTextBox
			// to the Controls collection, which would implicitly remove it from the table
			// we have already added it to.

			// Just add the panel we've assembled earlier.
			this.Controls.Add(this.panel);

			if (ShouldBeFocused) {
				WrappedTextBox.Focus();
			}
		}

		/// <summary>
		/// Initializes the child controls.
		/// </summary>
		protected override void InitializeControls() {
			base.InitializeControls();

			this.panel = new Panel();

			Table table = new Table();
			TableRow row1, row2, row3;
			TableCell cell;
			table.Rows.Add(row1 = new TableRow());
			table.Rows.Add(row2 = new TableRow());
			table.Rows.Add(row3 = new TableRow());

			// top row, left cell
			cell = new TableCell();
			this.label = new HtmlGenericControl("label");
			this.label.InnerText = LabelTextDefault;
			cell.Controls.Add(this.label);
			row1.Cells.Add(cell);

			// top row, middle cell
			cell = new TableCell();
			cell.Controls.Add(this.WrappedTextBox);
			row1.Cells.Add(cell);

			// top row, right cell
			cell = new TableCell();
			this.loginButton = new Button();
			this.loginButton.ID = "loginButton";
			this.loginButton.Text = ButtonTextDefault;
			this.loginButton.ToolTip = ButtonToolTipDefault;
			this.loginButton.Click += this.LoginButton_Click;
			this.loginButton.ValidationGroup = ValidationGroupDefault;
#if !Mono
			this.panel.DefaultButton = this.loginButton.ID;
#endif
			cell.Controls.Add(this.loginButton);
			row1.Cells.Add(cell);

			// middle row, left cell
			row2.Cells.Add(new TableCell());

			// middle row, middle cell
			cell = new TableCell();
			cell.Style[HtmlTextWriterStyle.Color] = "gray";
			cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
			this.requiredValidator = new RequiredFieldValidator();
			this.requiredValidator.ErrorMessage = RequiredTextDefault + RequiredTextSuffix;
			this.requiredValidator.Text = RequiredTextDefault + RequiredTextSuffix;
			this.requiredValidator.Display = ValidatorDisplay.Dynamic;
			this.requiredValidator.ControlToValidate = WrappedTextBox.ID;
			this.requiredValidator.ValidationGroup = ValidationGroupDefault;
			cell.Controls.Add(this.requiredValidator);
			this.identifierFormatValidator = new CustomValidator();
			this.identifierFormatValidator.ErrorMessage = UriFormatTextDefault + RequiredTextSuffix;
			this.identifierFormatValidator.Text = UriFormatTextDefault + RequiredTextSuffix;
			this.identifierFormatValidator.ServerValidate += this.IdentifierFormatValidator_ServerValidate;
			this.identifierFormatValidator.Enabled = UriValidatorEnabledDefault;
			this.identifierFormatValidator.Display = ValidatorDisplay.Dynamic;
			this.identifierFormatValidator.ControlToValidate = WrappedTextBox.ID;
			this.identifierFormatValidator.ValidationGroup = ValidationGroupDefault;
			cell.Controls.Add(this.identifierFormatValidator);
			this.errorLabel = new Label();
			this.errorLabel.EnableViewState = false;
			this.errorLabel.ForeColor = System.Drawing.Color.Red;
			this.errorLabel.Style[HtmlTextWriterStyle.Display] = "block"; // puts it on its own line
			this.errorLabel.Visible = false;
			cell.Controls.Add(this.errorLabel);
			this.examplePrefixLabel = new Label();
			this.examplePrefixLabel.Text = ExamplePrefixDefault;
			cell.Controls.Add(this.examplePrefixLabel);
			cell.Controls.Add(new LiteralControl(" "));
			this.exampleUrlLabel = new Label();
			this.exampleUrlLabel.Font.Bold = true;
			this.exampleUrlLabel.Text = ExampleUrlDefault;
			cell.Controls.Add(this.exampleUrlLabel);
			row2.Cells.Add(cell);

			// middle row, right cell
			cell = new TableCell();
			cell.Style[HtmlTextWriterStyle.Color] = "gray";
			cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
			cell.Style[HtmlTextWriterStyle.TextAlign] = "center";
			this.registerLink = new HyperLink();
			this.registerLink.Text = RegisterTextDefault;
			this.registerLink.ToolTip = RegisterToolTipDefault;
			this.registerLink.NavigateUrl = RegisterUrlDefault;
			this.registerLink.Visible = RegisterVisibleDefault;
			cell.Controls.Add(this.registerLink);
			row2.Cells.Add(cell);

			// bottom row, left cell
			cell = new TableCell();
			row3.Cells.Add(cell);

			// bottom row, middle cell
			cell = new TableCell();
			this.rememberMeCheckBox = new CheckBox();
			this.rememberMeCheckBox.Text = RememberMeTextDefault;
			this.rememberMeCheckBox.Checked = RememberMeDefault;
			this.rememberMeCheckBox.Visible = RememberMeVisibleDefault;
			this.rememberMeCheckBox.CheckedChanged += this.RememberMeCheckBox_CheckedChanged;
			cell.Controls.Add(this.rememberMeCheckBox);
			row3.Cells.Add(cell);

			// bottom row, right cell
			cell = new TableCell();
			row3.Cells.Add(cell);

			// this sets all the controls' tab indexes
			this.TabIndex = TabIndexDefault;

			this.panel.Controls.Add(table);

			this.idselectorJavascript = new Literal();
			this.panel.Controls.Add(this.idselectorJavascript);
		}

		/// <summary>
		/// Customizes HTML rendering of the control.
		/// </summary>
		/// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
		protected override void Render(HtmlTextWriter writer) {
			// avoid writing begin and end SPAN tags for XHTML validity.
			RenderContents(writer);
		}

		/// <summary>
		/// Renders the child controls.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the rendered content.</param>
		protected override void RenderChildren(HtmlTextWriter writer) {
			if (!this.DesignMode) {
				this.label.Attributes["for"] = this.WrappedTextBox.ClientID;

				if (!string.IsNullOrEmpty(this.IdSelectorIdentifier)) {
					this.idselectorJavascript.Visible = true;
					this.idselectorJavascript.Text = @"<script type='text/javascript'><!--
idselector_input_id = '" + WrappedTextBox.ClientID + @"';
// --></script>
<script type='text/javascript' id='__openidselector' src='https://www.idselector.com/selector/" + this.IdSelectorIdentifier + @"' charset='utf-8'></script>";
				} else {
					this.idselectorJavascript.Visible = false;
				}
			}

			base.RenderChildren(writer);
		}

		/// <summary>
		/// Adds failure handling to display an error message to the user.
		/// </summary>
		/// <param name="response">The response.</param>
		protected override void OnFailed(IAuthenticationResponse response) {
			base.OnFailed(response);

			if (!string.IsNullOrEmpty(this.FailedMessageText)) {
				this.errorLabel.Text = string.Format(CultureInfo.CurrentCulture, this.FailedMessageText, response.Exception.Message);
				this.errorLabel.Visible = true;
			}
		}

		/// <summary>
		/// Adds authentication cancellation behavior to display a message to the user.
		/// </summary>
		/// <param name="response">The response.</param>
		protected override void OnCanceled(IAuthenticationResponse response) {
			base.OnCanceled(response);

			if (!string.IsNullOrEmpty(this.CanceledText)) {
				this.errorLabel.Text = this.CanceledText;
				this.errorLabel.Visible = true;
			}
		}

		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		/// <returns>
		/// Returns whether the login should proceed.  False if some event handler canceled the request.
		/// </returns>
		protected virtual bool OnLoggingIn() {
			EventHandler<OpenIdEventArgs> loggingIn = this.LoggingIn;
			if (this.Request == null) {
				this.CreateRequest();
			}

			if (this.Request != null) {
				OpenIdEventArgs args = new OpenIdEventArgs(this.Request);
				if (loggingIn != null) {
					loggingIn(this, args);
				}

				return !args.Cancel;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Fires the <see cref="RememberMeChanged"/> event.
		/// </summary>
		protected virtual void OnRememberMeChanged() {
			EventHandler rememberMeChanged = this.RememberMeChanged;
			if (rememberMeChanged != null) {
				rememberMeChanged(this, new EventArgs());
			}
		}

		/// <summary>
		/// Handles the ServerValidate event of the identifierFormatValidator control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		private void IdentifierFormatValidator_ServerValidate(object source, ServerValidateEventArgs args) {
			args.IsValid = Identifier.IsValid(args.Value);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the rememberMeCheckBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void RememberMeCheckBox_CheckedChanged(object sender, EventArgs e) {
			this.RememberMe = this.rememberMeCheckBox.Checked;
			this.OnRememberMeChanged();
		}

		/// <summary>
		/// Handles the Click event of the loginButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void LoginButton_Click(object sender, EventArgs e) {
			if (!this.Page.IsValid) {
				return;
			}

			if (this.OnLoggingIn()) {
				this.LogOn();
			}
		}
	}
}
