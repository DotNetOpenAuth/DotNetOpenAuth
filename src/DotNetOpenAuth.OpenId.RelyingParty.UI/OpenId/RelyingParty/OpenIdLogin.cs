//-----------------------------------------------------------------------
// <copyright file="OpenIdLogin.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;

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
		private const bool RememberMeDefault = false;

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
		/// Fired when the Remember Me checkbox is changed by the user.
		/// </summary>
		[Description("Fires when the Remember Me checkbox is changed by the user.")]
		public event EventHandler RememberMeChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a <see cref="T:System.Web.UI.ControlCollection"/> object that represents the child controls for a specified server control in the UI hierarchy.
		/// </summary>
		/// <returns>
		/// The collection of child controls for the specified server control.
		/// </returns>
		public override ControlCollection Controls {
			get {
				this.EnsureChildControls();
				return base.Controls;
			}
		}

		/// <summary>
		/// Gets or sets the caption that appears before the text box.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(LabelTextDefault)]
		[Localizable(true)]
		[Description("The caption that appears before the text box.")]
		public string LabelText {
			get {
				EnsureChildControls();
				return this.label.InnerText;
			}

			set {
				EnsureChildControls();
				this.label.InnerText = value;
			}
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
			get {
				EnsureChildControls();
				return this.examplePrefixLabel.Text;
			}

			set {
				EnsureChildControls();
				this.examplePrefixLabel.Text = value;
			}
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
			get {
				EnsureChildControls();
				return this.exampleUrlLabel.Text;
			}

			set {
				EnsureChildControls();
				this.exampleUrlLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the text to display if the user attempts to login 
		/// without providing an Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "br", Justification = "HTML"), Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RequiredTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user attempts to login without providing an Identifier.")]
		public string RequiredText {
			get {
				EnsureChildControls();
				return this.requiredValidator.Text.Substring(0, this.requiredValidator.Text.Length - RequiredTextSuffix.Length);
			}

			set {
				EnsureChildControls();
				this.requiredValidator.ErrorMessage = this.requiredValidator.Text = value + RequiredTextSuffix;
			}
		}

		/// <summary>
		/// Gets or sets the text to display if the user provides an invalid form for an Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "br", Justification = "HTML"), SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid only supports primitive types.")]
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(UriFormatTextDefault)]
		[Localizable(true)]
		[Description("The text to display if the user provides an invalid form for an Identifier.")]
		public string UriFormatText {
			get {
				EnsureChildControls();
				return this.identifierFormatValidator.Text.Substring(0, this.identifierFormatValidator.Text.Length - RequiredTextSuffix.Length);
			}

			set {
				EnsureChildControls();
				this.identifierFormatValidator.ErrorMessage = this.identifierFormatValidator.Text = value + RequiredTextSuffix;
			}
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
			get {
				EnsureChildControls();
				return this.identifierFormatValidator.Enabled;
			}

			set {
				EnsureChildControls();
				this.identifierFormatValidator.Enabled = value;
			}
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
			get {
				EnsureChildControls();
				return this.registerLink.Text;
			}

			set {
				EnsureChildControls();
				this.registerLink.Text = value;
			}
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
			get {
				EnsureChildControls();
				return this.registerLink.NavigateUrl;
			}

			set {
				EnsureChildControls();
				this.registerLink.NavigateUrl = value;
			}
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
			get {
				EnsureChildControls();
				return this.registerLink.ToolTip;
			}

			set {
				EnsureChildControls();
				this.registerLink.ToolTip = value;
			}
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
			get {
				EnsureChildControls();
				return this.registerLink.Visible;
			}

			set {
				EnsureChildControls();
				this.registerLink.Visible = value;
			}
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
			get {
				EnsureChildControls();
				return this.loginButton.Text;
			}

			set {
				EnsureChildControls();
				this.loginButton.Text = value;
			}
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
			get {
				EnsureChildControls();
				return this.rememberMeCheckBox.Text;
			}

			set {
				EnsureChildControls();
				this.rememberMeCheckBox.Text = value;
			}
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
			get {
				EnsureChildControls();
				return this.rememberMeCheckBox.Visible;
			}

			set {
				EnsureChildControls();
				this.rememberMeCheckBox.Visible = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a successful authentication should result in a persistent
		/// cookie being saved to the browser.
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(RememberMeDefault)]
		[Description("Whether a successful authentication should result in a persistent cookie being saved to the browser.")]
		public bool RememberMe {
			get { return this.UsePersistentCookie != LogOnPersistence.Session; }
			set { this.UsePersistentCookie = value ? LogOnPersistence.PersistentAuthentication : LogOnPersistence.Session; }
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
					EnsureChildControls();
					base.TabIndex = (short)(value + TextBoxTabIndexOffset);
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
			get {
				EnsureChildControls();
				return this.loginButton.ToolTip;
			}

			set {
				EnsureChildControls();
				this.loginButton.ToolTip = value;
			}
		}

		/// <summary>
		/// Gets or sets the validation group that the login button and text box validator belong to.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(ValidationGroupDefault)]
		[Description("The validation group that the login button and text box validator belong to.")]
		public string ValidationGroup {
			get {
				EnsureChildControls();
				return this.requiredValidator.ValidationGroup;
			}

			set {
				EnsureChildControls();
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
			get { return (string)ViewState[IdSelectorIdentifierViewStateKey]; }
			set { ViewState[IdSelectorIdentifierViewStateKey] = value; }
		}

		#endregion

		#region Properties to hide

		/// <summary>
		/// Gets or sets a value indicating whether a FormsAuthentication 
		/// cookie should persist across user sessions.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override LogOnPersistence UsePersistentCookie {
			get {
				return base.UsePersistentCookie;
			}

			set {
				base.UsePersistentCookie = value;

				if (this.rememberMeCheckBox != null) {
					// use conditional here to prevent infinite recursion
					// with CheckedChanged event.
					bool rememberMe = value != LogOnPersistence.Session;
					if (this.rememberMeCheckBox.Checked != rememberMe) {
						this.rememberMeCheckBox.Checked = rememberMe;
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object and stores tracing information about the control if tracing is enabled.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HTmlTextWriter"/> object that receives the control content.</param>
		public override void RenderControl(HtmlTextWriter writer) {
			this.RenderChildren(writer);
		}

		/// <summary>
		/// Creates the child controls.
		/// </summary>
		protected override void CreateChildControls() {
			this.InitializeControls();

			// Just add the panel we've assembled earlier.
			base.Controls.Add(this.panel);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.EnsureChildControls();
		}

		/// <summary>
		/// Initializes the child controls.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.WebControl.set_ToolTip(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.Label.set_Text(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.HyperLink.set_Text(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.CheckBox.set_Text(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.Button.set_Text(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.BaseValidator.set_ErrorMessage(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "br", Justification = "HTML"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpenID", Justification = "It is correct"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MyOpenID", Justification = "Correct spelling"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "myopenid", Justification = "URL")]
		protected void InitializeControls() {
			this.panel = new Panel();

			Table table = new Table();
			try {
				TableRow row1, row2, row3;
				TableCell cell;
				table.Rows.Add(row1 = new TableRow());
				table.Rows.Add(row2 = new TableRow());
				table.Rows.Add(row3 = new TableRow());

				// top row, left cell
				cell = new TableCell();
				try {
					this.label = new HtmlGenericControl("label");
					this.label.InnerText = LabelTextDefault;
					cell.Controls.Add(this.label);
					row1.Cells.Add(cell);
				} catch {
					cell.Dispose();
					throw;
				}

				// top row, middle cell
				cell = new TableCell();
				try {
					cell.Controls.Add(new InPlaceControl(this));
					row1.Cells.Add(cell);
				} catch {
					cell.Dispose();
					throw;
				}

				// top row, right cell
				cell = new TableCell();
				try {
					this.loginButton = new Button();
					this.loginButton.ID = this.ID + "_loginButton";
					this.loginButton.Text = ButtonTextDefault;
					this.loginButton.ToolTip = ButtonToolTipDefault;
					this.loginButton.Click += this.LoginButton_Click;
					this.loginButton.ValidationGroup = ValidationGroupDefault;
#if !Mono
					this.panel.DefaultButton = this.loginButton.ID;
#endif
					cell.Controls.Add(this.loginButton);
					row1.Cells.Add(cell);
				} catch {
					cell.Dispose();
					throw;
				}

				// middle row, left cell
				row2.Cells.Add(new TableCell());

				// middle row, middle cell
				cell = new TableCell();
				try {
					cell.Style[HtmlTextWriterStyle.Color] = "gray";
					cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
					this.requiredValidator = new RequiredFieldValidator();
					this.requiredValidator.ErrorMessage = RequiredTextDefault + RequiredTextSuffix;
					this.requiredValidator.Text = RequiredTextDefault + RequiredTextSuffix;
					this.requiredValidator.Display = ValidatorDisplay.Dynamic;
					this.requiredValidator.ValidationGroup = ValidationGroupDefault;
					cell.Controls.Add(this.requiredValidator);
					this.identifierFormatValidator = new CustomValidator();
					this.identifierFormatValidator.ErrorMessage = UriFormatTextDefault + RequiredTextSuffix;
					this.identifierFormatValidator.Text = UriFormatTextDefault + RequiredTextSuffix;
					this.identifierFormatValidator.ServerValidate += this.IdentifierFormatValidator_ServerValidate;
					this.identifierFormatValidator.Enabled = UriValidatorEnabledDefault;
					this.identifierFormatValidator.Display = ValidatorDisplay.Dynamic;
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
				} catch {
					cell.Dispose();
					throw;
				}

				// middle row, right cell
				cell = new TableCell();
				try {
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
				} catch {
					cell.Dispose();
					throw;
				}

				// bottom row, left cell
				cell = new TableCell();
				row3.Cells.Add(cell);

				// bottom row, middle cell
				cell = new TableCell();
				try {
					this.rememberMeCheckBox = new CheckBox();
					this.rememberMeCheckBox.Text = RememberMeTextDefault;
					this.rememberMeCheckBox.Checked = this.UsePersistentCookie != LogOnPersistence.Session;
					this.rememberMeCheckBox.Visible = RememberMeVisibleDefault;
					this.rememberMeCheckBox.CheckedChanged += this.RememberMeCheckBox_CheckedChanged;
					cell.Controls.Add(this.rememberMeCheckBox);
					row3.Cells.Add(cell);
				} catch {
					cell.Dispose();
					throw;
				}

				// bottom row, right cell
				cell = new TableCell();
				try {
					row3.Cells.Add(cell);
				} catch {
					cell.Dispose();
					throw;
				}

				// this sets all the controls' tab indexes
				this.TabIndex = TabIndexDefault;

				this.panel.Controls.Add(table);
			} catch {
				table.Dispose();
				throw;
			}

			this.idselectorJavascript = new Literal();
			this.panel.Controls.Add(this.idselectorJavascript);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnInit(EventArgs e) {
			this.SetChildControlReferenceIds();

			base.OnInit(e);
		}

		/// <summary>
		/// Renders the child controls.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the rendered content.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.Literal.set_Text(System.String)", Justification = "By design"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "idselector", Justification = "HTML"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "charset", Justification = "html"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "src", Justification = "html"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "openidselector", Justification = "html"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "idselectorinputid", Justification = "html")]
		protected override void RenderChildren(HtmlTextWriter writer) {
			if (!this.DesignMode) {
				this.label.Attributes["for"] = this.ClientID;

				if (!string.IsNullOrEmpty(this.IdSelectorIdentifier)) {
					this.idselectorJavascript.Visible = true;
					this.idselectorJavascript.Text = @"<script type='text/javascript'><!--
idselector_input_id = '" + this.ClientID + @"';
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
				this.errorLabel.Text = string.Format(CultureInfo.CurrentCulture, this.FailedMessageText, response.Exception.ToStringDescriptive());
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
			this.Page.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!this.Page.IsValid) {
							return;
						}

						var authenticationRequests = await this.CreateRequestsAsync(CancellationToken.None);
						IAuthenticationRequest request = authenticationRequests.FirstOrDefault();
						if (request != null) {
							await this.LogOnAsync(request, CancellationToken.None);
						} else {
							if (!string.IsNullOrEmpty(this.FailedMessageText)) {
								this.errorLabel.Text = string.Format(
									CultureInfo.CurrentCulture, this.FailedMessageText, OpenIdStrings.OpenIdEndpointNotFound);
								this.errorLabel.Visible = true;
							}
						}
					}));
		}

		/// <summary>
		/// Renders the control inner.
		/// </summary>
		/// <param name="writer">The writer.</param>
		private void RenderControlInner(HtmlTextWriter writer) {
			base.RenderControl(writer);
		}

		/// <summary>
		/// Sets child control properties that depend on this control's ID.
		/// </summary>
		private void SetChildControlReferenceIds() {
			this.EnsureChildControls();
			this.EnsureID();
			ErrorUtilities.VerifyInternal(!string.IsNullOrEmpty(this.ID), "No control ID available yet!");
			this.requiredValidator.ControlToValidate = this.ID;
			this.requiredValidator.ID = this.ID + "_requiredValidator";
			this.identifierFormatValidator.ControlToValidate = this.ID;
			this.identifierFormatValidator.ID = this.ID + "_identifierFormatValidator";
		}

		/// <summary>
		/// A control that acts as a placeholder to indicate where
		/// the OpenIdLogin control should render its OpenIdTextBox parent.
		/// </summary>
		private class InPlaceControl : PlaceHolder {
			/// <summary>
			/// The owning control to render.
			/// </summary>
			private OpenIdLogin renderControl;

			/// <summary>
			/// Initializes a new instance of the <see cref="InPlaceControl"/> class.
			/// </summary>
			/// <param name="renderControl">The render control.</param>
			internal InPlaceControl(OpenIdLogin renderControl) {
				this.renderControl = renderControl;
			}

			/// <summary>
			/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
			/// </summary>
			/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
			protected override void Render(HtmlTextWriter writer) {
				this.renderControl.RenderControlInner(writer);
			}
		}
	}
}
