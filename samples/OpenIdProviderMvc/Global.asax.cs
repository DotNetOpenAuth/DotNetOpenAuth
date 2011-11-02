namespace OpenIdProviderMvc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	/// <summary>
	/// The global MVC application state and manager.
	/// </summary>
	/// <remarks>
	/// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	/// visit http://go.microsoft.com/?LinkId=9394801
	/// </remarks>
	public class MvcApplication : System.Web.HttpApplication {
		private static object behaviorInitializationSyncObject = new object();

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"User identities",
				"user/{id}/{action}",
				new { controller = "User", action = "Identity", id = string.Empty, anon = false });
			routes.MapRoute(
				"PPID identifiers",
				"anon",
				new { controller = "User", action = "Identity", id = string.Empty, anon = true });
			routes.MapRoute(
				"Default",                                              // Route name
				"{controller}/{action}/{id}",                           // URL with parameters
				new { controller = "Home", action = "Index", id = string.Empty }); // Parameter defaults
		}

		protected void Application_Start() {
			RegisterRoutes(RouteTable.Routes);
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
			InitializeBehaviors();
		}

		private static void InitializeBehaviors() {
			if (DotNetOpenAuth.OpenId.Provider.Behaviors.PpidGeneration.PpidIdentifierProvider == null) {
				lock (behaviorInitializationSyncObject) {
					if (DotNetOpenAuth.OpenId.Provider.Behaviors.PpidGeneration.PpidIdentifierProvider == null) {
						DotNetOpenAuth.OpenId.Provider.Behaviors.PpidGeneration.PpidIdentifierProvider = new Code.AnonymousIdentifierProvider();
						DotNetOpenAuth.OpenId.Provider.Behaviors.GsaIcamProfile.PpidIdentifierProvider = new Code.AnonymousIdentifierProvider();
					}
				}
			}
		}
	}
}