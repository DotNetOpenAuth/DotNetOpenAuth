using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OAuthAuthorizationServer {
	using System.Text;

	using DotNetOpenAuth.OAuth2;
	using Code;

	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		/// <summary>
		/// An application memory cache of recent log messages.
		/// </summary>
		public static StringBuilder LogMessages = new StringBuilder();

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		public static log4net.ILog Logger = log4net.LogManager.GetLogger("DotNetOpenAuth.OAuthAuthorizationServer");

		public static DatabaseNonceStore NonceStore { get; set; }

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			RegisterRoutes(RouteTable.Routes);

			NonceStore = new DatabaseNonceStore();

			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Sample starting...");
		}

		private void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}

		private void Application_Error(object sender, EventArgs e) {
			Logger.Error("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());
		}

		private void Application_EndRequest(object sender, EventArgs e) {
			CommitAndCloseDatabaseIfNecessary();
		}

		/// <summary>
		/// Gets the transaction-protected database connection for the current request.
		/// </summary>
		public static DataClassesDataContext DataContext {
			get {
				DataClassesDataContext dataContext = dataContextSimple;
				if (dataContext == null) {
					dataContext = new DataClassesDataContext();
					dataContext.Connection.Open();
					dataContext.Transaction = dataContext.Connection.BeginTransaction();
					dataContextSimple = dataContext;
				}

				return dataContext;
			}
		}

		public static User LoggedInUser {
			get { return DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == HttpContext.Current.User.Identity.Name); }
		}

		private static DataClassesDataContext dataContextSimple {
			get {
				if (HttpContext.Current != null) {
					return HttpContext.Current.Items["DataContext"] as DataClassesDataContext;
				} else {
					throw new InvalidOperationException();
				}
			}

			set {
				if (HttpContext.Current != null) {
					HttpContext.Current.Items["DataContext"] = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		private static void CommitAndCloseDatabaseIfNecessary() {
			var dataContext = dataContextSimple;
			if (dataContext != null) {
				dataContext.SubmitChanges();
				dataContext.Transaction.Commit();
				dataContext.Connection.Close();
			}
		}
	}
}