//-----------------------------------------------------------------------
// <copyright file="ProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Configuration;
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
		private const string PendingAuthenticationRequestKey = "pendingAuthenticationRequestKey";

		/// <summary>
		/// The default value for the <see cref="Enabled"/> property.
		/// </summary>
		private const bool EnabledDefault = true;

		/// <summary>
		/// The view state key in which to store the value of the <see cref="Enabled"/> property.
		/// </summary>
		private const string EnabledViewStateKey = "Enabled";

		/// <summary>
		/// Fired when an incoming OpenID request is an authentication challenge
		/// that must be responded to by the Provider web site according to its
		/// own user database and policies.
		/// </summary>
		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;

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
			get { return HttpContext.Current.Session[PendingAuthenticationRequestKey] as IAuthenticationRequest; }
			set { HttpContext.Current.Session[PendingAuthenticationRequestKey] = value; }
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
		/// Gets or sets a custom application store to use.  Null to use the default.
		/// </summary>
		/// <remarks>
		/// If set, this property must be set in each Page Load event
		/// as it is not persisted across postbacks.
		/// </remarks>
		public IProviderApplicationStore CustomApplicationStore { get; set; }

		/// <summary>
		/// Checks for incoming OpenID requests, responds to ones it can
		/// respond to without policy checks, and fires events for custom
		/// handling of the ones it cannot decide on automatically.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (this.Enabled) {
				// Use the explicitly given state store on this control if there is one.  
				// Then try the configuration file specified one.  Finally, use the default
				// in-memory one that's built into OpenIdProvider.
				using (OpenIdProvider provider = new OpenIdProvider(this.CustomApplicationStore ?? DotNetOpenAuthSection.Configuration.OpenId.Provider.ApplicationStore.CreateInstance(OpenIdProvider.HttpApplicationStore))) {
					// determine what incoming message was received
					IRequest request = provider.GetRequest();
					if (request != null) {
						// process the incoming message appropriately and send the response
						if (!request.IsResponseReady) {
							var idrequest = (IAuthenticationRequest)request;
							PendingAuthenticationRequest = idrequest;
							this.OnAuthenticationChallenge(idrequest);
						} else {
							PendingAuthenticationRequest = null;
						}
						if (request.IsResponseReady) {
							request.Response.Send();
							Page.Response.End();
							PendingAuthenticationRequest = null;
						}
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
	}
}
