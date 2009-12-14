//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using System.ServiceModel;
	using System.Web;

	public class Global : System.Web.HttpApplication {
		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		private static log4net.ILog logger = log4net.LogManager.GetLogger("WebFormsRelyingParty");

		public static log4net.ILog Logger {
			get { return logger; }
		}

		protected void Application_Start(object sender, EventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Web application starting...");
		}

		protected void Session_Start(object sender, EventArgs e) {
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
		}

		protected void Application_EndRequest(object sender, EventArgs e) {
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logger.Error("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());
		}

		protected void Session_End(object sender, EventArgs e) {
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Web application shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}
	}
}