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
	class Host : MarshalByRefObject {
		public static Host CreateHost(string webDirectory) {
			Host host = (Host)System.Web.Hosting.ApplicationHost.
				CreateApplicationHost(typeof(Host), "/", webDirectory);
			return host;
		}

		public void ProcessRequest(string page, string query, string body, TextWriter responseWriter) {
			ProcessRequest(new TestingWorkerRequest(page, query, body, responseWriter));
		}

		public void ProcessRequest(HttpWorkerRequest wr) {
			System.Web.HttpRuntime.ProcessRequest(wr);
		}
	}
}
