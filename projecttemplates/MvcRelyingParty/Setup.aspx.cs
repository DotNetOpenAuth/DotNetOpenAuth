namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;

	public partial class Setup : System.Web.UI.Page {
		private bool databaseCreated;

		protected void Page_Load(object sender, EventArgs e) {
			if (!Page.IsPostBack) {
				this.openidLogin.Focus();
			}
		}

		protected void openidLogin_LoggingIn(object sender, OpenIdEventArgs e) {
			// We don't actually want to log in... we just want the claimed identifier.
			e.Cancel = true;
			if (e.IsDirectedIdentity) {
				this.noOPIdentifierLabel.Visible = true;
			} else if (!this.databaseCreated) {
				Utilities.CreateDatabase(e.ClaimedIdentifier, this.openidLogin.Text, "MvcRelyingParty");
				this.MultiView1.ActiveViewIndex = 1;

				// indicate we have already created the database so that if the
				// identifier the user gave has multiple service endpoints,
				// we won't try to recreate the database as the next one is considered.
				this.databaseCreated = true;
			}
		}
	}
}
