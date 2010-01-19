namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security;
	using System.Security.Permissions;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class loginGoogleApps : System.Web.UI.Page {
		private static readonly HostMetaDiscoveryService GoogleAppsDiscovery = new HostMetaDiscoveryService {
			UseGoogleHostedHostMeta = true,
		};

		private static readonly OpenIdRelyingParty relyingParty;

		static loginGoogleApps() {
			relyingParty = new OpenIdRelyingParty();

			// We don't necessarily HAVE to clear the other discovery services, but
			// because host-meta discovery (particularly with Google) can cause ambiguity
			// in knowing which discovered endpoints are authoritative.  Because of the
			// extra security concerns it's a good idea to have a separate box 
			relyingParty.DiscoveryServices.Clear();
			relyingParty.DiscoveryServices.Insert(0, GoogleAppsDiscovery); // it should be first if we don't clear the other discovery services
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.OpenIdLogin1.RelyingParty = relyingParty;
			this.OpenIdLogin1.Focus();

			this.fullTrustRequired.Visible = IsPartiallyTrusted();
		}

		protected void OpenIdLogin1_LoggedIn(object sender, OpenIdEventArgs e) {
			State.FriendlyLoginName = e.Response.FriendlyIdentifierForDisplay;
		}

		private static bool IsPartiallyTrusted() {
			try {
				new SecurityPermission(PermissionState.Unrestricted).Demand();
				return false;
			} catch (SecurityException) {
				return true;
			}
		}
	}
}
