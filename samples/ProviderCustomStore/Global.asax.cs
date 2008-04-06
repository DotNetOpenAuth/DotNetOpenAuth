using System;
using ProviderPortal;

namespace ProviderCustomStore {
	public class Global : System.Web.HttpApplication {
		protected void Application_BeginRequest(object sender, EventArgs e) {
			URLRewriter.Process();
		}
	}
}