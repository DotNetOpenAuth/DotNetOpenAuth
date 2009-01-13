<%@ Page Title="DotNetOpenAuth Service Provider Sample" Language="C#" MasterPageFile="~/MasterPage.master" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data.SqlClient" %>

<script runat="server">

	protected void createDatabaseButton_Click(object sender, EventArgs e) {
		string dbPath = Path.Combine(Server.MapPath(Request.ApplicationPath), "App_Data");
		if (!Directory.Exists(dbPath)) {
			Directory.CreateDirectory(dbPath);
		}
		string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ConnectionString.Replace("|DataDirectory|", dbPath);
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
				OpenIDFriendlyIdentifier = "=arnott",
				OpenIDClaimedIdentifier = "=!9B72.7DD1.50A9.5CCD",
				Age = 27,
				FullName = "Andrew Arnott",
				FavoriteSites = new System.Data.Linq.EntitySet<FavoriteSite> {
					new FavoriteSite { SiteUrl = "http://www.microsoft.com" },
					new FavoriteSite { SiteUrl = "http://www.google.com" },
				},
			});

			dc.SubmitChanges();
			databaseStatus.Visible = true;
		} catch (System.Data.SqlClient.SqlException ex) {
			foreach (System.Data.SqlClient.SqlError error in ex.Errors) {
				Response.Write(error.Message);
			}
		}
	}
</script>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:Button ID="createDatabaseButton" runat="server" Text="(Re)create Database" OnClick="createDatabaseButton_Click" />
	<asp:Label runat="server" ID="databaseStatus" EnableViewState="false" Text="Database recreated!"
		Visible="false" />
</asp:Content>
