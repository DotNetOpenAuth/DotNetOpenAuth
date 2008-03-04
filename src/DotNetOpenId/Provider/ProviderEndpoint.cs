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
	public class ProviderEndpoint : WebControl {

		const string pendingAuthenticationRequestKey = "pendingAuthenticationRequestKey";
		public static CheckIdRequest PendingAuthenticationRequest {
			get { return HttpContext.Current.Session[pendingAuthenticationRequestKey] as CheckIdRequest; }
			set { HttpContext.Current.Session[pendingAuthenticationRequestKey] = value; }
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

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

		public class AuthenticationChallengeEventArgs : EventArgs {
			internal AuthenticationChallengeEventArgs(CheckIdRequest request) {
				Request = request;
			}
			public CheckIdRequest Request { get; set; }
		}

		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;
		protected virtual void OnAuthenticationChallenge(CheckIdRequest request) {
			var authenticationChallenge = AuthenticationChallenge;
			if (authenticationChallenge != null)
				authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
		}
	}
}
