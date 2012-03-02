//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms {
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Web;
	using OpenIdProviderWebForms.Code;

	public class Global : System.Web.HttpApplication {
		public static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Global));

		internal static StringBuilder LogMessages = new StringBuilder();

		public static string ToString(NameValueCollection collection) {
			using (StringWriter sw = new StringWriter()) {
				foreach (string key in collection.Keys) {
					sw.WriteLine("{0} = '{1}'", key, collection[key]);
				}
				return sw.ToString();
			}
		}

		protected void Application_Start(object sender, EventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Sample starting...");
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
			Logger.DebugFormat("Processing {0} on {1} ", this.Request.HttpMethod, this.stripQueryString(this.Request.Url));
			if (Request.QueryString.Count > 0) {
				Logger.DebugFormat("Querystring follows: \n{0}", ToString(Request.QueryString));
			}
			if (Request.Form.Count > 0) {
				Logger.DebugFormat("Posted form follows: \n{0}", ToString(Request.Form));
			}
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {
			Logger.DebugFormat("User {0} authenticated.", HttpContext.Current.User != null ? "IS" : "is NOT");
		}

		protected void Application_EndRequest(object sender, EventArgs e) {
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logger.ErrorFormat(
				"An unhandled exception was raised. Details follow: {0}",
				HttpContext.Current.Server.GetLastError());
		}

		private string stripQueryString(Uri uri) {
			UriBuilder builder = new UriBuilder(uri);
			builder.Query = null;
			return builder.ToString();
		}
	}
}