namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.IO;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth;
	using OpenIdRelyingPartyWebForms.Code;

	public class Global : HttpApplication {
		public static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Global));

		internal static StringBuilder LogMessages = new StringBuilder();

		internal static WebConsumerOpenIdRelyingParty GoogleWebConsumer {
			get {
				var googleWebConsumer = (WebConsumerOpenIdRelyingParty)HttpContext.Current.Application["GoogleWebConsumer"];
				if (googleWebConsumer == null) {
					googleWebConsumer = new WebConsumerOpenIdRelyingParty(GoogleConsumer.ServiceDescription, GoogleTokenManager);
					HttpContext.Current.Application["GoogleWebConsumer"] = googleWebConsumer;
				}

				return googleWebConsumer;
			}
		}

		internal static InMemoryTokenManager GoogleTokenManager {
			get {
				var tokenManager = (InMemoryTokenManager)HttpContext.Current.Application["GoogleTokenManager"];
				if (tokenManager == null) {
					string consumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
					string consumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];
					if (!string.IsNullOrEmpty(consumerKey)) {
						tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
						HttpContext.Current.Application["GoogleTokenManager"] = tokenManager;
					}
				}

				return tokenManager;
			}
		}

		internal static InMemoryTokenManager OwnSampleOPHybridTokenManager {
			get {
				var tokenManager = (InMemoryTokenManager)HttpContext.Current.Application["OwnSampleOPHybridTokenManager"];
				if (tokenManager == null) {
					string consumerKey = new Uri(HttpContext.Current.Request.Url, HttpContext.Current.Request.ApplicationPath).AbsoluteUri;
					string consumerSecret = "some crazy secret";
					tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
					HttpContext.Current.Application["OwnSampleOPHybridTokenManager"] = tokenManager;
				}

				return tokenManager;
			}
		}

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
			// System.Diagnostics.Debugger.Launch();
			Logger.DebugFormat("Processing {0} on {1} ", Request.HttpMethod, stripQueryString(Request.Url));
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

		private static string stripQueryString(Uri uri) {
			UriBuilder builder = new UriBuilder(uri);
			builder.Query = null;
			return builder.ToString();
		}
	}
}