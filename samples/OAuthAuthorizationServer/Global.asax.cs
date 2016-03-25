namespace OAuthAuthorizationServer {
	using System;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	using Code;

	using DotNetOpenAuth.Logging;

	/// <summary>
	/// The global MVC Application.
	/// </summary>
	/// <remarks>
	/// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	/// visit http://go.microsoft.com/?LinkId=9394801
	/// </remarks>
	public class MvcApplication : System.Web.HttpApplication {
		/// <summary>
		/// An application memory cache of recent log messages.
		/// </summary>
		public static StringBuilder LogMessages = new StringBuilder();

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		public static ILog Logger = LogProvider.GetLogger("DotNetOpenAuth.OAuthAuthorizationServer");

		public static DatabaseKeyNonceStore KeyNonceStore { get; set; }

		/// <summary>
		/// Gets the transaction-protected database connection for the current request.
		/// </summary>
		public static DataClassesDataContext DataContext {
			get {
				DataClassesDataContext dataContext = DataContextSimple;
				if (dataContext == null) {
					dataContext = new DataClassesDataContext();
					dataContext.Connection.Open();
					dataContext.Transaction = dataContext.Connection.BeginTransaction();
					DataContextSimple = dataContext;
				}

				return dataContext;
			}
		}

		public static User LoggedInUser {
			get { return DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == HttpContext.Current.User.Identity.Name); }
		}

		private static DataClassesDataContext DataContextSimple {
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

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }); // Parameter defaults
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			RegisterRoutes(RouteTable.Routes);

			KeyNonceStore = new DatabaseKeyNonceStore();

////			LogProvider.SetCurrentLogProvider(new ....)
			Logger.Info("Sample starting...");
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logger.ErrorException("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());

			// In the event of an unhandled exception, reverse any changes that were made to the database to avoid any partial database updates.
			var dataContext = DataContextSimple;
			if (dataContext != null) {
				dataContext.Transaction.Rollback();
				dataContext.Connection.Close();
				dataContext.Dispose();
				DataContextSimple = null;
			}
		}

		protected void Application_EndRequest(object sender, EventArgs e) {
			CommitAndCloseDatabaseIfNecessary();
		}

		private static void CommitAndCloseDatabaseIfNecessary() {
			var dataContext = DataContextSimple;
			if (dataContext != null) {
				dataContext.SubmitChanges();
				dataContext.Transaction.Commit();
				dataContext.Connection.Close();
			}
		}
	}
}