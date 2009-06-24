using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Collections.Specialized;

public class Global : HttpApplication {
	public static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Global));

	public static StringBuilder LogMessages = new StringBuilder();

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
		Logger.Info("OSIS test site starting...");
		DotNetOpenAuth.OpenId.Behaviors.USGovernmentLevel1.PpidIdentifierProvider = new PpidProvider();
		DotNetOpenAuth.OpenId.Behaviors.USGovernmentLevel1.DisableSslRequirement = HttpContext.Current.Request.Url.Host == "localhost";
		// Always mark it as allowed, although we'll add the PAPE no-PII URI if we don't want any for this test.
		DotNetOpenAuth.OpenId.Behaviors.USGovernmentLevel1.AllowPersonallyIdentifiableInformation = true;
	}

	protected void Application_End(object sender, EventArgs e) {
		Logger.Info("OSIS test site shutting down...");

		// this would be automatic, but in partial trust scenarios it is not.
		log4net.LogManager.Shutdown();
	}

	protected void Application_BeginRequest(object sender, EventArgs e) {
		// System.Diagnostics.Debugger.Launch();
		Logger.DebugFormat("Processing {0} on {1} ", Request.HttpMethod, this.stripQueryString(Request.Url));
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
		Logger.ErrorFormat("An unhandled exception was raised. Details follow: {0}", HttpContext.Current.Server.GetLastError());
	}

	private string stripQueryString(Uri uri) {
		UriBuilder builder = new UriBuilder(uri);
		builder.Query = null;
		return builder.ToString();
	}
}
