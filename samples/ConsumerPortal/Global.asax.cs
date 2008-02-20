using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using DotNetOpenId;

namespace ConsumerPortal {
	public class Global : System.Web.HttpApplication {
		protected void Application_BeginRequest(Object sender, EventArgs e) {
			// System.Diagnostics.Debugger.Launch();

			#region " Trace "
			if (TraceUtil.Switch.TraceInfo) {
				HttpContext.Current.Response.Filter = new CustomTraceStream(Response.Filter);
				((CustomTraceStream)HttpContext.Current.Response.Filter).ClearSavedPageOutput();
				string basicTraceMessage = String.Format("Processing {0} on {1} ", Request.HttpMethod, Request.Url.ToString());
				TraceUtil.ConsumerTrace(basicTraceMessage);

				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ConsumerTrace("Querystring follows: \n" +
						TraceUtil.ToString(Request.QueryString));
					TraceUtil.ConsumerTrace("Posted form follows: \n" +
						TraceUtil.ToString(Request.Form));
				}

			}
			#endregion
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ConsumerTrace(string.Format("Is Forms Authenticated = {0}", (HttpContext.Current.User != null).ToString().ToUpper()));
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
				TraceUtil.ConsumerTrace("An anunhandled exception was raised. Details follow:");
				TraceUtil.ConsumerTrace(HttpContext.Current.Server.GetLastError().ToString());

			}
		}
	}
}