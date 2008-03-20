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

namespace DotNetOpenId.RelyingParty
{
	[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
	[DefaultProperty("OpenIdUrl")]
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

		const short textBoxTabIndexOffset = 0;
		const short loginButtonTabIndexOffset = 1;
		const short rememberMeTabIndexOffset = 2;
		const short registerTabIndexOffset = 3;

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
		}

		void identifierFormatValidator_ServerValidate(object source, ServerValidateEventArgs args) {
			args.IsValid = Identifier.IsValid(args.Value);
		}

		void rememberMeCheckBox_CheckedChanged(object sender, EventArgs e) {
			RememberMe = rememberMeCheckBox.Checked;
			OnRememberMeChanged();
		}

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			if (!this.DesignMode)
				label.Attributes["for"] = WrappedTextBox.ClientID;

			base.RenderChildren(writer);
		}

		#region Properties
		const string labelTextDefault = "OpenID Login:";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(labelTextDefault)]
		[Localizable(true)]
		public string LabelText
		{
			get { return label.InnerText; }
			set { label.InnerText = value; }
		}

		const string examplePrefixDefault = "Example:";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(examplePrefixDefault)]
		[Localizable(true)]
		public string ExamplePrefix
		{
			get { return examplePrefixLabel.Text; }
			set { examplePrefixLabel.Text = value; }
		}

		const string exampleUrlDefault = "http://your.name.myopenid.com";
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(exampleUrlDefault)]
		[Localizable(true)]
		public string ExampleUrl
		{
			get { return exampleUrlLabel.Text; }
			set { exampleUrlLabel.Text = value; }
		}

		const string requiredTextSuffix = "<br/>";
		const string requiredTextDefault = "Provide an OpenID first.";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(requiredTextDefault)]
		[Localizable(true)]
		public string RequiredText
		{
			get { return requiredValidator.Text.Substring(0, requiredValidator.Text.Length - requiredTextSuffix.Length); }
			set { requiredValidator.ErrorMessage = requiredValidator.Text = value + requiredTextSuffix; }
		}

		const string uriFormatTextDefault = "Invalid OpenID URL.";
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(uriFormatTextDefault)]
		[Localizable(true)]
		public string UriFormatText
		{
			get { return identifierFormatValidator.Text.Substring(0, identifierFormatValidator.Text.Length - requiredTextSuffix.Length); }
			set { identifierFormatValidator.ErrorMessage = identifierFormatValidator.Text = value + requiredTextSuffix; }
		}

		const bool uriValidatorEnabledDefault = true;
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(uriValidatorEnabledDefault)]
		public bool UriValidatorEnabled
		{
			get { return identifierFormatValidator.Enabled; }
			set { identifierFormatValidator.Enabled = value; }
		}

		const string registerTextDefault = "register";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerTextDefault)]
		[Localizable(true)]
		public string RegisterText
		{
			get { return registerLink.Text; }
			set { registerLink.Text = value; }
		}

		const string registerUrlDefault = "https://www.myopenid.com/signup";
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerUrlDefault)]
		[Localizable(true)]
		public string RegisterUrl
		{
			get { return registerLink.NavigateUrl; }
			set { registerLink.NavigateUrl = value; }
		}

		const string registerToolTipDefault = "Sign up free for an OpenID with MyOpenID now.";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerToolTipDefault)]
		[Localizable(true)]
		public string RegisterToolTip
		{
			get { return registerLink.ToolTip; }
			set { registerLink.ToolTip = value; }
		}

		const bool registerVisibleDefault = true;
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(registerVisibleDefault)]
		public bool RegisterVisible {
			get { return registerLink.Visible; }
			set { registerLink.Visible = value; }
		}

		const string buttonTextDefault = "Login »";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(buttonTextDefault)]
		[Localizable(true)]
		public string ButtonText
		{
			get { return loginButton.Text; }
			set { loginButton.Text = value; }
		}

		const string rememberMeTextDefault = "Remember me";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(rememberMeTextDefault)]
		[Localizable(true)]
		public string RememberMeText {
			get { return rememberMeCheckBox.Text; }
			set { rememberMeCheckBox.Text = value; }
		}

		const bool rememberMeVisibleDefault = false;
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(rememberMeVisibleDefault)]
		public bool RememberMeVisible {
			get { return rememberMeCheckBox.Visible; }
			set { rememberMeCheckBox.Visible = value; }
		}

		const bool rememberMeDefault = UsePersistentCookieDefault;
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(UsePersistentCookieDefault)]
		public bool RememberMe {
			get { return UsePersistentCookie; }
			set { UsePersistentCookie = value; }
		}

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
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(buttonToolTipDefault)]
		[Localizable(true)]
		public string ButtonToolTip
		{
			get { return loginButton.ToolTip; }
			set { loginButton.ToolTip = value; }
		}

		const string validationGroupDefault = "OpenIdLogin";
		[Category("Behavior")]
		[DefaultValue(validationGroupDefault)]
		public string ValidationGroup
		{
			get { return requiredValidator.ValidationGroup; }
			set
			{
				requiredValidator.ValidationGroup = value;
				loginButton.ValidationGroup = value;
			}
		}
		#endregion

		#region Properties to hide
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
			if (OnLoggingIn(Text))
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
		public event EventHandler<OpenIdTextBox.OpenIdEventArgs> LoggingIn;
		protected virtual bool OnLoggingIn(Identifier userSuppliedIdentifier)
		{
			EventHandler<OpenIdTextBox.OpenIdEventArgs> loggingIn = LoggingIn;
			// TODO: discover the true identityUrl from the openIdUrl given to
			//       fill OpenIdEventArgs with before firing this event.
			OpenIdTextBox.OpenIdEventArgs args = new OpenIdTextBox.OpenIdEventArgs(userSuppliedIdentifier);
			if (loggingIn != null)
				loggingIn(this, args);
			return !args.Cancel;
		}

		[Description("Fires when the Remember Me checkbox is changed by the user.")]
		public event EventHandler RememberMeChanged;
		protected virtual void OnRememberMeChanged() {
			EventHandler rememberMeChanged = RememberMeChanged;
			if (rememberMeChanged != null)
				rememberMeChanged(this, new EventArgs());
		}
		#endregion
	}
}
