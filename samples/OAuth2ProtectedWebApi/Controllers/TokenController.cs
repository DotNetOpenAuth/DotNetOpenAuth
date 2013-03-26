namespace OAuth2ProtectedWebApi.Controllers {
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web.Http;

	using DotNetOpenAuth.OAuth2;

	using OAuth2ProtectedWebApi.Code;

	public class TokenController : ApiController {
		// POST /api/token
		public Task<HttpResponseMessage> Post(HttpRequestMessage request) {
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			return authServer.HandleTokenRequestAsync(request);
		}
	}
}