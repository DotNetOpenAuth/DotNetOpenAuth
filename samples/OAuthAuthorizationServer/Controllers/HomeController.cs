using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OAuthAuthorizationServer.Controllers {
	using System.Configuration;
	using System.Data.SqlClient;
	using System.IO;

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

				// Fill with sample data.
				dc.Clients.InsertOnSubmit(new Client {
					ClientIdentifier = "sampleconsumer",
					ClientSecret = "samplesecret",
					Name = "Some sample client",
				});
				dc.Users.InsertOnSubmit(new User {
					OpenIDFriendlyIdentifier = "=arnott",
					OpenIDClaimedIdentifier = "=!9B72.7DD1.50A9.5CCD",
				});

				dc.SubmitChanges();
				ViewData["Success"] = true;
			} catch (System.Data.SqlClient.SqlException ex) {
				ViewData["Error"] = string.Join("<br>", ex.Errors.OfType<SqlError>().Select(er => er.Message).ToArray());
			}

			return this.View();
		}
	}
}
