using System;
using ProviderPortal;

namespace ProviderCustomStore {
	public class Global : System.Web.HttpApplication {
		public Global() {
			// since this is a sample, and will often be used with localhost
			DotNetOpenId.UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
			URLRewriter.Process();
		}
	}
}