/********************************************************
 * Copyright (C) 2008 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.MobileControls;
using DotNetOpenId.Extensions.SimpleRegistration;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdMobileTextBox.EmbeddedLogoResourceName, "image/gif")]

namespace DotNetOpenId.RelyingParty
{
	/// <summary>
	/// An ASP.NET control for mobile devices that provides a minimal text box that is OpenID-aware.
	/// </summary>
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:OpenIdMobileTextBox runat=\"server\"></{0}:OpenIdMobileTextBox>")]
	public class OpenIdMobileTextBox : TextBox
	{
		internal const string EmbeddedLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.openid_login.gif";

		const string appearanceCategory = "Appearance";
		const string profileCategory = "Simple Registration";
		const string behaviorCategory = "Behavior";

		#region Properties
		const string realmUrlViewStateKey = "RealmUrl";
		const string realmUrlDefault = "~/";
		/// <summary>
		/// The OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.Realm"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
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
					new Realm(getResolvedRealm(value).ToString()); // throws an exception on failure.
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

		const string immediateModeViewStateKey = "ImmediateMode";
		const bool immediateModeDefault = false;
		/// <summary>
		/// True if a Provider should reply immediately to the authentication request
		/// without interacting with the user.  False if the Provider can take time
		/// to authenticate the user in order to complete an authentication attempt.
		/// </summary>
		[Bindable(true)]
		[Category(behaviorCategory)]
		[DefaultValue(immediateModeDefault)]
		[Description("Whether the Provider should respond immediately to an authentication attempt without interacting with the user.")]
		public bool ImmediateMode {
			get { return (bool)(ViewState[immediateModeViewStateKey] ?? immediateModeDefault); }
			set { ViewState[immediateModeViewStateKey] = value; }
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
		#endregion

		/// <summary>
		/// Checks for incoming OpenID authentication responses and fires appropriate events.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			var consumer = new OpenIdRelyingParty();
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

		/// <summary>
		/// The OpenID authentication request that is about to be sent.
		/// </summary>
		protected IAuthenticationRequest Request;
		/// <summary>
		/// Constructs the authentication request and adds the Simple Registration extension arguments.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected void PrepareAuthenticationRequest() {
			if (string.IsNullOrEmpty(Text))
				throw new InvalidOperationException(DotNetOpenId.Strings.OpenIdTextBoxEmpty);

			try {
				var consumer = new OpenIdRelyingParty();

				// Resolve the trust root, and swap out the scheme and port if necessary to match the
				// return_to URL, since this match is required by OpenId, and the consumer app
				// may be using HTTP at some times and HTTPS at others.
				UriBuilder realm = getResolvedRealm(RealmUrl);
				realm.Scheme = Page.Request.Url.Scheme;
				realm.Port = Page.Request.Url.Port;

				// Initiate openid request
				// Note: we must use realm.ToString() because trustRoot.Uri throws when wildcards are present.
				Request = consumer.CreateRequest(Text, realm.ToString());
				Request.Mode = ImmediateMode ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
				if (EnableRequestProfile) addProfileArgs(Request);
			} catch (WebException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			} catch (OpenIdException ex) {
				OnFailed(new FailedAuthenticationResponse(ex));
			}
		}

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public void LogOn()
		{
			if (Request == null)
				PrepareAuthenticationRequest();
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
					null : new Uri(Page.Request.Url, Page.ResolveUrl(PolicyUrl)),
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenId.Realm")]
		UriBuilder getResolvedRealm(string realm)
		{
			Debug.Assert(Page != null, "Current HttpContext required to resolve URLs.");
			// Allow for *. realm notation, as well as ASP.NET ~/ shortcuts.

			// We have to temporarily remove the *. notation if it's there so that
			// the rest of our URL manipulation will succeed.
			bool foundWildcard = false;
			// Note: we don't just use string.Replace because poorly written URLs
			// could potentially have multiple :// sequences in them.
			string realmNoWildcard = Regex.Replace(realm, @"^(\w+://)\*\.",
				delegate(Match m) {
					foundWildcard = true;
					return m.Groups[1].Value;
				});

			UriBuilder fullyQualifiedRealm = new UriBuilder(
				new Uri(Page.Request.Url, Page.ResolveUrl(realmNoWildcard)));

			if (foundWildcard)
			{
				fullyQualifiedRealm.Host = "*." + fullyQualifiedRealm.Host;
			}

			// Is it valid?
			// Note: we MUST use ToString.  Uri property throws if wildcard is present.
			new Realm(fullyQualifiedRealm.ToString()); // throws if not valid

			return fullyQualifiedRealm;
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
	}
}
