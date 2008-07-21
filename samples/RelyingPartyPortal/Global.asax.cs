using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace ConsumerPortal {
	public class Global : System.Web.HttpApplication {
		public Global() {
			// since this is a sample, and will often be used with localhost
			DotNetOpenId.UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		public static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Global));

		protected void Application_Start(object sender, EventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Sample starting...");
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");
			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}

		string stripQueryString(Uri uri) {
			UriBuilder builder = new UriBuilder(uri);
			builder.Query = null;
			return builder.ToString();
		}

		protected void Application_BeginRequest(Object sender, EventArgs e) {
			// System.Diagnostics.Debugger.Launch();
			Logger.DebugFormat("Processing {0} on {1} ", Request.HttpMethod, stripQueryString(Request.Url));
			if (Request.QueryString.Count > 0)
				Logger.DebugFormat("Querystring follows: \n{0}", ToString(Request.QueryString));
			if (Request.Form.Count > 0)
				Logger.DebugFormat("Posted form follows: \n{0}", ToString(Request.Form));
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			Logger.DebugFormat("User {0} authenticated.", HttpContext.Current.User != null ? "IS" : "is NOT");
		}

		protected void Application_EndRequest(Object sender, EventArgs e) {
		}

		protected void Application_Error(Object sender, EventArgs e) {
			Logger.ErrorFormat("An unhandled exception was raised. Details follow: {0}",
				HttpContext.Current.Server.GetLastError());
		}

		public static string ToString(NameValueCollection collection) {
			using (StringWriter sw = new StringWriter()) {
				foreach (string key in collection.Keys) {
					if (key.StartsWith("__")) continue; // skip
					sw.WriteLine("{0} = '{1}'", key, collection[key]);
				}
				return sw.ToString();
			}
		}
	}
}