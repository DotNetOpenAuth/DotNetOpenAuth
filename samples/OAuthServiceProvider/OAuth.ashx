<%@ WebHandler Language="C#" Class="OAuth" %>

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using DotNetOpenAuth.Messaging;
using OAuthServiceProvider.Code;

public class OAuth : HttpAsyncHandlerBase, IRequiresSessionState {
	ServiceProvider sp;

	public OAuth() {
		sp = new ServiceProvider(Constants.SelfDescription, Global.TokenManager, new CustomOAuthMessageFactory(Global.TokenManager));
	}

	public override bool IsReusable {
		get { return true; }
	}

	protected override async Task ProcessRequestAsync(HttpContext context) {
		IProtocolMessage request = await sp.ReadRequestAsync();
		RequestScopedTokenMessage requestToken;
		UserAuthorizationRequest requestAuth;
		AuthorizedTokenRequest requestAccessToken;
		if ((requestToken = request as RequestScopedTokenMessage) != null) {
			var response = sp.PrepareUnauthorizedTokenMessage(requestToken);
			var responseMessage = await sp.Channel.PrepareResponseAsync(response);
			await responseMessage.SendAsync(new HttpContextWrapper(context));
		} else if ((requestAuth = request as UserAuthorizationRequest) != null) {
			Global.PendingOAuthAuthorization = requestAuth;
			HttpContext.Current.Response.Redirect("~/Members/Authorize.aspx");
		} else if ((requestAccessToken = request as AuthorizedTokenRequest) != null) {
			var response = sp.PrepareAccessTokenMessage(requestAccessToken);
			var responseMessage = await sp.Channel.PrepareResponseAsync(response);
			await responseMessage.SendAsync(new HttpContextWrapper(context));
		} else {
			throw new InvalidOperationException();
		}
	}
}
