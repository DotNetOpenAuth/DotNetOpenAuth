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
		public static IAuthenticationRequest PendingAuthenticationRequest {
			get { return HttpContext.Current.Session[pendingAuthenticationRequestKey] as CheckIdRequest; }
			set { HttpContext.Current.Session[pendingAuthenticationRequestKey] = value; }
		}

		#region Properties to hide
		[Browsable(false), Bindable(false)]
		public override bool Visible {
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string AccessKey {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string CssClass {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableViewState {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override short TabIndex {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle {
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override FontInfo Font {
			get { return null; }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Height {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Width {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string ToolTip {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

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

		public class AuthenticationChallengeEventArgs : EventArgs {
			internal AuthenticationChallengeEventArgs(IAuthenticationRequest request) {
				Request = request;
			}
			public IAuthenticationRequest Request { get; set; }
		}

		public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;
		protected virtual void OnAuthenticationChallenge(IAuthenticationRequest request) {
			var authenticationChallenge = AuthenticationChallenge;
			if (authenticationChallenge != null)
				authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
		}
	}
}
