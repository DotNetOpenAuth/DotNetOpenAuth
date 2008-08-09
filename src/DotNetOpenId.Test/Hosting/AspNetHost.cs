using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Diagnostics;
using System.Net;
using DotNetOpenId.Provider;
using System.Threading;

namespace DotNetOpenId.Test.Hosting {
	/// <summary>
	/// Hosts a 'portable' version of the OpenIdProvider for testing itself and the
	/// RelyingParty against it.
	/// </summary>
	class AspNetHost : MarshalByRefObject {
		HttpHost httpHost;

		public AspNetHost() {
			httpHost = HttpHost.CreateHost(this);
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		public Uri BaseUri { get { return httpHost.BaseUri; } }

		public static AspNetHost CreateHost(string webDirectory) {
			AspNetHost host = (AspNetHost)ApplicationHost.
				CreateApplicationHost(typeof(AspNetHost), "/", webDirectory);
			return host;
		}

		public string ProcessRequest(string url) {
			return httpHost.ProcessRequest(url);
		}

		public string ProcessRequest(string url, string body) {
			return httpHost.ProcessRequest(url, body);
		}

		public void BeginProcessRequest(HttpListenerContext context) {
			ThreadPool.QueueUserWorkItem(state => { ProcessRequest(context); });
		}

		public void ProcessRequest(HttpListenerContext context) {
			try {
				using (TextWriter tw = new StreamWriter(context.Response.OutputStream)) {
					HttpRuntime.ProcessRequest(new TestingWorkerRequest(context, tw));
				}
			} catch (Exception ex) {
				TestSupport.Logger.Error("Exception in AspNetHost", ex);
				throw;
			}
		}

		public void CloseHttp() {
			httpHost.Dispose();
		}
	}
}
