using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.IO;

namespace DotNetOpenId.Test.Hosting {
	/// <summary>
	/// Hosts a 'portable' version of the OpenIdProvider for testing itself and the
	/// Consumer against it.
	/// </summary>
	class AspNetHost : MarshalByRefObject {
		public static AspNetHost CreateHost(string webDirectory) {
			AspNetHost host = (AspNetHost)System.Web.Hosting.ApplicationHost.
				CreateApplicationHost(typeof(AspNetHost), "/", webDirectory);
			return host;
		}

		public void ProcessRequest(string page, string query, Stream body, TextWriter responseWriter) {
			ProcessRequest(new TestingWorkerRequest(page, query, body, responseWriter));
		}

		public void ProcessRequest(HttpWorkerRequest wr) {
			HttpRuntime.ProcessRequest(wr);
		}
	}
}
