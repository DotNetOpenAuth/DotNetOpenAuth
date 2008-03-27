using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetOpenId.Provider {
	[DefaultEvent("AuthenticationChallenge")]
	[ToolboxData("<{0}:ProviderEndpoint runat='server' />")]
	public class ProviderEndpoint : Control {

		const string pendingAuthenticationRequestKey = "pendingAuthenticationRequestKey";
		public static IAuthenticationRequest PendingAuthenticationRequest {
			get { return HttpContext.Current.Session[pendingAuthenticationRequestKey] as CheckIdRequest; }
			set { HttpContext.Current.Session[pendingAuthenticationRequestKey] = value; }
		}

		const bool enabledDefault = true;
		const string enabledViewStateKey = "Enabled";
		[Category("Behavior")]
		[DefaultValue(enabledDefault)]
		public bool Enabled {
			get {
				return ViewState[enabledViewStateKey] == null ?
				enabledDefault : (bool)ViewState[enabledViewStateKey];
			}
			set { ViewState[enabledViewStateKey] = value; }
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Enabled) {
				OpenIdProvider openIDServer = new OpenIdProvider();

				// determine what incoming message was received
				if (openIDServer.Request != null) {
					// process the incoming message appropriately and send the response
					if (!openIDServer.Request.IsResponseReady) {
						var idrequest = (CheckIdRequest)openIDServer.Request;
						PendingAuthenticationRequest = idrequest;
						OnAuthenticationChallenge(idrequest);
					} else {
						PendingAuthenticationRequest = null;
					}
					if (openIDServer.Request.IsResponseReady) {
						openIDServer.Request.Response.Send();
						Page.Response.End();
						PendingAuthenticationRequest = null;
					}
				}
			}
		}

		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;
		protected virtual void OnAuthenticationChallenge(IAuthenticationRequest request) {
			var authenticationChallenge = AuthenticationChallenge;
			if (authenticationChallenge != null)
				authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
		}
	}
	public class AuthenticationChallengeEventArgs : EventArgs {
		internal AuthenticationChallengeEventArgs(IAuthenticationRequest request) {
			Request = request;
		}
		public IAuthenticationRequest Request { get; set; }
	}

}
