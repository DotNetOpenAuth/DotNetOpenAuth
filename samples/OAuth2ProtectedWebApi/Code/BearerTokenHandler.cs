namespace OAuth2ProtectedWebApi.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.OAuth2;

	public class BearerTokenHandler : DelegatingHandler {
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (request.Headers.Authorization != null) {
				if (request.Headers.Authorization.Scheme == "Bearer") {
					string bearer = request.Headers.Authorization.Parameter;
					var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(MemoryCryptoKeyStore.Instance));
					var principal = await resourceServer.GetPrincipalAsync(request, cancellationToken);
					HttpContext.Current.User = principal;
					Thread.CurrentPrincipal = principal;
				}
			}

			return await base.SendAsync(request, cancellationToken);
		}
	}
}