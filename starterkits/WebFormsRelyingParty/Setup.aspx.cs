namespace WebFormsOpenIdRelyingParty {
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
	using Microsoft.SqlServer.Management.Common;
	using Microsoft.SqlServer.Management.Smo;

	public partial class Setup : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!Page.IsPostBack) {
				openidLogin.Focus();
			}
		}

		protected void openidLogin_LoggingIn(object sender, OpenIdEventArgs e) {
			// We don't actually want to log in... we just want the claimed identifier.
			e.Cancel = true;
			if (e.IsDirectedIdentity) {
				noOPIdentifierLabel.Visible = true;
			} else {
				this.CreateDatabase(e.ClaimedIdentifier, openidLogin.Text);
				this.MultiView1.ActiveViewIndex = 1;
			}
		}

		private void CreateDatabase(Identifier claimedId, string friendlyId) {
			const string SqlFormat = @"
CREATE DATABASE [{0}] ON (NAME='{0}', FILENAME='{0}')
GO
USE ""{0}""
GO
{1}
EXEC [dbo].[AddUser] 'admin', 'admin', '{2}', '{3}'
GO
";
			string databasePath = HttpContext.Current.Server.MapPath("~/App_Data/Database.mdf");
			string schemaSql = File.ReadAllText(HttpContext.Current.Server.MapPath("~/Admin/CreateDatabase.sql"));
			string sql = string.Format(CultureInfo.InvariantCulture, SqlFormat, databasePath, schemaSql, claimedId, friendlyId);

			var serverConnection = new ServerConnection(".\\sqlexpress");
			try {
				serverConnection.ExecuteNonQuery(sql);
				var server = new Server(serverConnection);
				server.DetachDatabase(databasePath, true);
			} finally {
				serverConnection.Disconnect();
			}
		}
	}
}
