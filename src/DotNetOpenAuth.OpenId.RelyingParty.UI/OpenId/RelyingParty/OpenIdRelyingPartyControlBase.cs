//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyControlBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyControlBase.EmbeddedJavascriptResource, "text/javascript")]

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Methods of indicating to the rest of the web site that the user has logged in.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnSite", Justification = "Two words intended.")]
	public enum LogOnSiteNotification {
		/// <summary>
		/// The rest of the web site is unaware that the user just completed an OpenID login.
		/// </summary>
		None,

		/// <summary>
		/// After the <see cref="OpenIdRelyingPartyControlBase.LoggedIn"/> event is fired
		/// the control automatically calls <see cref="System.Web.Security.FormsAuthentication.RedirectFromLoginPage(string, bool)"/>
		/// with the <see cref="IAuthenticationResponse.ClaimedIdentifier"/> as the username
		/// unless the <see cref="OpenIdRelyingPartyControlBase.LoggedIn"/> event handler sets
		/// <see cref="OpenIdEventArgs.Cancel"/> property to true.
		/// </summary>
		FormsAuthentication,
	}

	/// <summary>
	/// How an OpenID user session should be persisted across visits.
	/// </summary>
	public enum LogOnPersistence {
		/// <summary>
		/// The user should only be logged in as long as the browser window remains open.
		/// Nothing is persisted to help the user on a return visit.  Public kiosk mode.
		/// </summary>
		Session,

		/// <summary>
		/// The user should only be logged in as long as the browser window remains open.
		/// The OpenID Identifier is persisted to help expedite re-authentication when
		/// the user visits the next time.
		/// </summary>
		SessionAndPersistentIdentifier,

		/// <summary>
		/// The user is issued a persistent authentication ticket so that no login is
		/// necessary on their return visit.
		/// </summary>
		PersistentAuthentication,
	}

	/// <summary>
	/// A common base class for OpenID Relying Party controls.
	/// </summary>
	[DefaultProperty("Identifier"), ValidationProperty("Identifier")]
	[ParseChildren(true), PersistChildren(false)]
	public abstract class OpenIdRelyingPartyControlBase : Control, IPostBackEventHandler, IDisposable {
		/// <summary>
		/// The manifest resource name of the javascript file to include on the hosting page.
		/// </summary>
		internal const string EmbeddedJavascriptResource = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdRelyingPartyControlBase.js";

		/// <summary>
		/// The cookie used to persist the Identifier the user logged in with.
		/// </summary>
		internal const string PersistentIdentifierCookieName = OpenIdUtilities.CustomParameterPrefix + "OpenIDIdentifier";

		/// <summary>
		/// The callback parameter name to use to store which control initiated the auth request.
		/// </summary>
		internal const string ReturnToReceivingControlId = OpenIdUtilities.CustomParameterPrefix + "receiver";

		#region Protected internal callback parameter names

		/// <summary>
		/// The callback parameter to use for recognizing when the callback is in a popup window or hidden iframe.
		/// </summary>
		protected internal const string UIPopupCallbackKey = OpenIdUtilities.CustomParameterPrefix + "uipopup";

		/// <summary>
		/// The parameter name to include in the formulated auth request so that javascript can know whether
		/// the OP advertises support for the UI extension.
		/// </summary>
		protected internal const string PopupUISupportedJSHint = OpenIdUtilities.CustomParameterPrefix + "popupUISupported";

		#endregion

		#region Property category constants

		/// <summary>
		/// The "Appearance" category for properties.
		/// </summary>
		protected const string AppearanceCategory = "Appearance";

		/// <summary>
		/// The "Behavior" category for properties.
		/// </summary>
		protected const string BehaviorCategory = "Behavior";

		/// <summary>
		/// The "OpenID" category for properties and events.
		/// </summary>
		protected const string OpenIdCategory = "OpenID";

		#endregion

		#region Private callback parameter names

		/// <summary>
		/// The callback parameter for use with persisting the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieCallbackKey = OpenIdUtilities.CustomParameterPrefix + "UsePersistentCookie";

		/// <summary>
		/// The callback parameter to use for recognizing when the callback is in the parent window.
		/// </summary>
		private const string UIPopupCallbackParentKey = OpenIdUtilities.CustomParameterPrefix + "uipopupParent";

		#endregion

		#region Property default values

		/// <summary>
		/// The default value for the <see cref="Stateless"/> property.
		/// </summary>
		private const bool StatelessDefault = false;

		/// <summary>
		/// The default value for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlDefault = "";

		/// <summary>
		/// Default value of <see cref="UsePersistentCookie"/>.
		/// </summary>
		private const LogOnPersistence UsePersistentCookieDefault = LogOnPersistence.Session;

		/// <summary>
		/// Default value of <see cref="LogOnMode"/>.
		/// </summary>
		private const LogOnSiteNotification LogOnModeDefault = LogOnSiteNotification.FormsAuthentication;

		/// <summary>
		/// The default value for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlDefault = "~/";

		/// <summary>
		/// The default value for the <see cref="Popup"/> property.
		/// </summary>
		private const PopupBehavior PopupDefault = PopupBehavior.Never;

		/// <summary>
		/// The default value for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const bool RequireSslDefault = false;

		#endregion

		#region Property view state keys

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Extensions"/> property.
		/// </summary>
		private const string ExtensionsViewStateKey = "Extensions";

		/// <summary>
		/// The viewstate key to use for the <see cref="Stateless"/> property.
		/// </summary>
		private const string StatelessViewStateKey = "Stateless";

		/// <summary>
		/// The viewstate key to use for the <see cref="UsePersistentCookie"/> property.
		/// </summary>
		private const string UsePersistentCookieViewStateKey = "UsePersistentCookie";

		/// <summary>
		/// The viewstate key to use for the <see cref="LogOnMode"/> property.
		/// </summary>
		private const string LogOnModeViewStateKey = "LogOnMode";

		/// <summary>
		/// The viewstate key to use for the <see cref="RealmUrl"/> property.
		/// </summary>
		private const string RealmUrlViewStateKey = "RealmUrl";

		/// <summary>
		/// The viewstate key to use for the <see cref="ReturnToUrl"/> property.
		/// </summary>
		private const string ReturnToUrlViewStateKey = "ReturnToUrl";

		/// <summary>
		/// The key under which the value for the <see cref="Identifier"/> property will be stored.
		/// </summary>
		private const string IdentifierViewStateKey = "Identifier";

		/// <summary>
		/// The viewstate key to use for the <see cref="Popup"/> property.
		/// </summary>
		private const string PopupViewStateKey = "Popup";

		/// <summary>
		/// The viewstate key to use for the <see cref="RequireSsl"/> property.
		/// </summary>
		private const string RequireSslViewStateKey = "RequireSsl";

		#endregion

		/// <summary>
		/// The lifetime of the cookie used to persist the Identifier the user logged in with.
		/// </summary>
		private static readonly TimeSpan PersistentIdentifierTimeToLiveDefault = TimeSpan.FromDays(14);

		/// <summary>
		/// Backing field for the <see cref="RelyingParty"/> property.
		/// </summary>
		private OpenIdRelyingParty relyingParty;

		/// <summary>
		/// A value indicating whether the <see cref="relyingParty"/> field contains
		/// an instance that we own and should Dispose.
		/// </summary>
		private bool relyingPartyOwned;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyControlBase"/> class.
		/// </summary>
		protected OpenIdRelyingPartyControlBase() {
			Reporting.RecordFeatureUse(this);
		}

		#region Events

		/// <summary>
		/// Fired when the user has typed in their identifier, discovery was successful
		/// and a login attempt is about to begin.
		/// </summary>
		[Description("Fired when the user has typed in their identifier, discovery was successful and a login attempt is about to begin."), Category(OpenIdCategory)]
		public event EventHandler<OpenIdEventArgs> LoggingIn;

		/// <summary>
		/// Fired upon completion of a successful login.
		/// </summary>
		[Description("Fired upon completion of a successful login."), Category(OpenIdCategory)]
		public event EventHandler<OpenIdEventArgs> LoggedIn;

		/// <summary>
		/// Fired when a login attempt fails.
		/// </summary>
		[Description("Fired when a login attempt fails."), Category(OpenIdCategory)]
		public event EventHandler<OpenIdEventArgs> Failed;

		/// <summary>
		/// Fired when an authentication attempt is canceled at the OpenID Provider.
		/// </summary>
		[Description("Fired when an authentication attempt is canceled at the OpenID Provider."), Category(OpenIdCategory)]
		public event EventHandler<OpenIdEventArgs> Canceled;

		/// <summary>
		/// Occurs when the <see cref="Identifier"/> property is changed.
		/// </summary>
		protected event EventHandler IdentifierChanged;

		#endregion

		/// <summary>
		/// Gets or sets the <see cref="OpenIdRelyingParty"/> instance to use.
		/// </summary>
		/// <value>The default value is an <see cref="OpenIdRelyingParty"/> instance initialized according to the web.config file.</value>
		/// <remarks>
		/// A performance optimization would be to store off the 
		/// instance as a static member in your web site and set it
		/// to this property in your <see cref="Control.Load">Page.Load</see>
		/// event since instantiating these instances can be expensive on 
		/// heavily trafficked web pages.
		/// </remarks>
		[Browsable(false)]
		public virtual OpenIdRelyingParty RelyingParty {
			get {
				if (this.relyingParty == null) {
					this.relyingParty = this.CreateRelyingParty();
					this.ConfigureRelyingParty(this.relyingParty);
					this.relyingPartyOwned = true;
				}
				return this.relyingParty;
			}

			set {
				if (this.relyingPartyOwned && this.relyingParty != null) {
					this.relyingParty.Dispose();
				}

				this.relyingParty = value;
				this.relyingPartyOwned = false;
			}
		}

		/// <summary>
		/// Gets the collection of extension requests this selector should include in generated requests.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public Collection<IOpenIdMessageExtension> Extensions {
			get {
				if (this.ViewState[ExtensionsViewStateKey] == null) {
					var extensions = new Collection<IOpenIdMessageExtension>();
					this.ViewState[ExtensionsViewStateKey] = extensions;
					return extensions;
				} else {
					return (Collection<IOpenIdMessageExtension>)this.ViewState[ExtensionsViewStateKey];
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether stateless mode is used.
		/// </summary>
		[Bindable(true), DefaultValue(StatelessDefault), Category(OpenIdCategory)]
		[Description("Controls whether stateless mode is used.")]
		public bool Stateless {
			get { return (bool)(ViewState[StatelessViewStateKey] ?? StatelessDefault); }
			set { ViewState[StatelessViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the OpenID <see cref="Realm"/> of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(RealmUrlDefault), Category(OpenIdCategory)]
		[Description("The OpenID Realm of the relying party web site.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string RealmUrl {
			get {
				return (string)(ViewState[RealmUrlViewStateKey] ?? RealmUrlDefault);
			}

			set {
				Requires.NotNullOrEmpty(value, "value");

				if (Page != null && !DesignMode) {
					// Validate new value by trying to construct a Realm object based on it.
					new Realm(OpenIdUtilities.GetResolvedRealm(this.Page, value, new HttpRequestWrapper(this.Context.Request))); // throws an exception on failure.
				} else {
					// We can't fully test it, but it should start with either ~/ or a protocol.
					if (Regex.IsMatch(value, @"^https?://")) {
						new Uri(value.Replace("*.", string.Empty)); // make sure it's fully-qualified, but ignore wildcards
					} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
						// this is valid too
					} else {
						throw new UriFormatException();
					}
				}
				ViewState[RealmUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the OpenID ReturnTo of the relying party web site.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Bindable property must be simple type")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Using Uri.ctor for validation.")]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), DefaultValue(ReturnToUrlDefault), Category(OpenIdCategory)]
		[Description("The OpenID ReturnTo of the relying party web site.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string ReturnToUrl {
			get {
				return (string)(this.ViewState[ReturnToUrlViewStateKey] ?? ReturnToUrlDefault);
			}

			set {
				if (this.Page != null && !this.DesignMode) {
					// Validate new value by trying to construct a Uri based on it.
					new Uri(new HttpRequestWrapper(this.Context.Request).GetPublicFacingUrl(), this.Page.ResolveUrl(value)); // throws an exception on failure.
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

				this.ViewState[ReturnToUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to send a persistent cookie upon successful 
		/// login so the user does not have to log in upon returning to this site.
		/// </summary>
		[Bindable(true), DefaultValue(UsePersistentCookieDefault), Category(BehaviorCategory)]
		[Description("Whether to send a persistent cookie upon successful " +
			"login so the user does not have to log in upon returning to this site.")]
		public virtual LogOnPersistence UsePersistentCookie {
			get { return (LogOnPersistence)(this.ViewState[UsePersistentCookieViewStateKey] ?? UsePersistentCookieDefault); }
			set { this.ViewState[UsePersistentCookieViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the way a completed login is communicated to the rest of the web site.
		/// </summary>
		[Bindable(true), DefaultValue(LogOnModeDefault), Category(BehaviorCategory)]
		[Description("The way a completed login is communicated to the rest of the web site.")]
		public virtual LogOnSiteNotification LogOnMode {
			get { return (LogOnSiteNotification)(this.ViewState[LogOnModeViewStateKey] ?? LogOnModeDefault); }
			set { this.ViewState[LogOnModeViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating when to use a popup window to complete the login experience.
		/// </summary>
		/// <value>The default value is <see cref="PopupBehavior.Never"/>.</value>
		[Bindable(true), DefaultValue(PopupDefault), Category(BehaviorCategory)]
		[Description("When to use a popup window to complete the login experience.")]
		public virtual PopupBehavior Popup {
			get { return (PopupBehavior)(ViewState[PopupViewStateKey] ?? PopupDefault); }
			set { ViewState[PopupViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to enforce on high security mode,
		/// which requires the full authentication pipeline to be protected by SSL.
		/// </summary>
		[Bindable(true), DefaultValue(RequireSslDefault), Category(OpenIdCategory)]
		[Description("Turns on high security mode, requiring the full authentication pipeline to be protected by SSL.")]
		public bool RequireSsl {
			get { return (bool)(ViewState[RequireSslViewStateKey] ?? RequireSslDefault); }
			set { ViewState[RequireSslViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Identifier that will be used to initiate login.
		/// </summary>
		[Bindable(true), Category(OpenIdCategory)]
		[Description("The OpenID Identifier that this button will use to initiate login.")]
		[TypeConverter(typeof(IdentifierConverter))]
		public virtual Identifier Identifier {
			get {
				return (Identifier)ViewState[IdentifierViewStateKey];
			}

			set {
				ViewState[IdentifierViewStateKey] = value;
				this.OnIdentifierChanged();
			}
		}

		/// <summary>
		/// Gets or sets the default association preference to set on authentication requests.
		/// </summary>
		internal AssociationPreference AssociationPreference { get; set; }

		/// <summary>
		/// Gets ancestor controls, starting with the immediate parent, and progressing to more distant ancestors.
		/// </summary>
		protected IEnumerable<Control> ParentControls {
			get {
				Control parent = this;
				while ((parent = parent.Parent) != null) {
					yield return parent;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this control is a child control of a composite OpenID control.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is embedded in parent OpenID control; otherwise, <c>false</c>.
		/// </value>
		protected bool IsEmbeddedInParentOpenIdControl {
			get { return this.ParentControls.OfType<OpenIdRelyingPartyControlBase>().Any(); }
		}

		/// <summary>
		/// Clears any cookie set by this control to help the user on a returning visit next time.
		/// </summary>
		public static void LogOff() {
			HttpContext.Current.Response.SetCookie(CreateIdentifierPersistingCookie(null));
		}

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public async Task LogOnAsync(CancellationToken cancellationToken) {
			var authenticationRequests = await this.CreateRequestsAsync(cancellationToken);
			IAuthenticationRequest request = authenticationRequests.FirstOrDefault();
			ErrorUtilities.VerifyProtocol(request != null, OpenIdStrings.OpenIdEndpointNotFound);
			await this.LogOnAsync(request, cancellationToken);
		}

		/// <summary>
		/// Immediately redirects to the OpenID Provider to verify the Identifier
		/// provided in the text box.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public async Task LogOnAsync(IAuthenticationRequest request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			if (this.IsPopupAppropriate(request)) {
				await this.ScriptPopupWindowAsync(request, cancellationToken);
			} else {
				await request.RedirectToProviderAsync(new HttpContextWrapper(this.Context), cancellationToken);
			}
		}

		#region IPostBackEventHandler Members

		/// <summary>
		/// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
		/// </summary>
		/// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
			this.RaisePostBackEvent(eventArgument);
		}

		#endregion

		/// <summary>
		/// Enables a server control to perform final clean up before it is released from memory.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Unavoidable because base class does not expose a protected virtual Dispose(bool) method."), SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Base class doesn't implement virtual Dispose(bool), so we must call its Dispose() method.")]
		public sealed override void Dispose() {
			this.Dispose(true);
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="identifier">The identifier to create a request for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier" />.
		/// </returns>
		protected internal virtual Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");

			// If this control is actually a member of another OpenID RP control,
			// delegate creation of requests to the parent control.
			var parentOwner = this.ParentControls.OfType<OpenIdRelyingPartyControlBase>().FirstOrDefault();
			if (parentOwner != null) {
				return parentOwner.CreateRequestsAsync(identifier, cancellationToken);
			} else {
				// Delegate to a private method to keep 'yield return' and Code Contract separate.
				return this.CreateRequestsCoreAsync(identifier, cancellationToken);
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (this.relyingPartyOwned && this.relyingParty != null) {
					this.relyingParty.Dispose();
					this.relyingParty = null;
				}
			}
		}

		/// <summary>
		/// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
		/// </summary>
		/// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Predefined signature.")]
		protected virtual void RaisePostBackEvent(string eventArgument) {
		}

		/// <summary>
		/// Creates the authentication requests for the value set in the <see cref="Identifier" /> property.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier" />.
		/// </returns>
		protected Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(CancellationToken cancellationToken) {
			RequiresEx.ValidState(this.Identifier != null, OpenIdStrings.NoIdentifierSet);
			return this.CreateRequestsAsync(this.Identifier, cancellationToken);
		}

		/// <summary>
		/// Raises the <see cref="E:Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Page.IsPostBack) {
				// OpenID responses NEVER come in the form of a postback.
				return;
			}

			if (this.Identifier == null) {
				this.TryPresetIdentifierWithCookie();
			}

			// Take an unreliable sneek peek to see if we're in a popup and an OpenID
			// assertion is coming in.  We shouldn't process assertions in a popup window.
			if (this.Page.Request.QueryString[UIPopupCallbackKey] == "1"
			    && this.Page.Request.QueryString[UIPopupCallbackParentKey] == null) {
				// We're in a popup window.  We need to close it and pass the
				// message back to the parent window for processing.
				this.Page.RegisterAsyncTask(new PageAsyncTask(async ct => {
					await this.ScriptClosingPopupOrIFrameAsync(ct);
				}));
			} else {
				// Only sniff for an OpenID response if it is targeted at this control.
				// Note that Stateless mode causes no receiver to be indicated, and
				// we want to handle that, but only if there isn't a parent control that
				// will be handling that.
				string receiver = this.Page.Request.QueryString[ReturnToReceivingControlId]
				                  ?? this.Page.Request.Form[ReturnToReceivingControlId];
				if (receiver == this.ClientID || (receiver == null && !this.IsEmbeddedInParentOpenIdControl)) {
					this.Page.RegisterAsyncTask(
						new PageAsyncTask(
							async ct => {
								var response = await this.RelyingParty.GetResponseAsync(new HttpRequestWrapper(this.Context.Request), ct);
								Logger.Controls.DebugFormat(
									"The {0} control checked for an authentication response and found: {1}",
									this.ID,
									response != null ? response.Status.ToString() : "nothing");
								this.ProcessResponse(response);
							}));
				}
			}
		}

		/// <summary>
		/// Notifies the user agent via an AJAX response of a completed authentication attempt.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		protected virtual Task ScriptClosingPopupOrIFrameAsync(CancellationToken cancellationToken) {
			return this.RelyingParty.ProcessResponseFromPopupAsync(new HttpRequestWrapper(this.Context.Request).AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Called when the <see cref="Identifier"/> property is changed.
		/// </summary>
		protected virtual void OnIdentifierChanged() {
			var identifierChanged = this.IdentifierChanged;
			if (identifierChanged != null) {
				identifierChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Processes the response.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void ProcessResponse(IAuthenticationResponse response) {
			if (response == null) {
				return;
			}
			string persistentString = response.GetUntrustedCallbackArgument(UsePersistentCookieCallbackKey);
			if (persistentString != null) {
				this.UsePersistentCookie = (LogOnPersistence)Enum.Parse(typeof(LogOnPersistence), persistentString);
			}

			switch (response.Status) {
				case AuthenticationStatus.Authenticated:
					this.OnLoggedIn(response);
					break;
				case AuthenticationStatus.Canceled:
					this.OnCanceled(response);
					break;
				case AuthenticationStatus.Failed:
					this.OnFailed(response);
					break;
				case AuthenticationStatus.SetupRequired:
				case AuthenticationStatus.ExtensionsOnly:
				default:
					// The NotApplicable (extension-only assertion) is NOT one that we support
					// in this control because that scenario is primarily interesting to RPs
					// that are asking a specific OP, and it is not user-initiated as this textbox
					// is designed for.
					throw new InvalidOperationException(MessagingStrings.UnexpectedMessageReceivedOfMany);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdRelyingPartyControlBase), EmbeddedJavascriptResource);
		}

		/// <summary>
		/// Fires the <see cref="LoggedIn"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnLoggedIn(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			Requires.That(response.Status == AuthenticationStatus.Authenticated, "response", "response not authenticatedl");

			var loggedIn = this.LoggedIn;
			OpenIdEventArgs args = new OpenIdEventArgs(response);
			if (loggedIn != null) {
				loggedIn(this, args);
			}

			if (!args.Cancel) {
				if (this.UsePersistentCookie == LogOnPersistence.SessionAndPersistentIdentifier) {
					Page.Response.SetCookie(CreateIdentifierPersistingCookie(response));
				}

				switch (this.LogOnMode) {
					case LogOnSiteNotification.FormsAuthentication:
						FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, this.UsePersistentCookie == LogOnPersistence.PersistentAuthentication);
						break;
					case LogOnSiteNotification.None:
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Fires the <see cref="LoggingIn"/> event.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// Returns whether the login should proceed.  False if some event handler canceled the request.
		/// </returns>
		protected virtual bool OnLoggingIn(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");

			EventHandler<OpenIdEventArgs> loggingIn = this.LoggingIn;

			OpenIdEventArgs args = new OpenIdEventArgs(request);
			if (loggingIn != null) {
				loggingIn(this, args);
			}

			return !args.Cancel;
		}

		/// <summary>
		/// Fires the <see cref="Canceled"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnCanceled(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			Requires.That(response.Status == AuthenticationStatus.Canceled, "response", "response not canceled");

			var canceled = this.Canceled;
			if (canceled != null) {
				canceled(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Fires the <see cref="Failed"/> event.
		/// </summary>
		/// <param name="response">The response.</param>
		protected virtual void OnFailed(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");
			Requires.That(response.Status == AuthenticationStatus.Failed, "response", "response not failed");

			var failed = this.Failed;
			if (failed != null) {
				failed(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Creates the relying party instance used to generate authentication requests.
		/// </summary>
		/// <returns>The instantiated relying party.</returns>
		protected OpenIdRelyingParty CreateRelyingParty() {
			ICryptoKeyAndNonceStore store = this.Stateless ? null : OpenIdElement.Configuration.RelyingParty.ApplicationStore.CreateInstance(OpenIdRelyingParty.GetHttpApplicationStore(new HttpContextWrapper(this.Context)), null);
			return this.CreateRelyingParty(store);
		}

		/// <summary>
		/// Creates the relying party instance used to generate authentication requests.
		/// </summary>
		/// <param name="store">The store to pass to the relying party constructor.</param>
		/// <returns>The instantiated relying party.</returns>
		protected virtual OpenIdRelyingParty CreateRelyingParty(ICryptoKeyAndNonceStore store) {
			return new OpenIdRelyingParty(store);
		}

		/// <summary>
		/// Configures the relying party.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "relyingParty", Justification = "This makes it possible for overrides to see the value before it is set on a field.")]
		protected virtual void ConfigureRelyingParty(OpenIdRelyingParty relyingParty) {
			Requires.NotNull(relyingParty, "relyingParty");

			// Only set RequireSsl to true, as we don't want to override 
			// a .config setting of true with false.
			if (this.RequireSsl) {
				relyingParty.SecuritySettings.RequireSsl = true;
			}
		}

		/// <summary>
		/// Detects whether a popup window should be used to show the Provider's UI.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// 	<c>true</c> if a popup should be used; <c>false</c> otherwise.
		/// </returns>
		protected virtual bool IsPopupAppropriate(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");

			switch (this.Popup) {
				case PopupBehavior.Never:
					return false;
				case PopupBehavior.Always:
					return true;
				case PopupBehavior.IfProviderSupported:
					return request.DiscoveryResult.IsExtensionSupported<UIRequest>();
				default:
					throw ErrorUtilities.ThrowInternal("Unexpected value for Popup property.");
			}
		}

		/// <summary>
		/// Adds attributes to an HTML &lt;A&gt; tag that will be written by the caller using
		/// <see cref="HtmlTextWriter.RenderBeginTag(HtmlTextWriterTag)" /> after this method.
		/// </summary>
		/// <param name="writer">The HTML writer.</param>
		/// <param name="response">The response.</param>
		/// <param name="windowStatus">The text to try to display in the status bar on mouse hover.</param>
		protected void RenderOpenIdMessageTransmissionAsAnchorAttributes(HtmlTextWriter writer, HttpResponseMessage response, string windowStatus) {
			Requires.NotNull(writer, "writer");
			Requires.NotNull(response, "response");

			// We render a standard HREF attribute for non-javascript browsers.
			writer.AddAttribute(HtmlTextWriterAttribute.Href, response.GetDirectUriRequest().AbsoluteUri);

			// And for the Javascript ones we do the extra work to use form POST where necessary.
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.CreateGetOrPostAHrefValue(response) + " return false;");

			writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
			if (!string.IsNullOrEmpty(windowStatus)) {
				writer.AddAttribute("onMouseOver", "window.status = " + MessagingUtilities.GetSafeJavascriptValue(windowStatus));
				writer.AddAttribute("onMouseOut", "window.status = null");
			}
		}

		/// <summary>
		/// Creates the identifier-persisting cookie, either for saving or deleting.
		/// </summary>
		/// <param name="response">The positive authentication response; or <c>null</c> to clear the cookie.</param>
		/// <returns>An persistent cookie.</returns>
		private static HttpCookie CreateIdentifierPersistingCookie(IAuthenticationResponse response) {
			HttpCookie cookie = new HttpCookie(PersistentIdentifierCookieName);
			bool clearingCookie = false;

			// We'll try to store whatever it was the user originally typed in, but fallback
			// to the final claimed_id.
			if (response != null && response.Status == AuthenticationStatus.Authenticated) {
				var positiveResponse = (PositiveAuthenticationResponse)response;

				// We must escape the value because XRIs start with =, and any leading '=' gets dropped (by ASP.NET?)
				cookie.Value = Uri.EscapeDataString(positiveResponse.Endpoint.UserSuppliedIdentifier ?? response.ClaimedIdentifier);
			} else {
				clearingCookie = true;
				cookie.Value = string.Empty;
				if (HttpContext.Current.Request.Browser["supportsEmptyStringInCookieValue"] == "false") {
					cookie.Value = "NoCookie";
				}
			}

			if (clearingCookie) {
				// mark the cookie has having already expired to cause the user agent to delete
				// the old persisted cookie.
				cookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
			} else {
				// Make the cookie persistent by setting an expiration date
				cookie.Expires = DateTime.Now + PersistentIdentifierTimeToLiveDefault;
			}

			return cookie;
		}

		/// <summary>
		/// Creates the authentication requests for a given user-supplied Identifier.
		/// </summary>
		/// <param name="identifier">The identifier to create a request for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any one of which may be
		/// used to determine the user's control of the <see cref="IAuthenticationRequest.ClaimedIdentifier" />.
		/// </returns>
		private async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsCoreAsync(Identifier identifier, CancellationToken cancellationToken) {
			ErrorUtilities.VerifyArgumentNotNull(identifier, "identifier"); // NO CODE CONTRACTS! (yield return used here)
			IEnumerable<IAuthenticationRequest> requests;
			var requestContext = new HttpRequestWrapper(this.Context.Request);
			var results = new List<IAuthenticationRequest>();

			// Approximate the returnTo (either based on the customize property or the page URL)
			// so we can use it to help with Realm resolution.
			Uri returnToApproximation;
			if (this.ReturnToUrl != null) {
				string returnToResolvedPath = this.ResolveUrl(this.ReturnToUrl);
				returnToApproximation = new Uri(requestContext.GetPublicFacingUrl(), returnToResolvedPath);
			} else {
				returnToApproximation = this.Page.Request.Url;
			}

			// Resolve the trust root, and swap out the scheme and port if necessary to match the
			// return_to URL, since this match is required by OpenID, and the consumer app
			// may be using HTTP at some times and HTTPS at others.
			UriBuilder realm = OpenIdUtilities.GetResolvedRealm(this.Page, this.RealmUrl, requestContext);
			realm.Scheme = returnToApproximation.Scheme;
			realm.Port = returnToApproximation.Port;

			// Initiate OpenID request
			// We use TryParse here to avoid throwing an exception which 
			// might slip through our validator control if it is disabled.
			Realm typedRealm = new Realm(realm);
			if (string.IsNullOrEmpty(this.ReturnToUrl)) {
				requests = await this.RelyingParty.CreateRequestsAsync(identifier, typedRealm, requestContext);
			} else {
				// Since the user actually gave us a return_to value,
				// the "approximation" is exactly what we want.
				requests = await this.RelyingParty.CreateRequestsAsync(identifier, typedRealm, returnToApproximation, cancellationToken);
			}

			// Some OPs may be listed multiple times (one with HTTPS and the other with HTTP, for example).
			// Since we're gathering OPs to try one after the other, just take the first choice of each OP
			// and don't try it multiple times.
			requests = requests.Distinct(DuplicateRequestedHostsComparer.Instance);

			// Configure each generated request.
			foreach (var req in requests) {
				if (this.IsPopupAppropriate(req)) {
					// Inform ourselves in return_to that we're in a popup.
					req.SetUntrustedCallbackArgument(UIPopupCallbackKey, "1");

					if (req.DiscoveryResult.IsExtensionSupported<UIRequest>()) {
						// Inform the OP that we'll be using a popup window consistent with the UI extension.
						// But beware that the extension MAY have already been added if we're using
						// the OpenIdAjaxRelyingParty class.
						if (!((AuthenticationRequest)req).Extensions.OfType<UIRequest>().Any()) {
							req.AddExtension(new UIRequest());
						}

						// Provide a hint for the client javascript about whether the OP supports the UI extension.
						// This is so the window can be made the correct size for the extension.
						// If the OP doesn't advertise support for the extension, the javascript will use
						// a bigger popup window.
						req.SetUntrustedCallbackArgument(PopupUISupportedJSHint, "1");
					}
				}

				// Add the extensions injected into the control.
				foreach (var extension in this.Extensions) {
					req.AddExtension(extension);
				}

				// Add state that needs to survive across the redirect, but at this point
				// only save those properties that are not expected to be changed by a
				// LoggingIn event handler.
				req.SetUntrustedCallbackArgument(ReturnToReceivingControlId, this.ClientID);

				// Apply the control's association preference to this auth request, but only if
				// it is less demanding (greater ordinal value) than the existing one.
				// That way, we protect against retrying an association that was already attempted.
				var authReq = (AuthenticationRequest)req;
				if (authReq.AssociationPreference < this.AssociationPreference) {
					authReq.AssociationPreference = this.AssociationPreference;
				}

				if (this.OnLoggingIn(req)) {
					// We save this property after firing OnLoggingIn so that the host page can
					// change its value and have that value saved.
					req.SetUntrustedCallbackArgument(UsePersistentCookieCallbackKey, this.UsePersistentCookie.ToString());

					results.Add(req);
				}
			}

			return results;
		}

		/// <summary>
		/// Gets the javascript to executee to redirect or POST an OpenID message to a remote party.
		/// </summary>
		/// <param name="requestRedirectingResponse">The request redirecting response.</param>
		/// <returns>
		/// The javascript that should execute.
		/// </returns>
		private string CreateGetOrPostAHrefValue(HttpResponseMessage requestRedirectingResponse) {
			Requires.NotNull(requestRedirectingResponse, "requestRedirectingResponse");

			Uri directUri = requestRedirectingResponse.GetDirectUriRequest();
			return "window.dnoa_internal.GetOrPost(" + MessagingUtilities.GetSafeJavascriptValue(directUri.AbsoluteUri) + ");";
		}

		/// <summary>
		/// Wires the return page to immediately display a popup window with the Provider in it.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		private async Task ScriptPopupWindowAsync(IAuthenticationRequest request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");
			RequiresEx.ValidState(this.RelyingParty != null);

			StringBuilder startupScript = new StringBuilder();

			// Add a callback function that the popup window can call on this, the
			// parent window, to pass back the authentication result.
			startupScript.AppendLine("window.dnoa_internal = {};");
			startupScript.AppendLine("window.dnoa_internal.processAuthorizationResult = function(uri) { window.location = uri; };");
			startupScript.AppendLine("window.dnoa_internal.popupWindow = function() {");
			startupScript.AppendFormat(
				@"\tvar openidPopup = {0}",
				await OpenId.RelyingParty.Extensions.UI.UIUtilities.GetWindowPopupScriptAsync(this.RelyingParty, request, "openidPopup", cancellationToken));
			startupScript.AppendLine("};");

			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "loginPopup", startupScript.ToString(), true);
		}

		/// <summary>
		/// Tries to preset the <see cref="Identifier"/> property based on a persistent
		/// cookie on the browser.
		/// </summary>
		/// <returns>
		/// A value indicating whether the <see cref="Identifier"/> property was
		/// successfully preset to some non-empty value.
		/// </returns>
		private bool TryPresetIdentifierWithCookie() {
			HttpCookie cookie = this.Page.Request.Cookies[PersistentIdentifierCookieName];
			if (cookie != null) {
				this.Identifier = Uri.UnescapeDataString(cookie.Value);
				return true;
			}

			return false;
		}
	}
}
