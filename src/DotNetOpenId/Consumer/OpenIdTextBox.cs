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

using DotNetOpenId.Session;
using DotNetOpenId.RegistrationExtension;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

[assembly: WebResource(DotNetOpenId.Consumer.OpenIdTextBox.EmbeddedLogoResourceName, "image/gif")]

namespace DotNetOpenId.Consumer
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:OpenIdTextBox runat=\"server\"></{0}:OpenIdTextBox>")]
	public class OpenIdTextBox : CompositeControl
	{
		public OpenIdTextBox()
		{
			InitializeControls();
		}

		internal const string EmbeddedLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".Consumer.openid_login.gif";
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
			wrappedTextBox.TabIndex = tabIndexDefault;
		}

		protected bool ShouldBeFocused;
		public override void Focus()
		{
			if (Controls.Count == 0)
				ShouldBeFocused = true;
			else
				WrappedTextBox.Focus();
		}

		const string appearanceCategory = "Appearance";
		const string profileCategory = "Profile";
		const string behaviorCategory = "Behavior";

		#region Properties
		const string textDefault = "";
		string text = textDefault;
		[Bindable(true)]
		[Category(appearanceCategory)]
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

		const string trustRootUrlViewStateKey = "TrustRootUrl";
		const string trustRootUrlDefault = "~/";
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(trustRootUrlDefault)]
		public string TrustRootUrl
		{
			get { return (string)(ViewState[trustRootUrlViewStateKey] ?? trustRootUrlDefault); }
			set
			{
				if (Page != null && !DesignMode)
				{
					// Validate new value by trying to construct a TrustRoot object based on it.
					new DotNetOpenId.Provider.TrustRoot(getResolvedTrustRoot(value).ToString()); // throws an exception on failure.
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
				ViewState[trustRootUrlViewStateKey] = value; 
			}
		}

		const string cssClassDefault = "openid";
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(cssClassDefault)]
		public override string CssClass
		{
			get { return WrappedTextBox.CssClass; }
			set { WrappedTextBox.CssClass = value; }
		}

		const string showLogoViewStateKey = "ShowLogo";
		const bool showLogoDefault = true;
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(showLogoDefault)]
		public bool ShowLogo {
			get { return (bool)(ViewState[showLogoViewStateKey] ?? showLogoDefault); }
			set { ViewState[showLogoViewStateKey] = value; }
		}

		const string usePersistentCookieViewStateKey = "UsePersistentCookie";
		protected const bool usePersistentCookieDefault = false;
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(usePersistentCookieDefault)]
		[Description("Whether to send a persistent cookie upon successful " +
			"login so the user does not have to log in upon returning to this site.")]
		public virtual bool UsePersistentCookie
		{
			get { return (bool)(ViewState[usePersistentCookieViewStateKey] ?? usePersistentCookieDefault); }
			set { ViewState[usePersistentCookieViewStateKey] = value; }
		}

		const int columnsDefault = 40;
		[Bindable(true)]
		[Category(appearanceCategory)]
		[DefaultValue(columnsDefault)]
		public int Columns
		{
			get { return WrappedTextBox.Columns; }
			set { WrappedTextBox.Columns = value; }
		}

		protected const short tabIndexDefault = 0;
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(tabIndexDefault)]
		public override short TabIndex {
			get { return WrappedTextBox.TabIndex; }
			set { WrappedTextBox.TabIndex = value; }
		}

		const string requestNicknameViewStateKey = "RequestNickname";
		const ProfileRequest requestNicknameDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestNicknameDefault)]
		public ProfileRequest RequestNickname
		{
			get { return (ProfileRequest)(ViewState[requestNicknameViewStateKey] ?? requestNicknameDefault); }
			set { ViewState[requestNicknameViewStateKey] = value; }
		}

		const string requestEmailViewStateKey = "RequestEmail";
		const ProfileRequest requestEmailDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestEmailDefault)]
		public ProfileRequest RequestEmail
		{
			get { return (ProfileRequest)(ViewState[requestEmailViewStateKey] ?? requestEmailDefault); }
			set { ViewState[requestEmailViewStateKey] = value; }
		}

		const string requestFullNameViewStateKey = "RequestFullName";
		const ProfileRequest requestFullNameDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestFullNameDefault)]
		public ProfileRequest RequestFullName
		{
			get { return (ProfileRequest)(ViewState[requestFullNameViewStateKey] ?? requestFullNameDefault); }
			set { ViewState[requestFullNameViewStateKey] = value; }
		}

		const string requestBirthdateViewStateKey = "RequestBirthday";
		const ProfileRequest requestBirthdateDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestBirthdateDefault)]
		public ProfileRequest RequestBirthdate
		{
			get { return (ProfileRequest)(ViewState[requestBirthdateViewStateKey] ?? requestBirthdateDefault); }
			set { ViewState[requestBirthdateViewStateKey] = value; }
		}

		const string requestGenderViewStateKey = "RequestGender";
		const ProfileRequest requestGenderDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestGenderDefault)]
		public ProfileRequest RequestGender
		{
			get { return (ProfileRequest)(ViewState[requestGenderViewStateKey] ?? requestGenderDefault); }
			set { ViewState[requestGenderViewStateKey] = value; }
		}

		const string requestPostalCodeViewStateKey = "RequestPostalCode";
		const ProfileRequest requestPostalCodeDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestPostalCodeDefault)]
		public ProfileRequest RequestPostalCode
		{
			get { return (ProfileRequest)(ViewState[requestPostalCodeViewStateKey] ?? requestPostalCodeDefault); }
			set { ViewState[requestPostalCodeViewStateKey] = value; }
		}

		const string requestCountryViewStateKey = "RequestCountry";
		const ProfileRequest requestCountryDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestCountryDefault)]
		public ProfileRequest RequestCountry
		{
			get { return (ProfileRequest)(ViewState[requestCountryViewStateKey] ?? requestCountryDefault); }
			set { ViewState[requestCountryViewStateKey] = value; }
		}

		const string requestLanguageViewStateKey = "RequestLanguage";
		const ProfileRequest requestLanguageDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestLanguageDefault)]
		public ProfileRequest RequestLanguage
		{
			get { return (ProfileRequest)(ViewState[requestLanguageViewStateKey] ?? requestLanguageDefault); }
			set { ViewState[requestLanguageViewStateKey] = value; }
		}

		const string requestTimeZoneViewStateKey = "RequestTimeZone";
		const ProfileRequest requestTimeZoneDefault = ProfileRequest.NoRequest;
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(requestTimeZoneDefault)]
		public ProfileRequest RequestTimeZone
		{
			get { return (ProfileRequest)(ViewState[requestTimeZoneViewStateKey] ?? requestTimeZoneDefault); }
			set { ViewState[requestTimeZoneViewStateKey] = value; }
		}

		const string policyUrlViewStateKey = "PolicyUrl";
		const string policyUrlDefault = "";
		[Bindable(true)]
		[Category(profileCategory)]
		[DefaultValue(policyUrlDefault)]
		public string PolicyUrl
		{
			get { return (string)ViewState[policyUrlViewStateKey] ?? policyUrlDefault; }
			set { ViewState[policyUrlViewStateKey] = value; }
		}

		const string enableRequestProfileViewStateKey = "EnableRequestProfile";
		const bool enableRequestProfileDefault = true;
		[Bindable(true)]
		[Category(profileCategory)]
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
				if (!Page.IsPostBack && Page.Request.QueryString[QueryStringArgs.openid.mode] != null)
				{
					DotNetOpenId.Consumer.Consumer consumer =
						new DotNetOpenId.Consumer.Consumer();

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
				OnCanceled(cex);
			}
		}
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (ShowLogo)
			{
				string logoUrl = Page.ClientScript.GetWebResourceUrl(
					typeof(OpenIdTextBox), EmbeddedLogoResourceName);
				WrappedTextBox.Style["background"] = string.Format(
					"url({0}) no-repeat", logoUrl);
				WrappedTextBox.Style["background-position"] = "0 50%";
				WrappedTextBox.Style[HtmlTextWriterStyle.PaddingLeft] = "18px";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderStyle] = "solid";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderWidth] = "1px";
				WrappedTextBox.Style[HtmlTextWriterStyle.BorderColor] = "lightgray";
			}
		}
		public void Login()
		{
			if (string.IsNullOrEmpty(Text))
				throw new InvalidOperationException(DotNetOpenId.Strings.OpenIdTextBoxEmpty);

			try {
				DotNetOpenId.Consumer.Consumer consumer =
					new DotNetOpenId.Consumer.Consumer();

				Uri userUri = UriUtil.NormalizeUri(Text);
				// Initiate openid request
				AuthRequest request = consumer.Begin(userUri);
				if (EnableRequestProfile) addProfileArgs(request);

				// Build the return_to URL
				UriBuilder return_to = new UriBuilder(Page.Request.Url);
				// Trim off any old "openid." prefixed parameters to avoid carrying
				// state from a prior login attempt.
				return_to.Query = string.Empty;
				var return_to_params = new Dictionary<string, string>(Page.Request.QueryString.Count);
				foreach (string key in Page.Request.QueryString) {
					if (!key.StartsWith(QueryStringArgs.openid.Prefix) && key != QueryStringArgs.nonce) {
						return_to_params.Add(key, Page.Request.QueryString[key]);
					}
				}
				UriUtil.AppendQueryArgs(return_to, return_to_params);

				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder trustRoot = getResolvedTrustRoot(TrustRootUrl);
				trustRoot.Scheme = return_to.Scheme;
				trustRoot.Port = return_to.Port;
				// Throw an exception now if the trustroot and the return_to URLs don't match
				// as required by the provider.  We could wait for the provider to test this and
				// fail, but this will be faster and give us a better error message.
				if (!(new DotNetOpenId.Provider.TrustRoot(trustRoot.ToString()).ValidateUrl(return_to.ToString())))
					throw new DotNetOpenId.Provider.UntrustedReturnUrlException(return_to.Uri, trustRoot.ToString(), new NameValueCollection());

				// Note: we must use trustRoot.ToString() because trustRoot.Uri throws when wildcards are present.
				Uri redirectUrl = request.CreateRedirect(trustRoot.ToString(), return_to.Uri, AuthRequest.Mode.Setup);

				Page.Response.Redirect(redirectUrl.AbsoluteUri);
			} catch (WebException ex) {
				OnError(ex);
			} catch (FailureException ex) {
				OnError(ex);
			}
		}

		void addProfileArgs(AuthRequest request)
		{
			request.ExtraArgs.Add(QueryStringArgs.openid.sreg.required, string.Join(",", assembleProfileFields(ProfileRequest.Require)));
			request.ExtraArgs.Add(QueryStringArgs.openid.sreg.optional, string.Join(",", assembleProfileFields(ProfileRequest.Request)));
			request.ExtraArgs.Add(QueryStringArgs.openid.sreg.policy_url, PolicyUrl);
		}

		string[] assembleProfileFields(ProfileRequest level)
		{
			List<string> fields = new List<string>(10);
			if (RequestNickname == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.nickname);
			if (RequestEmail == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.email);
			if (RequestFullName == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.fullname);
			if (RequestBirthdate == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.dob);
			if (RequestGender == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.gender);
			if (RequestPostalCode == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.postcode);
			if (RequestCountry == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.country);
			if (RequestLanguage == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.language);
			if (RequestTimeZone == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.timezone);

			return fields.ToArray();
		}
		ProfileFieldValues parseProfileFields(NameValueCollection queryString)
		{
			ProfileFieldValues fields = new ProfileFieldValues();
			if (RequestNickname > ProfileRequest.NoRequest)
				fields.Nickname = queryString[QueryStringArgs.openid.sreg.nickname];
			if (RequestEmail > ProfileRequest.NoRequest)
				fields.Email = queryString[QueryStringArgs.openid.sreg.email];
			if (RequestFullName > ProfileRequest.NoRequest)
				fields.Fullname = queryString[QueryStringArgs.openid.sreg.fullname];
			if (RequestBirthdate > ProfileRequest.NoRequest && !string.IsNullOrEmpty(queryString[QueryStringArgs.openid.sreg.dob]))
			{
				DateTime birthdate;
				DateTime.TryParse(queryString[QueryStringArgs.openid.sreg.dob], out birthdate);
				fields.Birthdate = birthdate;
			}
			if (RequestGender > ProfileRequest.NoRequest)
				switch (queryString[QueryStringArgs.openid.sreg.gender])
				{
					case QueryStringArgs.Genders.Male: fields.Gender = Gender.Male; break;
					case QueryStringArgs.Genders.Female: fields.Gender = Gender.Female; break;
				}
			if (RequestPostalCode > ProfileRequest.NoRequest)
				fields.PostalCode = queryString[QueryStringArgs.openid.sreg.postcode];
			if (RequestCountry > ProfileRequest.NoRequest)
				fields.Country = queryString[QueryStringArgs.openid.sreg.country];
			if (RequestLanguage > ProfileRequest.NoRequest)
				fields.Language = queryString[QueryStringArgs.openid.sreg.language];
			if (RequestTimeZone > ProfileRequest.NoRequest)
				fields.TimeZone = queryString[QueryStringArgs.openid.sreg.timezone];
			return fields;
		}

		UriBuilder getResolvedTrustRoot(string trustRoot)
		{
			Debug.Assert(Page != null, "Current HttpContext required to resolve URLs.");
			// Allow for *. trustroot notation, as well as ASP.NET ~/ shortcuts.

			// We have to temporarily remove the *. notation if it's there so that
			// the rest of our URL manipulation will succeed.
			bool foundWildcard = false;
			// Note: we don't just use string.Replace because poorly written URLs
			// could potentially have multiple :// sequences in them.
			string trustRootNoWildcard = Regex.Replace(trustRoot, @"^(\w+://)\*\.",
				delegate(Match m) {
					foundWildcard = true;
					return m.Groups[1].Value;
				});

			UriBuilder fullyQualifiedTrustRoot = new UriBuilder(
				new Uri(Page.Request.Url, Page.ResolveUrl(trustRootNoWildcard)));

			if (foundWildcard)
			{
				fullyQualifiedTrustRoot.Host = "*." + fullyQualifiedTrustRoot.Host;
			}

			// Is it valid?
			// Note: we MUST use ToString.  Uri property throws if wildcard is present.
			new DotNetOpenId.Provider.TrustRoot(fullyQualifiedTrustRoot.ToString()); // throws if not valid

			return fullyQualifiedTrustRoot;
		}

		#region Events
		public class OpenIdEventArgs : EventArgs
		{
			public OpenIdEventArgs(Uri openIdUri, ProfileFieldValues profileFields)
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

			private ProfileFieldValues profileFields;
			public ProfileFieldValues ProfileFields
			{
				get { return profileFields; }
			}
		}
		/// <summary>
		/// Fired upon completion of a successful login.
		/// </summary>
		[Description("Fired upon completion of a successful login.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		protected virtual void OnLoggedIn(Uri openIdUri, ProfileFieldValues profileFields)
		{
			EventHandler<OpenIdEventArgs> loggedIn = LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(openIdUri, profileFields);
			if (loggedIn != null)
				loggedIn(this, args);
			if (!args.Cancel)
				FormsAuthentication.RedirectFromLoginPage(openIdUri.AbsoluteUri, UsePersistentCookie);
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

		/// <summary>
		/// Fired when a login attempt fails or is canceled by the user.
		/// </summary>
		[Description("Fired when a login attempt fails or is canceled by the user.")]
		public event EventHandler<ErrorEventArgs> Error;
		protected virtual void OnError(Exception errorException)
		{
			if (errorException == null)
				throw new ArgumentNullException("errorException");

			EventHandler<ErrorEventArgs> error = Error;
			if (error != null)
				error(this, new ErrorEventArgs(errorException.Message, errorException));
		}

		/// <summary>
		/// Fired when an authentication attempt is canceled at the OpenID Provider.
		/// </summary>
		[Description("Fired when an authentication attempt is canceled at the OpenID Provider.")]
		public event EventHandler<ErrorEventArgs> Canceled;
		protected virtual void OnCanceled(Exception cancelException)
		{
			if (cancelException == null)
				throw new ArgumentNullException("cancelException");

			EventHandler<ErrorEventArgs> canceled = Canceled;
			if (canceled != null)
				canceled(this, new ErrorEventArgs(cancelException.Message, cancelException));
		}
		#endregion
	}
}
