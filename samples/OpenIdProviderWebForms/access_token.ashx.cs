namespace OpenIdProviderWebForms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Services;
	using DotNetOpenAuth.OAuth;
	using OpenIdProviderWebForms.Code;

	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class access_token : IHttpHandler {
		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest(HttpContext context) {
			var request = OAuthHybrid.ServiceProvider.ReadAccessTokenRequest();
			var response = OAuthHybrid.ServiceProvider.PrepareAccessTokenMessage(request);
			OAuthHybrid.ServiceProvider.Channel.Respond(response);
		}
	}
}
