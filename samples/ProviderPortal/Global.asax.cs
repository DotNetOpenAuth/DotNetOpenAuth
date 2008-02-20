using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using DotNetOpenId;

namespace ProviderPortal {
	public class Global : System.Web.HttpApplication {

		protected void Application_BeginRequest(Object sender, EventArgs e) {
			/*
			 * The URLRewriter was taken from http://www.codeproject.com/aspnet/URLRewriter.asp and modified slightly.
			 * It will read the config section called 'urlrewrites' from web.config and process each rule 
			 * The rules are set of url transformations defined using regular expressions with support for substitutions (the ability to extract regex-matched portions of a string).
			 * There is only one rule currenty defined. It rewrites urls like: user/john ->user.aspx?username=john
			 */
			// System.Diagnostics.Debugger.Launch();

			#region " Trace "
			if (TraceUtil.Switch.TraceInfo) {
				HttpContext.Current.Response.Filter = new CustomTraceStream(Response.Filter);
				((CustomTraceStream)HttpContext.Current.Response.Filter).ClearSavedPageOutput();

				string basicTraceMessage = String.Format("Processing {0} on {1} ", Request.HttpMethod, Request.Url.ToString());
				TraceUtil.ProviderTrace(basicTraceMessage);

				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ProviderTrace("Querystring follows: \n" +
						TraceUtil.ToString(Request.QueryString));
					TraceUtil.ProviderTrace("Posted form follows: \n" +
						TraceUtil.ToString(Request.Form));
				}

			}
			#endregion

			URLRewriter.Process();
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ProviderTrace(string.Format("Is Forms Authenticated = {0}", (HttpContext.Current.User != null).ToString().ToUpper()));
			}
		}


		protected void Application_EndRequest(Object sender, EventArgs e) {
			if (TraceUtil.TracePageOutput) {
				// taking this out for now because it makes the logs very big 
				TraceUtil.ProviderTrace("Entire page response follows:");
				TraceUtil.ProviderTrace(((CustomTraceStream)HttpContext.Current.Response.Filter).SavedPageOutput);
			}
		}

		protected void Application_Error(Object sender, EventArgs e) {
			if (TraceUtil.Switch.TraceError) {
				TraceUtil.ProviderTrace("An anunhandled exception was raised. Details follow:");
				TraceUtil.ProviderTrace(HttpContext.Current.Server.GetLastError().ToString());

			}
		}

	}
}