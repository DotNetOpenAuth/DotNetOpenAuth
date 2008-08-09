/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty
{
	/// <summary>
	/// An ASP.NET control providing a complete OpenID login experience.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdLogin runat=\"server\"></{0}:OpenIdLogin>")]
	public class OpenIdLogin : OpenIdTextBox
	{
		Panel panel;
		Button loginButton;
		HtmlGenericControl label;
		RequiredFieldValidator requiredValidator;
		CustomValidator identifierFormatValidator;
		Label examplePrefixLabel;
		Label exampleUrlLabel;
		HyperLink registerLink;
		CheckBox rememberMeCheckBox;
		Literal idselectorJavascript;

		const short textBoxTabIndexOffset = 0;
		const short loginButtonTabIndexOffset = 1;
		const short rememberMeTabIndexOffset = 2;
		const short registerTabIndexOffset = 3;

		/// <summary>
		/// Creates the child controls.
		/// </summary>
		protected override void CreateChildControls()
		{
			// Don't call base.CreateChildControls().  This would add the WrappedTextBox
			// to the Controls collection, which would implicitly remove it from the table
			// we have already added it to.

			// Just add the panel we've assembled earlier.
			Controls.Add(panel);

			if (ShouldBeFocused)
				WrappedTextBox.Focus();
		}

		/// <summary>
		/// Initializes the child controls.
		/// </summary>
		protected override void InitializeControls()
		{
			base.InitializeControls();

			panel = new Panel();

			Table table = new Table();
			TableRow row1, row2, row3;
			TableCell cell;
			table.Rows.Add(row1 = new TableRow());
			table.Rows.Add(row2 = new TableRow());
			table.Rows.Add(row3 = new TableRow());

			// top row, left cell
			cell = new TableCell();
			label = new HtmlGenericControl("label");
			label.InnerText = labelTextDefault;
			cell.Controls.Add(label);
			row1.Cells.Add(cell);

			// top row, middle cell
			cell = new TableCell();
			cell.Controls.Add(WrappedTextBox);
			row1.Cells.Add(cell);

			// top row, right cell
			cell = new TableCell();
			loginButton = new Button();
			loginButton.ID = "loginButton";
			loginButton.Text = buttonTextDefault;
			loginButton.ToolTip = buttonToolTipDefault;
			loginButton.Click += new EventHandler(loginButton_Click);
			loginButton.ValidationGroup = validationGroupDefault;
#if !Mono
			panel.DefaultButton = loginButton.ID;
#endif
			cell.Controls.Add(loginButton);
			row1.Cells.Add(cell);

			// middle row, left cell
			row2.Cells.Add(new TableCell());

			// middle row, middle cell
			cell = new TableCell();
			cell.Style[HtmlTextWriterStyle.Color] = "gray";
			cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
			requiredValidator = new RequiredFieldValidator();
			requiredValidator.ErrorMessage = requiredTextDefault + requiredTextSuffix;
			requiredValidator.Text = requiredTextDefault + requiredTextSuffix;
			requiredValidator.Display = ValidatorDisplay.Dynamic;
			requiredValidator.ControlToValidate = WrappedTextBox.ID;
			requiredValidator.ValidationGroup = validationGroupDefault;
			cell.Controls.Add(requiredValidator);
			identifierFormatValidator = new CustomValidator();
			identifierFormatValidator.ErrorMessage = uriFormatTextDefault + requiredTextSuffix;
			identifierFormatValidator.Text = uriFormatTextDefault + requiredTextSuffix;
			identifierFormatValidator.ServerValidate += new ServerValidateEventHandler(identifierFormatValidator_ServerValidate);
			identifierFormatValidator.Enabled = uriValidatorEnabledDefault;
			identifierFormatValidator.Display = ValidatorDisplay.Dynamic;
			identifierFormatValidator.ControlToValidate = WrappedTextBox.ID;
			identifierFormatValidator.ValidationGroup = validationGroupDefault;
			cell.Controls.Add(identifierFormatValidator);
			examplePrefixLabel = new Label();
			examplePrefixLabel.Text = examplePrefixDefault;
			cell.Controls.Add(examplePrefixLabel);
			cell.Controls.Add(new LiteralControl(" "));
			exampleUrlLabel = new Label();
			exampleUrlLabel.Font.Bold = true;
			exampleUrlLabel.Text = exampleUrlDefault;
			cell.Controls.Add(exampleUrlLabel);
			row2.Cells.Add(cell);

			// middle row, right cell
			cell = new TableCell();
			cell.Style[HtmlTextWriterStyle.Color] = "gray";
			cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
			cell.Style[HtmlTextWriterStyle.TextAlign] = "center";
			registerLink = new HyperLink();
			registerLink.Text = registerTextDefault;
			registerLink.ToolTip = registerToolTipDefault;
			registerLink.NavigateUrl = registerUrlDefault;
			registerLink.Visible = registerVisibleDefault;
			cell.Controls.Add(registerLink);
			row2.Cells.Add(cell);

			// bottom row, left cell
			cell = new TableCell();
			row3.Cells.Add(cell);

			// bottom row, middle cell
			cell = new TableCell();
			rememberMeCheckBox = new CheckBox();
			rememberMeCheckBox.Text = rememberMeTextDefault;
			rememberMeCheckBox.Checked = rememberMeDefault;
			rememberMeCheckBox.Visible = rememberMeVisibleDefault;
			rememberMeCheckBox.CheckedChanged += new EventHandler(rememberMeCheckBox_CheckedChanged);
			cell.Controls.Add(rememberMeCheckBox);
			row3.Cells.Add(cell);

			// bottom row, right cell
			cell = new TableCell();
			row3.Cells.Add(cell);

			// this sets all the controls' tab indexes
			TabIndex = TabIndexDefault;

			panel.Controls.Add(table);

			idselectorJavascript = new Literal();
			panel.Controls.Add(idselectorJavascript);
		}

		void identifierFormatValidator_ServerValidate(object source, ServerValidateEventArgs args) {
			args.IsValid = Identifier.IsValid(args.Value);
		}

		void rememberMeCheckBox_CheckedChanged(object sender, EventArgs e) {
			RememberMe = rememberMeCheckBox.Checked;
			OnRememberMeChanged();
		}

		/// <summary>
		/// Customizes HTML rendering of the control.
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			// avoid writing begin and end SPAN tags for XHTML validity.
			RenderContents(writer);
		}

		/// <summary>
		/// Renders the child controls.
		/// </summary>
		protected override void RenderChildren(HtmlTextWriter writer)
		{
			if (!this.DesignMode) {
				label.Attributes["for"] = WrappedTextBox.ClientID;

				if (!string.IsNullOrEmpty(IdSelectorIdentifier)) {
					idselectorJavascript.Visible = true;
					idselectorJavascript.Text = @"<script type='text/javascript'><!--
idselector_input_id = '" + WrappedTextBox.ClientID + @"';
// --></script>
<script type='text/javascript' id='__openidselector' src='https://www.idselector.com/selector/" + IdSelectorIdentifier + @"' charset='utf-8'></script>";
				} else {
					idselectorJavascript.Visible = false;
				}
			}

			base.RenderChildren(writer);
		}

		#region Properties
		const string labelTextDefault = "OpenID Login:";
		/// <summary>
		/// The caption that appears before the text box.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(labelTextDefault)]
		[Localizable(true)]
		[Description("The caption that appears before the text box.")]
		public string LabelText
		{
			get { return label.InnerText; }
			set { label.InnerText = value; }
		}

		const string examplePrefixDefault = "Example:";
		/// <summary>
		/// The text that introduces the example OpenID url.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(examplePrefixDefault)]
		[Localizable(true)]
		[Description("The text that introduces the example OpenID url.")]
		public string ExamplePrefix
		{
			get { return examplePrefixLabel.Text; }
			set { examplePrefixLabel.Text = value; }
		}

		const string exampleUrlDefault = "http://your.name.myopenid.com";
		/// <summary>
		/// The example OpenID Identifier to display to the user.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(exampleUrlDefault)]
		[Localizable(true)]
		[Description("The example OpenID Identifier to display to the user.")]
		public string ExampleUrl
		{
			get { return exampleUrlLabel.Text; }
			set { exampleUrlLabel.Text = value; }
		}

		const string requiredTextSuffix = "<br/>";
		const string requiredTextDefault = "Provide an OpenID first.";
		/// <summary>
		/// The text to display if the user attempts to login without providing an Identifier.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(requiredTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user attempts to login without providing an Identifier.")]
		public string RequiredText
		{
			get { return requiredValidator.Text.Substring(0, requiredValidator.Text.Length - requiredTextSuffix.Length); }
			set { requiredValidator.ErrorMessage = requiredValidator.Text = value + requiredTextSuffix; }
		}

		const string uriFormatTextDefault = "Invalid OpenID URL.";
		/// <summary>
		/// The text to display if the user provides an invalid form for an Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(uriFormatTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user provides an invalid form for an Identifier.")]
		public string UriFormatText
		{
			get { return identifierFormatValidator.Text.Substring(0, identifierFormatValidator.Text.Length - requiredTextSuffix.Length); }
			set { identifierFormatValidator.ErrorMessage = identifierFormatValidator.Text = value + requiredTextSuffix; }
		}

		const bool uriValidatorEnabledDefault = true;
		/// <summary>
		/// Whether to perform Identifier format validation prior to an authentication attempt.
		/// </summary>
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(uriValidatorEnabledDefault)]
		[Description("Whether to perform Identifier format validation prior to an authentication attempt.")]
		public bool UriValidatorEnabled
		{
			get { return identifierFormatValidator.Enabled; }
			set { identifierFormatValidator.Enabled = value; }
		}

		const string registerTextDefault = "register";
		/// <summary>
		/// The text of the link users can click on to obtain an OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerTextDefault)]
		[Localizable(true)]
		[Description("The text of the link users can click on to obtain an OpenID.")]
		public string RegisterText
		{
			get { return registerLink.Text; }
			set { registerLink.Text = value; }
		}

		const string registerUrlDefault = "https://www.myopenid.com/signup";
		/// <summary>
		/// The URL to link users to who click the link to obtain a new OpenID.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerUrlDefault)]
		[Localizable(true)]
		[Description("The URL to link users to who click the link to obtain a new OpenID.")]
		public string RegisterUrl
		{
			get { return registerLink.NavigateUrl; }
			set { registerLink.NavigateUrl = value; }
		}

		const string registerToolTipDefault = "Sign up free for an OpenID with MyOpenID now.";
		/// <summary>
		/// The text of the tooltip to display when the user hovers over the link to obtain a new OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerToolTipDefault)]
		[Localizable(true)]
		[Description("The text of the tooltip to display when the user hovers over the link to obtain a new OpenID.")]
		public string RegisterToolTip
		{
			get { return registerLink.ToolTip; }
			set { registerLink.ToolTip = value; }
		}

		const bool registerVisibleDefault = true;
		/// <summary>
		/// Whether to display a link to allow users to easily obtain a new OpenID.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerVisibleDefault)]
		[Description("Whether to display a link to allow users to easily obtain a new OpenID.")]
		public bool RegisterVisible {
			get { return registerLink.Visible; }
			set { registerLink.Visible = value; }
		}

		const string buttonTextDefault = "Login »";
		/// <summary>
		/// The text that appears on the button that initiates login.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(buttonTextDefault)]
		[Localizable(true)]
		[Description("The text that appears on the button that initiates login.")]
		public string ButtonText
		{
			get { return loginButton.Text; }
			set { loginButton.Text = value; }
		}

		const string rememberMeTextDefault = "Remember me";
		/// <summary>
		/// The text of the "Remember Me" checkbox.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(rememberMeTextDefault)]
		[Localizable(true)]
		[Description("The text of the \"Remember Me\" checkbox.")]
		public string RememberMeText {
			get { return rememberMeCheckBox.Text; }
			set { rememberMeCheckBox.Text = value; }
		}

		const bool rememberMeVisibleDefault = false;
		/// <summary>
		/// Whether the "Remember Me" checkbox should be displayed.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(rememberMeVisibleDefault)]
		[Description("Whether the \"Remember Me\" checkbox should be displayed.")]
		public bool RememberMeVisible {
			get { return rememberMeCheckBox.Visible; }
			set { rememberMeCheckBox.Visible = value; }
		}

		const bool rememberMeDefault = UsePersistentCookieDefault;
		/// <summary>
		/// Whether a successful authentication should result in a persistent
		/// cookie being saved to the browser.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(UsePersistentCookieDefault)]
		[Description("Whether a successful authentication should result in a persistent cookie being saved to the browser.")]
		public bool RememberMe {
			get { return UsePersistentCookie; }
			set { UsePersistentCookie = value; }
		}

		/// <summary>
		/// The starting tab index to distribute across the controls.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value+2")]
		public override short TabIndex {
			get { return base.TabIndex; }
			set {
				WrappedTextBox.TabIndex = (short)(value + textBoxTabIndexOffset);
				loginButton.TabIndex = (short)(value + loginButtonTabIndexOffset);
				rememberMeCheckBox.TabIndex = (short)(value + rememberMeTabIndexOffset);
				registerLink.TabIndex = (short)(value + registerTabIndexOffset);
			}
		}

		const string buttonToolTipDefault = "Account login";
		/// <summary>
		/// The tooltip to display when the user hovers over the login button.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(buttonToolTipDefault)]
		[Localizable(true)]
		[Description("The tooltip to display when the user hovers over the login button.")]
		public string ButtonToolTip
		{
			get { return loginButton.ToolTip; }
			set { loginButton.ToolTip = value; }
		}

		const string validationGroupDefault = "OpenIdLogin";
		/// <summary>
		/// The validation group that the login button and text box validator belong to.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(validationGroupDefault)]
		[Description("The validation group that the login button and text box validator belong to.")]
		public string ValidationGroup
		{
			get { return requiredValidator.ValidationGroup; }
			set
			{
				requiredValidator.ValidationGroup = value;
				loginButton.ValidationGroup = value;
			}
		}

		const string idSelectorIdentifierViewStateKey = "IdSelectorIdentifier";
		/// <summary>
		/// The unique hash string that ends your idselector.com account.
		/// </summary>
		[Category("Behavior")]
		[Description("The unique hash string that ends your idselector.com account.")]
		public string IdSelectorIdentifier {
			get { return (string)(ViewState[idSelectorIdentifierViewStateKey]); }
			set { ViewState[idSelectorIdentifierViewStateKey] = value; }
		}
		#endregion

		#region Properties to hide
		/// <summary>
		/// Whether a FormsAuthentication cookie should persist across user sessions.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override bool UsePersistentCookie {
			get { return base.UsePersistentCookie; }
			set {
				base.UsePersistentCookie = value;
				// use conditional here to prevent infinite recursion
				// with CheckedChanged event.
				if (rememberMeCheckBox.Checked != value) {
					rememberMeCheckBox.Checked = value;
				}
			}
		}
		#endregion

		#region Event handlers
		void loginButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid) return;
			if (OnLoggingIn())
				LogOn();
		}

		#endregion

		#region Events
		/// <summary>
		/// Fired after the user clicks the log in button, but before the authentication
		/// process begins.  Offers a chance for the web application to disallow based on 
		/// OpenID URL before redirecting the user to the OpenID Provider.
		/// </summary>
		[Description("Fired after the user clicks the log in button, but before the authentication process begins.  Offers a chance for the web application to disallow based on OpenID URL before redirecting the user to the OpenID Provider.")]
		public event EventHandler<OpenIdEventArgs> LoggingIn;
		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		/// <returns>
		/// Returns whether the login should proceed.  False if some event handler canceled the request.
		/// </returns>
		protected virtual bool OnLoggingIn()
		{
			EventHandler<OpenIdEventArgs> loggingIn = LoggingIn;
			if (Request == null)
				CreateRequest();
			if (Request != null) {
				OpenIdEventArgs args = new OpenIdEventArgs(Request);
				if (loggingIn != null)
					loggingIn(this, args);
				return !args.Cancel;
			} else
				return false;
		}

		/// <summary>
		/// Fired when the Remember Me checkbox is changed by the user.
		/// </summary>
		[Description("Fires when the Remember Me checkbox is changed by the user.")]
		public event EventHandler RememberMeChanged;
		/// <summary>
		/// Fires the <see cref="RememberMeChanged"/> event.
		/// </summary>
		protected virtual void OnRememberMeChanged() {
			EventHandler rememberMeChanged = RememberMeChanged;
			if (rememberMeChanged != null)
				rememberMeChanged(this, new EventArgs());
		}
		#endregion
	}
}
