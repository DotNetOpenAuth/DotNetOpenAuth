<%@ WebHandler Language="C#" Class="OAuth" %>

using System;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using DotNetOAuth;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messages;
using DotNetOAuth.Messaging;

public class OAuth : IHttpHandler, IRequiresSessionState {
	ServiceProvider sp;

	public OAuth() {
		sp = new ServiceProvider(Constants.SelfDescription, Global.TokenManager, new CustomOAuthTypeProvider(Global.TokenManager));
	}

	public void ProcessRequest(HttpContext context) {
		IProtocolMessage request = sp.ReadRequest();
		RequestScopedTokenMessage requestToken;
		DirectUserToServiceProviderMessage requestAuth;
		GetAccessTokenMessage requestAccessToken;
		if ((requestToken = request as RequestScopedTokenMessage) != null) {
			var response = sp.PrepareUnauthorizedTokenMessage(requestToken);
			sp.Channel.Send(response).Send();
		} else if ((requestAuth = request as DirectUserToServiceProviderMessage) != null) {
			Global.PendingOAuthAuthorization = requestAuth;
			HttpContext.Current.Response.Redirect("~/Members/Authorize.aspx");
		} else if ((requestAccessToken = request as GetAccessTokenMessage) != null) {
			var response = sp.PrepareAccessTokenMessage(requestAccessToken);
			sp.Channel.Send(response).Send();
		} else {
			throw new InvalidOperationException();
		}
	}

	public bool IsReusable {
		get { return true; }
	}
}