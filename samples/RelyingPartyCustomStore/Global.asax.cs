using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace RelyingPartyCustomStore {
	public class Global : System.Web.HttpApplication {
		public Global() {
			// since this is a sample, and will often be used with localhost
			DotNetOpenId.UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}
	}
}