/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://jmpinline.nerdbank.net/
 ********************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using Janrain.OpenId.RegistrationExtension;
using Janrain.OpenId;

namespace NerdBank.OpenId.Consumer
{
	[DefaultProperty("OpenIdUrl")]
	[ToolboxData("<{0}:OpenIdLogin runat=\"server\"></{0}:OpenIdLogin>")]
	public class OpenIdLogin : OpenIdTextBox
	{
		// Regex comes from http://mjtsai.com/blog/2003/08/18/url_regex_generator/
		// It's (allegedly) fully RFC-compliant.
		// I've modified it to allow HTTPS, and to allow no protocol at all,
		// in which case UriUtil.NormalizeUri(string) will add 'http://' automatically.
		// The short version that the RegularExpressionValidator control 
		// prescribes doesn't allow "localhost" or IP addresses as hosts.
		internal const string UriRegex = @"(?:(?:https?://)?(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*))*)(?:\?(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*))?)?)|(?:ftp://(?:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*)(?::(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*))?@)?(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?))(?:/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*))*)(?:;type=[AIDaid])?)?)|(?:news:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;/?:&=])+@(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3})))|(?:[a-zA-Z](?:[a-zA-Z\d]|[_.+-])*)|\*))|(?:nntp://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)/(?:[a-zA-Z](?:[a-zA-Z\d]|[_.+-])*)(?:/(?:\d+))?)|(?:telnet://(?:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*)(?::(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*))?@)?(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?))/?)|(?:gopher://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:/(?:[a-zA-Z\d$\-_.+!*'(),;/?:@&=]|(?:%[a-fA-F\d]{2}))(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),;/?:@&=]|(?:%[a-fA-F\d]{2}))*)(?:%09(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*)(?:%09(?:(?:[a-zA-Z\d$\-_.+!*'(),;/?:@&=]|(?:%[a-fA-F\d]{2}))*))?)?)?)?)|(?:wais://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)/(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*)(?:(?:/(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*)/(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*))|\?(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*))?)|(?:mailto:(?:(?:[a-zA-Z\d$\-_.+!*'(),;/?:@&=]|(?:%[a-fA-F\d]{2}))+))|(?:file://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))|localhost)?/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*))*))|(?:prospero://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*))*)(?:(?:;(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&])*)=(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&])*)))*)|(?:ldap://(?:(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?))?/(?:(?:(?:(?:(?:(?:(?:[a-zA-Z\d]|%(?:3\d|[46][a-fA-F\d]|[57][Aa\d]))|(?:%20))+|(?:OID|oid)\.(?:(?:\d+)(?:\.(?:\d+))*))(?:(?:%0[Aa])?(?:%20)*)=(?:(?:%0[Aa])?(?:%20)*))?(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*))(?:(?:(?:%0[Aa])?(?:%20)*)\+(?:(?:%0[Aa])?(?:%20)*)(?:(?:(?:(?:(?:[a-zA-Z\d]|%(?:3\d|[46][a-fA-F\d]|[57][Aa\d]))|(?:%20))+|(?:OID|oid)\.(?:(?:\d+)(?:\.(?:\d+))*))(?:(?:%0[Aa])?(?:%20)*)=(?:(?:%0[Aa])?(?:%20)*))?(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*)))*)(?:(?:(?:(?:%0[Aa])?(?:%20)*)(?:[;,])(?:(?:%0[Aa])?(?:%20)*))(?:(?:(?:(?:(?:(?:[a-zA-Z\d]|%(?:3\d|[46][a-fA-F\d]|[57][Aa\d]))|(?:%20))+|(?:OID|oid)\.(?:(?:\d+)(?:\.(?:\d+))*))(?:(?:%0[Aa])?(?:%20)*)=(?:(?:%0[Aa])?(?:%20)*))?(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*))(?:(?:(?:%0[Aa])?(?:%20)*)\+(?:(?:%0[Aa])?(?:%20)*)(?:(?:(?:(?:(?:[a-zA-Z\d]|%(?:3\d|[46][a-fA-F\d]|[57][Aa\d]))|(?:%20))+|(?:OID|oid)\.(?:(?:\d+)(?:\.(?:\d+))*))(?:(?:%0[Aa])?(?:%20)*)=(?:(?:%0[Aa])?(?:%20)*))?(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))*)))*))*(?:(?:(?:%0[Aa])?(?:%20)*)(?:[;,])(?:(?:%0[Aa])?(?:%20)*))?)(?:\?(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+)(?:,(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+))*)?)(?:\?(?:base|one|sub)(?:\?(?:((?:[a-zA-Z\d$\-_.+!*'(),;/?:@&=]|(?:%[a-fA-F\d]{2}))+)))?)?)?)|(?:(?:z39\.50[rs])://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+)(?:\+(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+))*(?:\?(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+))?)?(?:;esn=(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+))?(?:;rs=(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+)(?:\+(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))+))*)?))|(?:cid:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?:@&=])*))|(?:mid:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?:@&=])*))?)|(?:vemmi://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[/?:@&=])*)(?:(?:;(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[/?:@&])*)=(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[/?:@&])*))*))?)|(?:imap://(?:(?:(?:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~])+)(?:(?:;[Aa][Uu][Tt][Hh]=(?:\*|(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~])+))))?)|(?:(?:;[Aa][Uu][Tt][Hh]=(?:\*|(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~])+)))(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~])+))?))@)?(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?))/(?:(?:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~:@/])+)?;[Tt][Yy][Pp][Ee]=(?:[Ll](?:[Ii][Ss][Tt]|[Ss][Uu][Bb])))|(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~:@/])+)(?:\?(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~:@/])+))?(?:(?:;[Uu][Ii][Dd][Vv][Aa][Ll][Ii][Dd][Ii][Tt][Yy]=(?:[1-9]\d*)))?)|(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~:@/])+)(?:(?:;[Uu][Ii][Dd][Vv][Aa][Ll][Ii][Dd][Ii][Tt][Yy]=(?:[1-9]\d*)))?(?:/;[Uu][Ii][Dd]=(?:[1-9]\d*))(?:(?:/;[Ss][Ee][Cc][Tt][Ii][Oo][Nn]=(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[&=~:@/])+)))?)))?)|(?:nfs:(?:(?://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:(?:/(?:(?:(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*)(?:/(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*))*)?)))?)|(?:/(?:(?:(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*)(?:/(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*))*)?))|(?:(?:(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*)(?:/(?:(?:(?:[a-zA-Z\d\$\-_.!~*'(),])|(?:%[a-fA-F\d]{2})|[:@&=+])*))*)?)))";
		Panel panel;
		Button loginButton;
		HtmlGenericControl label;
		RequiredFieldValidator requiredValidator;
		RegularExpressionValidator uriFormatValidator;
		Label examplePrefixLabel;
		Label exampleUrlLabel;
		HyperLink registerLink;

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
			TableRow row1, row2;
			TableCell cell;
			table.Rows.Add(row1 = new TableRow());
			table.Rows.Add(row2 = new TableRow());

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

