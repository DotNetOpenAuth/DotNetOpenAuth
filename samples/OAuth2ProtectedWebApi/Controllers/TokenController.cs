namespace OAuth2ProtectedWebApi.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web.Http;

	using DotNetOpenAuth.OAuth2;

	public class TokenController : ApiController {
		// POST /api/token
		public Task<HttpResponseMessage> Post(HttpRequestMessage request) {
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			return authServer.HandleTokenRequestAsync(request);
		}
	}
}