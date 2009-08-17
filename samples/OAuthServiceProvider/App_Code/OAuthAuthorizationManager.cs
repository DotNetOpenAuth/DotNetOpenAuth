using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
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

			var policy = new OAuthPrincipalAuthorizationPolicy(sp.CreatePrincipal(auth));
			var policies = new List<IAuthorizationPolicy> {
				policy,
			};
			var securityContext = new ServiceSecurityContext(policies.AsReadOnly());
			if (operationContext.IncomingMessageProperties.Security != null) {
				operationContext.IncomingMessageProperties.Security.ServiceSecurityContext = securityContext;
			} else {
				operationContext.IncomingMessageProperties.Security = new SecurityMessageProperty {
					ServiceSecurityContext = securityContext,
				};
			}

			// Only allow this method call if the access token scope permits it.
			string[] scopes = accessToken.Scope.Split('|');
			if (scopes.Contains(operationContext.IncomingMessageHeaders.Action)) {
				return true;
			}
		}

		return false;
	}
}
