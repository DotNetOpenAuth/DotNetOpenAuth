using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DotNetOpenAuth;
using DotNetOpenAuth.OAuth;

/// <summary>
/// A WCF extension to authenticate incoming messages using OAuth.
/// </summary>
public class OAuthAuthorizationManager : ServiceAuthorizationManager {
	public OAuthAuthorizationManager() {
	}

	protected override bool CheckAccessCore(OperationContext operationContext) {
		if (!base.CheckAccessCore(operationContext)) {
			return false;
		}

		HttpRequestMessageProperty httpDetails = operationContext.RequestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
		Uri requestUri = operationContext.RequestContext.RequestMessage.Properties["OriginalHttpRequestUri"] as Uri;
		ServiceProvider sp = Constants.CreateServiceProvider();
		var auth = sp.ReadProtectedResourceAuthorization(httpDetails, requestUri);
		if (auth != null) {
			var accessToken = Global.DataContext.OAuthTokens.Single(token => token.Token == auth.AccessToken);

			// Only allow this method call if the access token scope permits it.
			string[] scopes = accessToken.Scope.Split('|');
			if (scopes.Contains(operationContext.IncomingMessageHeaders.Action)) {
				operationContext.IncomingMessageProperties["OAuthAccessToken"] = accessToken;
				return true;
			}
		}

		return false;
	}
}
