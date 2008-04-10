using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// An OpenID Provider control that automatically responds to certain
	/// automated OpenID messages, and routes authentication requests to
	/// custom code via an event handler.
	/// </summary>
	[DefaultEvent("AuthenticationChallenge")]
	[ToolboxData("<{0}:ProviderEndpoint runat='server' />")]
	public class ProviderEndpoint : Control {

		const string pendingAuthenticationRequestKey = "pendingAuthenticationRequestKey";
		/// <summary>
		/// An incoming OpenID authentication request that has not yet been responded to.
		/// </summary>
		/// <remarks>
		/// This request is stored in the ASP.NET Session state, so it will survive across
		/// redirects, postbacks, and transfers.  This allows you to authenticate the user
		/// yourself, and confirm his/her desire to authenticate to the relying party site
		/// before responding to the relying party's authentication request.
		/// </remarks>
		public static IAuthenticationRequest PendingAuthenticationRequest {
			get { return HttpContext.Current.Session[pendingAuthenticationRequestKey] as CheckIdRequest; }
			set { HttpContext.Current.Session[pendingAuthenticationRequestKey] = value; }
		}

		const bool enabledDefault = true;
		const string enabledViewStateKey = "Enabled";
		/// <summary>
		/// Whether or not this control should be listening for and responding
		/// to incoming OpenID requests.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(enabledDefault)]
		public bool Enabled {
			get {
				return ViewState[enabledViewStateKey] == null ?
				enabledDefault : (bool)ViewState[enabledViewStateKey];
			}
			set { ViewState[enabledViewStateKey] = value; }
		}

		/// <summary>
		/// Checks for incoming OpenID requests, responds to ones it can
		/// respond to without policy checks, and fires events for custom
		/// handling of the ones it cannot decide on automatically.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Enabled) {
				OpenIdProvider provider = new OpenIdProvider();

				// determine what incoming message was received
				if (provider.Request != null) {
					// process the incoming message appropriately and send the response
					if (!provider.Request.IsResponseReady) {
						var idrequest = (CheckIdRequest)provider.Request;
						PendingAuthenticationRequest = idrequest;
						OnAuthenticationChallenge(idrequest);
					} else {
						PendingAuthenticationRequest = null;
					}
					if (provider.Request.IsResponseReady) {
						provider.Request.Response.Send();
						Page.Response.End();
						PendingAuthenticationRequest = null;
					}
				}
			}
		}

		/// <summary>
		/// Fired when an incoming OpenID request is an authentication challenge
		/// that must be responded to by the Provider web site according to its
		/// own user database and policies.
		/// </summary>
		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;
		/// <summary>
		/// Fires the <see cref="AuthenticationChallenge"/> event.
		/// </summary>
		protected virtual void OnAuthenticationChallenge(IAuthenticationRequest request) {
			var authenticationChallenge = AuthenticationChallenge;
			if (authenticationChallenge != null)
				authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
		}
	}

	/// <summary>
	/// The event arguments that include details of the incoming request.
	/// </summary>
	public class AuthenticationChallengeEventArgs : EventArgs {
		internal AuthenticationChallengeEventArgs(IAuthenticationRequest request) {
			Request = request;
		}
		/// <summary>
		/// The incoming authentication request.
		/// </summary>
		public IAuthenticationRequest Request { get; set; }
	}

}
