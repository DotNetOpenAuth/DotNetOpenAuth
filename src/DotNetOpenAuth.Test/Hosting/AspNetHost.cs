namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.IO;
	using System.Net;
	using System.Threading;
	using System.Web;
	using System.Web.Hosting;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Test.OpenId;

	/// <summary>
	/// Hosts a 'portable' version of the OpenIdProvider for testing itself and the
	/// RelyingParty against it.
	/// </summary>
	class AspNetHost : MarshalByRefObject {
		HttpHost httpHost;

		public AspNetHost() {
			httpHost = HttpHost.CreateHost(this);
			////if (!UntrustedWebRequestHandler.WhitelistHosts.Contains("localhost"))
			////    UntrustedWebRequestHandler.WhitelistHosts.Add("localhost");
		}

		public Uri BaseUri {
			get { return httpHost.BaseUri; }
		}

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
				Logger.Error("Exception in AspNetHost", ex);
				throw;
			}
		}

		public void CloseHttp() {
			httpHost.Dispose();
		}
	}
}
