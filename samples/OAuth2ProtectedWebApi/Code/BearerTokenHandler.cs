namespace OAuth2ProtectedWebApi.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.OAuth2;

	/// <summary>
	/// An HTTP server message handler that detects OAuth 2 bearer tokens in the authorization header
	/// and applies the appropriate principal to the request when found.
	/// </summary>
	public class BearerTokenHandler : DelegatingHandler {
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (request.Headers.Authorization != null) {
				if (request.Headers.Authorization.Scheme == "Bearer") {
					var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AuthorizationServerHost.HardCodedCryptoKeyStore));
					var principal = await resourceServer.GetPrincipalAsync(request, cancellationToken);
					HttpContext.Current.User = principal;
					Thread.CurrentPrincipal = principal;
				}
			}

			return await base.SendAsync(request, cancellationToken);
		}
	}
}