//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System.Web.Http;
	using Owin;

	public class Startup {
		// This code configures Web API. The Startup class is specified as a type
		// parameter in the WebApp.Start method.
		public void Configuration(IAppBuilder appBuilder) {
			// Configure Web API for self-host. 
			HttpConfiguration config = new HttpConfiguration();
			config.Routes.MapHttpRoute("default", "{controller}/{id}", new { controller = "Home", id = RouteParameter.Optional });
			appBuilder.UseWebApi(config);
		}
	}
}