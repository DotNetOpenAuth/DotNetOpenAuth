using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Diagnostics;

namespace DotNetOpenId.Test.Hosting {
	/// <summary>
	/// Hosts a 'portable' version of the OpenIdProvider for testing itself and the
	/// Consumer against it.
	/// </summary>
	class AspNetHost : MarshalByRefObject {
		public static AspNetHost CreateHost(string webDirectory) {
			AspNetHost host = (AspNetHost)ApplicationHost.
				CreateApplicationHost(typeof(AspNetHost), "/", webDirectory);
			return host;
		}

		public string ProcessRequest(string page) {
			return ProcessRequest(page, string.Empty);
		}

		public string ProcessRequest(string page, string query) {
			return ProcessRequest(page, query, null);
		}

		public string ProcessRequest(string page, string query, string body) {
			Trace.TraceInformation("Submitting ASP.NET request: {0}?{1}{2}{3}",
				page, query, Environment.NewLine, body);
			using (TextWriter tw = new StringWriter()) {
				Stream bodyStream = body != null ? new MemoryStream(Encoding.ASCII.GetBytes(body)) : null;
				ProcessRequest(page, query, bodyStream, tw);
				Trace.TraceInformation("Response:{0}{1}", Environment.NewLine, tw);
				return tw.ToString();
			}
		}

		public void ProcessRequest(string page, string query, Stream body, TextWriter response) {
			ProcessRequest(new TestingWorkerRequest(page, query, body, response));
		}

		public void ProcessRequest(HttpWorkerRequest wr) {
			HttpRuntime.ProcessRequest(wr);
		}
	}
}
