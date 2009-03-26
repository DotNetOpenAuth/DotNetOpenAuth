<%@ Application Language="C#" %>
<%@ Import Namespace="System.IO" %>

<script RunAt="server">
	void Application_Start(object sender, EventArgs e) {
		log4net.Config.XmlConfigurator.Configure();
		Logging.Logger.Info("Sample starting...");
	}

	void Application_End(object sender, EventArgs e) {
		Logging.Logger.Info("Sample shutting down...");
		// this would be automatic, but in partial trust scenarios it is not.
		log4net.LogManager.Shutdown();
	}

	void Application_Error(object sender, EventArgs e) {
		Logging.Logger.ErrorFormat("An unhandled exception was raised. Details follow: {0}", HttpContext.Current.Server.GetLastError());
	}

	void Application_BeginRequest(object sender, EventArgs e) {
		Logging.Logger.DebugFormat("Processing {0} on {1} ", Request.HttpMethod, stripQueryString(Request.Url));
		if (Request.QueryString.Count > 0) {
			Logging.Logger.DebugFormat("Querystring follows: \n{0}", ToString(Request.QueryString));
		}
		if (Request.Form.Count > 0) {
			Logging.Logger.DebugFormat("Posted form follows: \n{0}", ToString(Request.Form));
		}
	}

	void Session_Start(object sender, EventArgs e) {
		// Code that runs when a new session is started

	}

	void Session_End(object sender, EventArgs e) {
		// Code that runs when a session ends. 
		// Note: The Session_End event is raised only when the sessionstate mode
		// is set to InProc in the Web.config file. If session mode is set to StateServer 
		// or SQLServer, the event is not raised.

	}

	private static string ToString(NameValueCollection collection) {
		using (StringWriter sw = new StringWriter()) {
			foreach (string key in collection.Keys) {
				sw.WriteLine("{0} = '{1}'", key, collection[key]);
			}
			return sw.ToString();
		}
	}

	private static string stripQueryString(Uri uri) {
		UriBuilder builder = new UriBuilder(uri);
		builder.Query = null;
		return builder.ToString();
	}

	</script>
