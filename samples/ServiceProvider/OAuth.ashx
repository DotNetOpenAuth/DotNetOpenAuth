<%@ WebHandler Language="C#" Class="OAuth" %>

using System;
using System.Web;
using System.Web.SessionState;
using DotNetOAuth;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messages;
using DotNetOAuth.Messaging;

public class OAuth : IHttpHandler, IRequiresSessionState {
	ServiceProvider sp;

	public OAuth() {
		sp = new ServiceProvider(Constants.SelfDescription, Constants.TokenManager);
	}

	public void ProcessRequest(HttpContext context) {
		IProtocolMessage request = sp.ReadRequest();
		RequestTokenMessage requestToken;
		DirectUserToServiceProviderMessage requestAuth;
		RequestAccessTokenMessage requestAccessToken;
		if ((requestToken = request as RequestTokenMessage) != null) {
			sp.SendUnauthorizedTokenResponse(requestToken, null).Send();
		} else if ((requestAuth = request as DirectUserToServiceProviderMessage) != null) {
			HttpContext.Current.Session["authrequest"] = requestAuth;
			//HttpContext.Current.Response.Redirect("~/authorize.aspx");
			Constants.TokenManager.AuthorizeRequestToken(requestAuth.RequestToken);
			sp.SendAuthorizationResponse(requestAuth).Send();
		} else if ((requestAccessToken = request as RequestAccessTokenMessage) != null) {
			sp.SendAccessToken(requestAccessToken, null).Send();
		} else {
			throw new InvalidOperationException();
		}
	}

	public bool IsReusable {
		get { return true; }
	}
}