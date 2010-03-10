namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	//// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	//// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		/// <summary>
		/// The logger for this web site to use.
		/// </summary>
		private static log4net.ILog logger = log4net.LogManager.GetLogger("MvcRelyingParty");

		public static log4net.ILog Logger {
			get { return logger; }
		}

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = string.Empty });
			routes.MapRoute(
				"OpenIdDiscover",
				"Auth/Discover");
		}

		protected void Application_Start() {
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Web application starting...");
			RegisterRoutes(RouteTable.Routes);
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logger.Error("An unhandled exception occurred in ASP.NET processing for page " + HttpContext.Current.Request.Path, Server.GetLastError());
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Web application shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}
	}
}