/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://jmpinline.nerdbank.net/
 ********************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Net.Mail;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Janrain.OpenId;
using Janrain.OpenId.Consumer;
using Janrain.OpenId.Session;
using Janrain.OpenId.RegistrationExtension;
using Janrain.OpenId.Store;

namespace NerdBank.OpenId.RegistrationExtension
{
}

namespace Janrain.OpenId.RegistrationExtension
{
}

namespace NerdBank.OpenId.Consumer
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:OpenIdTextBox runat=\"server\"></{0}:OpenIdTextBox>")]
	public class OpenIdTextBox : CompositeControl
	{
		public OpenIdTextBox()
		{
			InitializeControls();
		}

		TextBox wrappedTextBox;
		protected TextBox WrappedTextBox
		{
			get { return wrappedTextBox; }
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Controls.Add(wrappedTextBox);
			if (ShouldBeFocused)
				WrappedTextBox.Focus();
		}

		protected virtual void InitializeControls()
		{
			wrappedTextBox = new TextBox();
			wrappedTextBox.ID = "wrappedTextBox";
			wrappedTextBox.CssClass = cssClassDefault;
			wrappedTextBox.Columns = columnsDefault;
			wrappedTextBox.Text = text;
		}

		protected bool ShouldBeFocused;
		public override void Focus()
		{
			if (Controls.Count == 0)
				ShouldBeFocused = true;
			else
				WrappedTextBox.Focus();
		}

		#region Properties
		const string textDefault = "";
		string text = textDefault;
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		public string Text
		{
			get { return (WrappedTextBox != null) ? WrappedTextBox.Text : text; }
			set
			{
				text = value;
				if (WrappedTextBox != null) WrappedTextBox.Text = value;
			}
		}

		const string cssClassDefault = "openid";
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(cssClassDefault)]
		public override string CssClass
		{
			get { return WrappedTextBox.CssClass; }
			set { WrappedTextBox.CssClass = value; }
		}

		const int columnsDefault = 40;
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(columnsDefault)]
		public int Columns
		{
			get { return WrappedTextBox.Columns; }
			set { WrappedTextBox.Columns = value; }
		}

		const string requestNicknameViewStateKey = "RequestNickname";
		const ProfileRequest requestNicknameDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestNicknameDefault)]
		public ProfileRequest RequestNickname
		{
			get { return (ProfileRequest)(ViewState[requestNicknameViewStateKey] ?? requestNicknameDefault); }
			set { ViewState[requestNicknameViewStateKey] = value; }
		}

		const string requestEmailViewStateKey = "RequestEmail";
		const ProfileRequest requestEmailDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestEmailDefault)]
		public ProfileRequest RequestEmail
		{
			get { return (ProfileRequest)(ViewState[requestEmailViewStateKey] ?? requestEmailDefault); }
			set { ViewState[requestEmailViewStateKey] = value; }
		}

		const string requestFullNameViewStateKey = "RequestFullName";
		const ProfileRequest requestFullNameDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestFullNameDefault)]
		public ProfileRequest RequestFullName
		{
			get { return (ProfileRequest)(ViewState[requestFullNameViewStateKey] ?? requestFullNameDefault); }
			set { ViewState[requestFullNameViewStateKey] = value; }
		}

		const string requestBirthdateViewStateKey = "RequestBirthday";
		const ProfileRequest requestBirthdateDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestBirthdateDefault)]
		public ProfileRequest RequestBirthdate
		{
			get { return (ProfileRequest)(ViewState[requestBirthdateViewStateKey] ?? requestBirthdateDefault); }
			set { ViewState[requestBirthdateViewStateKey] = value; }
		}

		const string requestGenderViewStateKey = "RequestGender";
		const ProfileRequest requestGenderDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestGenderDefault)]
		public ProfileRequest RequestGender
		{
			get { return (ProfileRequest)(ViewState[requestGenderViewStateKey] ?? requestGenderDefault); }
			set { ViewState[requestGenderViewStateKey] = value; }
		}

		const string requestPostalCodeViewStateKey = "RequestPostalCode";
		const ProfileRequest requestPostalCodeDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestPostalCodeDefault)]
		public ProfileRequest RequestPostalCode
		{
			get { return (ProfileRequest)(ViewState[requestPostalCodeViewStateKey] ?? requestPostalCodeDefault); }
			set { ViewState[requestPostalCodeViewStateKey] = value; }
		}

		const string requestCountryViewStateKey = "RequestCountry";
		const ProfileRequest requestCountryDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestCountryDefault)]
		public ProfileRequest RequestCountry
		{
			get { return (ProfileRequest)(ViewState[requestCountryViewStateKey] ?? requestCountryDefault); }
			set { ViewState[requestCountryViewStateKey] = value; }
		}

		const string requestLanguageViewStateKey = "RequestLanguage";
		const ProfileRequest requestLanguageDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestLanguageDefault)]
		public ProfileRequest RequestLanguage
		{
			get { return (ProfileRequest)(ViewState[requestLanguageViewStateKey] ?? requestLanguageDefault); }
			set { ViewState[requestLanguageViewStateKey] = value; }
		}

		const string requestTimeZoneViewStateKey = "RequestTimeZone";
		const ProfileRequest requestTimeZoneDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(requestTimeZoneDefault)]
		public ProfileRequest RequestTimeZone
		{
			get { return (ProfileRequest)(ViewState[requestTimeZoneViewStateKey] ?? requestTimeZoneDefault); }
			set { ViewState[requestTimeZoneViewStateKey] = value; }
		}

		const string policyUrlViewStateKey = "PolicyUrl";
		const string policyUrlDefault = "";
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(policyUrlDefault)]
		public string PolicyUrl
		{
			get { return (string)ViewState[policyUrlViewStateKey] ?? policyUrlDefault; }
			set { ViewState[policyUrlViewStateKey] = value; }
		}

		const string enableRequestProfileViewStateKey = "EnableRequestProfile";
		const bool enableRequestProfileDefault = true;
		[Bindable(true)]
		[Category("Profile")]
		[DefaultValue(enableRequestProfileDefault)]
		public bool EnableRequestProfile
		{
			get { return (bool)(ViewState[enableRequestProfileViewStateKey] ?? enableRequestProfileDefault); }
			set { ViewState[enableRequestProfileViewStateKey] = value; }
		}
		#endregion

		#region Properties to hide
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle
		{
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override FontInfo Font
		{
			get { return null; }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Height
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Width
		{
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string ToolTip
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string SkinID
		{
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming
		{
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				if (!Page.IsPostBack && Page.Request.QueryString["openid.mode"] != null)
				{
					Janrain.OpenId.Consumer.Consumer consumer =
						new Janrain.OpenId.Consumer.Consumer(new SystemHttpSessionState(Page.Session), MemoryStore.GetInstance());

					ConsumerResponse resp = consumer.Complete(Page.Request.QueryString);
					OnLoggedIn(resp.IdentityUrl, parseProfileFields(Page.Request.QueryString));
				}
			}
			catch (FailureException fexc)
			{
				OnError(fexc);
			}
			catch (CancelException cex)
			{
				OnError(cex);
			}
		}

		public void Login()
		{
			if (string.IsNullOrEmpty(Text))
				throw new InvalidOperationException(Janrain.OpenId.Strings.OpenIdTextBoxEmpty);

			Janrain.OpenId.Consumer.Consumer consumer =
				new Janrain.OpenId.Consumer.Consumer(new SystemHttpSessionState(Page.Session), MemoryStore.GetInstance());

			Uri userUri = UriUtil.NormalizeUri(Text);
			// Initiate openid request
			AuthRequest request = consumer.Begin(userUri);
			if (EnableRequestProfile) addProfileArgs(request);

			// Build the trust root
			UriBuilder builder = new UriBuilder(Page.Request.Url.AbsoluteUri);
			builder.Query = null;
			builder.Password = null;
			builder.UserName = null;
			builder.Fragment = null;
			builder.Path = Page.Request.ApplicationPath;
			string trustRoot = builder.Uri.ToString();

			// Build the return_to URL
			builder = new UriBuilder(Page.Request.Url.AbsoluteUri);
			if (!string.IsNullOrEmpty(Page.Request.QueryString["ReturnUrl"]))
			{
				builder.Query = "ReturnUrl=" + Page.Request.QueryString["ReturnUrl"];
			}
			Uri returnTo = new Uri(builder.ToString());

			Uri redirectUrl = request.CreateRedirect(trustRoot, returnTo, AuthRequest.Mode.SETUP);
			Page.Response.Redirect(redirectUrl.AbsoluteUri);
		}

		void addProfileArgs(AuthRequest request)
		{
			request.ExtraArgs.Add("openid.sreg.required", string.Join(",", assembleProfileFields(ProfileRequest.Require)));
			request.ExtraArgs.Add("openid.sreg.optional", string.Join(",", assembleProfileFields(ProfileRequest.Request)));
			request.ExtraArgs.Add("openid.sreg.policy_url", PolicyUrl);
		}

		string[] assembleProfileFields(ProfileRequest level)
		{
			List<string> fields = new List<string>(10);
			if (RequestNickname == level)
				fields.Add("nickname");
			if (RequestEmail == level)
				fields.Add("email");
			if (RequestFullName == level)
				fields.Add("fullname");
			if (RequestBirthdate == level)
				fields.Add("dob");
			if (RequestGender == level)
				fields.Add("gender");
			if (RequestPostalCode == level)
				fields.Add("postcode");
			if (RequestCountry == level)
				fields.Add("country");
			if (RequestLanguage == level)
				fields.Add("language");
			if (RequestTimeZone == level)
				fields.Add("timezone");

			return fields.ToArray();
		}
		OpenIdProfileFields parseProfileFields(NameValueCollection queryString)
		{
			OpenIdProfileFields fields = new OpenIdProfileFields();
			if (RequestNickname > ProfileRequest.NoRequest)
				fields.Nickname = queryString["openid.sreg.nickname"];
			if (RequestEmail > ProfileRequest.NoRequest && !string.IsNullOrEmpty(queryString["openid.sreg.email"]))
				fields.Email = new MailAddress(queryString["openid.sreg.email"]);
			if (RequestFullName > ProfileRequest.NoRequest)
				fields.Fullname = queryString["openid.sreg.fullname"];
			if (RequestBirthdate > ProfileRequest.NoRequest && !string.IsNullOrEmpty(queryString["openid.sreg.dob"]))
			{
				DateTime birthdate;
				DateTime.TryParse(queryString["openid.sreg.dob"], out birthdate);
				fields.Birthdate = birthdate;
			}
			if (RequestGender > ProfileRequest.NoRequest)
				switch (queryString["openid.sreg.gender"])
				{
					case "M": fields.Gender = Gender.Male; break;
					case "F": fields.Gender = Gender.Female; break;
				}
			if (RequestPostalCode > ProfileRequest.NoRequest)
				fields.PostalCode = queryString["openid.sreg.postcode"];
			if (RequestCountry > ProfileRequest.NoRequest)
				fields.Country = queryString["openid.sreg.country"];
			if (RequestLanguage > ProfileRequest.NoRequest)
				fields.Language = queryString["openid.sreg.language"];
			if (RequestTimeZone > ProfileRequest.NoRequest)
				fields.TimeZone = queryString["openid.sreg.timezone"];
			return fields;
		}

		#region Events
		public class OpenIdEventArgs : EventArgs
		{
			public OpenIdEventArgs(Uri openIdUri, OpenIdProfileFields profileFields)
			{
				this.openIdUri = openIdUri;
				this.profileFields = profileFields;
			}
			private Uri openIdUri;
			/// <summary>
			/// The OpenID url of the authenticating user.
			/// </summary>
			public Uri OpenIdUri
			{
				get { return openIdUri; }
			}
			private bool cancel;
			/// <summary>
			/// Cancels the OpenID authentication and/or login process.
			/// </summary>
			public bool Cancel
			{
				get { return cancel; }
				set { cancel = value; }
			}

			private OpenIdProfileFields profileFields;
			public OpenIdProfileFields ProfileFields
			{
				get { return profileFields; }
			}
		}
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		protected virtual void OnLoggedIn(Uri openIdUri, OpenIdProfileFields profileFields)
		{
			EventHandler<OpenIdEventArgs> loggedIn = LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(openIdUri, profileFields);
			if (loggedIn != null)
				loggedIn(this, args);
			if (!args.Cancel)
				FormsAuthentication.RedirectFromLoginPage(openIdUri.AbsoluteUri, false);
		}

		#endregion
		#region Error handling
		public class ErrorEventArgs : EventArgs
		{
			public ErrorEventArgs(string errorMessage, Exception errorException)
			{
				ErrorMessage = errorMessage;
				ErrorException = errorException;
			}
			public string ErrorMessage;
			public Exception ErrorException;
		}
		public event EventHandler<ErrorEventArgs> Error;
		protected virtual void OnError(Exception errorException)
		{
			if (errorException == null)
				throw new ArgumentNullException("errorException");

			EventHandler<ErrorEventArgs> error = Error;
			if (error != null)
				error(this, new ErrorEventArgs(errorException.Message, errorException));
		}
		#endregion
	}
}
