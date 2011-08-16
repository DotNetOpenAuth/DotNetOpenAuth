<%@ WebHandler Language="C#" Class="OpenIdProviderEndpoint" %>
using System;
using System.Web;
using DotNetOpenAuth.OpenId.Provider;

public class OpenIdProviderEndpoint : IHttpHandler {
	public void ProcessRequest(HttpContext context) {
		OpenIdProvider provider = new OpenIdProvider();
		IRequest request = provider.GetRequest();
		if (request != null) {
			if (!request.IsResponseReady) {
				IAuthenticationRequest authRequest = (IAuthenticationRequest)request;
				authRequest.IsAuthenticated = true;
			}

			provider.Respond(request);
		}
	}

	public bool IsReusable {
		get { return true; }
	}
}