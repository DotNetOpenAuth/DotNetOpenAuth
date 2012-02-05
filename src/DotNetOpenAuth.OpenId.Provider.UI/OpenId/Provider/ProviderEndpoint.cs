//-----------------------------------------------------------------------
// <copyright file="ProviderEndpoint.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// An OpenID Provider control that automatically responds to certain
	/// automated OpenID messages, and routes authentication requests to
	/// custom code via an event handler.
	/// </summary>
	[DefaultEvent("AuthenticationChallenge")]
	[ToolboxData("<{0}:ProviderEndpoint runat='server' />")]
	public class ProviderEndpoint : Control {
		/// <summary>
		/// The key used to store the pending authentication request in the ASP.NET session.
		/// </summary>
		private const string PendingRequestKey = "pendingRequest";

		/// <summary>
		/// The default value for the <see cref="Enabled"/> property.
		/// </summary>
		private const bool EnabledDefault = true;

		/// <summary>
		/// The view state key in which to store the value of the <see cref="Enabled"/> property.
		/// </summary>
		private const string EnabledViewStateKey = "Enabled";

		/// <summary>
		/// Backing field for the <see cref="Provider"/> property.
		/// </summary>
		private static OpenIdProvider provider;

		/// <summary>
		/// The lock that must be obtained when initializing the provider field.
		/// </summary>
		private static object providerInitializerLock = new object();

		/// <summary>
		/// Fired when an incoming OpenID request is an authentication challenge
		/// that must be responded to by the Provider web site according to its
		/// own user database and policies.
		/// </summary>
		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;

		/// <summary>
		/// Fired when an incoming OpenID message carries extension requests
		/// but is not regarding any OpenID identifier.
		/// </summary>
		public event EventHandler<AnonymousRequestEventArgs> AnonymousRequest;

		/// <summary>
		/// Gets or sets the <see cref="OpenIdProvider"/> instance to use for all instances of this control.
		/// </summary>
		/// <value>The default value is an <see cref="OpenIdProvider"/> instance initialized according to the web.config file.</value>
		public static OpenIdProvider Provider {
			get {
				Contract.Ensures(Contract.Result<OpenIdProvider>() != null);
				if (provider == null) {
					lock (providerInitializerLock) {
						if (provider == null) {
							provider = CreateProvider();
						}
					}
				}

				return provider;
			}

			set {
				Requires.NotNull(value, "value");
				provider = value;
			}
		}

		/// <summary>
		/// Gets or sets an incoming OpenID authentication request that has not yet been responded to.
		/// </summary>
		/// <remarks>
		/// This request is stored in the ASP.NET Session state, so it will survive across
		/// redirects, postbacks, and transfers.  This allows you to authenticate the user
		/// yourself, and confirm his/her desire to authenticate to the relying party site
		/// before responding to the relying party's authentication request.
		/// </remarks>
		public static IAuthenticationRequest PendingAuthenticationRequest {
			get {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				Contract.Ensures(Contract.Result<IAuthenticationRequest>() == null || PendingRequest != null);
				return HttpContext.Current.Session[PendingRequestKey] as IAuthenticationRequest;
			}

			set {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				HttpContext.Current.Session[PendingRequestKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets an incoming OpenID anonymous request that has not yet been responded to.
		/// </summary>
		/// <remarks>
		/// This request is stored in the ASP.NET Session state, so it will survive across
		/// redirects, postbacks, and transfers.  This allows you to authenticate the user
		/// yourself, and confirm his/her desire to provide data to the relying party site
		/// before responding to the relying party's request.
		/// </remarks>
		public static IAnonymousRequest PendingAnonymousRequest {
			get {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				Contract.Ensures(Contract.Result<IAnonymousRequest>() == null || PendingRequest != null);
				return HttpContext.Current.Session[PendingRequestKey] as IAnonymousRequest;
			}

			set {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				HttpContext.Current.Session[PendingRequestKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets an incoming OpenID request that has not yet been responded to.
		/// </summary>
		/// <remarks>
		/// This request is stored in the ASP.NET Session state, so it will survive across
		/// redirects, postbacks, and transfers.  This allows you to authenticate the user
		/// yourself, and confirm his/her desire to provide data to the relying party site
		/// before responding to the relying party's request.
		/// </remarks>
		public static IHostProcessedRequest PendingRequest {
			get {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				return HttpContext.Current.Session[PendingRequestKey] as IHostProcessedRequest;
			}

			set {
				Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
				Requires.ValidState(HttpContext.Current.Session != null, MessagingStrings.SessionRequired);
				HttpContext.Current.Session[PendingRequestKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not this control should 
		/// be listening for and responding to incoming OpenID requests.
		/// </summary>
		[Category("Behavior"), DefaultValue(EnabledDefault)]
		public bool Enabled {
			get {
				return ViewState[EnabledViewStateKey] == null ?
				EnabledDefault : (bool)ViewState[EnabledViewStateKey];
			}

			set {
				ViewState[EnabledViewStateKey] = value;
			}
		}

		/// <summary>
		/// Sends the response for the <see cref="PendingAuthenticationRequest"/> and clears the property.
		/// </summary>
		public static void SendResponse() {
			var pendingRequest = PendingRequest;
			PendingRequest = null;
			Provider.SendResponse(pendingRequest);
		}

		/// <summary>
		/// Checks for incoming OpenID requests, responds to ones it can
		/// respond to without policy checks, and fires events for custom
		/// handling of the ones it cannot decide on automatically.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			// There is the unusual scenario that this control is hosted by
			// an ASP.NET web page that has other UI on it to that the user
			// might see, including controls that cause a postback to occur.
			// We definitely want to ignore postbacks, since any openid messages
			// they contain will be old.
			if (this.Enabled && !this.Page.IsPostBack) {
				// Use the explicitly given state store on this control if there is one.  
				// Then try the configuration file specified one.  Finally, use the default
				// in-memory one that's built into OpenIdProvider.
				// determine what incoming message was received
				IRequest request = Provider.GetRequest();
				if (request != null) {
					PendingRequest = null;

					// process the incoming message appropriately and send the response
					IAuthenticationRequest idrequest;
					IAnonymousRequest anonRequest;
					if ((idrequest = request as IAuthenticationRequest) != null) {
						PendingAuthenticationRequest = idrequest;
						this.OnAuthenticationChallenge(idrequest);
					} else if ((anonRequest = request as IAnonymousRequest) != null) {
						PendingAnonymousRequest = anonRequest;
						if (!this.OnAnonymousRequest(anonRequest)) {
							// This is a feature not supported by the OP, so
							// go ahead and set disapproved so we can send a response.
							Logger.OpenId.Warn("An incoming anonymous OpenID request message was detected, but the ProviderEndpoint.AnonymousRequest event is not handled, so returning cancellation message to relying party.");
							anonRequest.IsApproved = false;
						}
					}
					if (request.IsResponseReady) {
						PendingAuthenticationRequest = null;
						Provider.SendResponse(request);
					}
				}
			}
		}

		/// <summary>
		/// Fires the <see cref="AuthenticationChallenge"/> event.
		/// </summary>
		/// <param name="request">The request to include in the event args.</param>
		protected virtual void OnAuthenticationChallenge(IAuthenticationRequest request) {
			var authenticationChallenge = this.AuthenticationChallenge;
			if (authenticationChallenge != null) {
				authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
			}
		}

		/// <summary>
		/// Fires the <see cref="AnonymousRequest"/> event.
		/// </summary>
		/// <param name="request">The request to include in the event args.</param>
		/// <returns><c>true</c> if there were any anonymous request handlers.</returns>
		protected virtual bool OnAnonymousRequest(IAnonymousRequest request) {
			var anonymousRequest = this.AnonymousRequest;
			if (anonymousRequest != null) {
				anonymousRequest(this, new AnonymousRequestEventArgs(request));
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Creates the default OpenIdProvider to use.
		/// </summary>
		/// <returns>The new instance of OpenIdProvider.</returns>
		private static OpenIdProvider CreateProvider() {
			Contract.Ensures(Contract.Result<OpenIdProvider>() != null);
			return new OpenIdProvider(OpenIdElement.Configuration.Provider.ApplicationStore.CreateInstance(OpenIdProvider.HttpApplicationStore));
		}
	}
}
