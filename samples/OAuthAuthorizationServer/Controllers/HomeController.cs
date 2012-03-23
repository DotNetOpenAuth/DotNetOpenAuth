namespace OAuthAuthorizationServer.Controllers {
	using System.Configuration;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using System.Web.Mvc;
	using System.Web.Security;
	using OAuthAuthorizationServer.Code;

	[HandleError]
	public class HomeController : Controller {
		public ActionResult Index() {
			return View();
		}

		public ActionResult About() {
			return View();
		}

		[HttpPost]
		public ActionResult CreateDatabase() {
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

				// Add the necessary row for the sample client.
				dc.Clients.InsertOnSubmit(new Client {
					ClientIdentifier = "sampleconsumer",
					ClientSecret = "samplesecret",
					Name = "Some sample client",
				});
				dc.Clients.InsertOnSubmit(new Client {
					ClientIdentifier = "sampleImplicitConsumer",
					Name = "Some sample client used for implicit grants (no secret)",
					Callback = "http://localhost:59722/",
				});

				dc.SubmitChanges();

				// Force the user to log out because a new database warrants a new row in the users table, which we create
				// when the user logs in.
				FormsAuthentication.SignOut();
				ViewData["Success"] = true;
			} catch (SqlException ex) {
				ViewData["Error"] = string.Join("<br>", ex.Errors.OfType<SqlError>().Select(er => er.Message).ToArray());
			}

			return this.View();
		}
	}
}
