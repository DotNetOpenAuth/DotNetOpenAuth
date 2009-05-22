namespace OpenIdRelyingPartyMvc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	//// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	//// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default",                                              // Route name
				"{controller}/{action}/{id}",                           // URL with parameters
				new { controller = "Home", action = "Index", id = string.Empty });  // Parameter defaults

			routes.MapRoute(
				"Root",
				string.Empty,
				new { controller = "Home", action = "Index", id = string.Empty });
		}

		protected void Application_Start(object sender, EventArgs e) {
			RegisterRoutes(RouteTable.Routes);
		}
	}
}