﻿namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public partial class Global : HttpApplication {
		protected void Application_Start(object sender, EventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
			Logging.Logger.Info("Sample starting...");
		}

		protected void Application_End(object sender, EventArgs e) {
			Logging.Logger.Info("Sample shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logging.Logger.ErrorFormat("An unhandled exception was raised. Details follow: {0}", HttpContext.Current.Server.GetLastError());
		}

		protected void Session_Start(object sender, EventArgs e) {
			// Code that runs when a new session is started
		}

		protected void Session_End(object sender, EventArgs e) {
			// Code that runs when a session ends. 
			// Note: The Session_End event is raised only when the sessionstate mode
			// is set to InProc in the Web.config file. If session mode is set to StateServer 
			// or SQLServer, the event is not raised.
		}
	}
}