			// bottom row, left cell
			row2.Cells.Add(new TableCell());

			// bottom row, middle cell
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
			uriFormatValidator = new RegularExpressionValidator();
			uriFormatValidator.ErrorMessage = uriFormatTextDefault + requiredTextSuffix;
			uriFormatValidator.Text = uriFormatTextDefault + requiredTextSuffix;
			uriFormatValidator.ValidationExpression = UriRegex;
			uriFormatValidator.Enabled = uriValidatorEnabledDefault;
			uriFormatValidator.Display = ValidatorDisplay.Dynamic;
			uriFormatValidator.ControlToValidate = WrappedTextBox.ID;
			uriFormatValidator.ValidationGroup = validationGroupDefault;
			cell.Controls.Add(uriFormatValidator);
			examplePrefixLabel = new Label();
			examplePrefixLabel.Text = examplePrefixDefault;
			cell.Controls.Add(examplePrefixLabel);
			cell.Controls.Add(new LiteralControl(" "));
			exampleUrlLabel = new Label();
			exampleUrlLabel.Font.Bold = true;
			exampleUrlLabel.Text = exampleUrlDefault;
			cell.Controls.Add(exampleUrlLabel);
			row2.Cells.Add(cell);

			// bottom row, right cell
			cell = new TableCell();
			cell.Style[HtmlTextWriterStyle.Color] = "gray";
			cell.Style[HtmlTextWriterStyle.FontSize] = "smaller";
			cell.Style[HtmlTextWriterStyle.TextAlign] = "center";
			registerLink = new HyperLink();
			registerLink.Text = registerTextDefault;
			registerLink.ToolTip = registerToolTipDefault;
			registerLink.NavigateUrl = registerUrlDefault;
			cell.Controls.Add(registerLink);
			row2.Cells.Add(cell);

			panel.Controls.Add(table);
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
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(uriFormatTextDefault)]
		[Localizable(true)]
		public string UriFormatText
		{
			get { return uriFormatValidator.Text.Substring(0, uriFormatValidator.Text.Length - requiredTextSuffix.Length); }
			set { uriFormatValidator.ErrorMessage = uriFormatValidator.Text = value + requiredTextSuffix; }
		}

		const bool uriValidatorEnabledDefault = true;
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(uriValidatorEnabledDefault)]
		public bool UriValidatorEnabled
		{
			get { return uriFormatValidator.Enabled; }
			set { uriFormatValidator.Enabled = value; }
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

		#region Event handlers
		void loginButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid) return;
			if (OnLoggingIn(UriUtil.NormalizeUri(Text)))
				Login();
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
		protected virtual bool OnLoggingIn(Uri openIdUri)
		{
			EventHandler<OpenIdTextBox.OpenIdEventArgs> loggingIn = LoggingIn;
			OpenIdTextBox.OpenIdEventArgs args = new OpenIdTextBox.OpenIdEventArgs(openIdUri,
				OpenIdProfileFields.Empty);
			if (loggingIn != null)
				loggingIn(this, args);
			return !args.Cancel;
		}
		#endregion
	}
}
