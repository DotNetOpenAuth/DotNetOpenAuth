namespace OAuthServiceProvider {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Linq;
	using System.Web;
	using OAuthServiceProvider.Code;

	public partial class _Default : System.Web.UI.Page {
		protected void createDatabaseButton_Click(object sender, EventArgs e) {
			string databasePath = Path.Combine(Server.MapPath(Request.ApplicationPath), "App_Data");
			if (!Directory.Exists(databasePath)) {
				Directory.CreateDirectory(databasePath);
			}
			string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ConnectionString.Replace("|DataDirectory|", databasePath);
			var dc = new DataClassesDataContext(connectionString);
			if (dc.DatabaseExists()) {
				dc.DeleteDatabase();
			}
			try {
				dc.CreateDatabase();

				// Fill with sample data.
				dc.OAuthConsumers.InsertOnSubmit(new OAuthConsumer {
					ConsumerKey = "sampleconsumer",
					ConsumerSecret = "samplesecret",
				});
				dc.Users.InsertOnSubmit(new User {
					OpenIDFriendlyIdentifier = "http://blog.nerdbank.net/",
					OpenIDClaimedIdentifier = "http://blog.nerdbank.net/",
					Age = 27,
					FullName = "Andrew Arnott",
					FavoriteSites = new System.Data.Linq.EntitySet<FavoriteSite> {
					new FavoriteSite { SiteUrl = "http://www.microsoft.com" },
					new FavoriteSite { SiteUrl = "http://www.google.com" },
				},
				});

				dc.SubmitChanges();
				this.databaseStatus.Visible = true;
			} catch (System.Data.SqlClient.SqlException ex) {
				foreach (System.Data.SqlClient.SqlError error in ex.Errors) {
					Response.Write(error.Message);
				}
			}
		}
	}
}