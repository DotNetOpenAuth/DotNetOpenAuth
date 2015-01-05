namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Policy;
	using System.Linq;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using System.Threading.Tasks;
	using DotNetOpenAuth;
	using DotNetOpenAuth.Logging;
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
			Uri requestUri = operationContext.RequestContext.RequestMessage.Properties.Via;
			ServiceProvider sp = Constants.CreateServiceProvider();
			((DatabaseTokenManager)sp.TokenManager).OperationContext = operationContext; // artificially preserve this across thread changes.
			return Task.Run(
				async delegate {
					try {
						var auth = await sp.ReadProtectedResourceAuthorizationAsync(httpDetails, requestUri);
						if (auth != null) {
							var accessToken = Global.DataContext.OAuthTokens.Single(token => token.Token == auth.AccessToken);

							var principal = sp.CreatePrincipal(auth);
							var policy = new OAuthPrincipalAuthorizationPolicy(principal);
							var policies = new List<IAuthorizationPolicy> { policy, };

							var securityContext = new ServiceSecurityContext(policies.AsReadOnly());
							if (operationContext.IncomingMessageProperties.Security != null) {
								operationContext.IncomingMessageProperties.Security.ServiceSecurityContext = securityContext;
							} else {
								operationContext.IncomingMessageProperties.Security = new SecurityMessageProperty {
									ServiceSecurityContext = securityContext,
								};
							}

							securityContext.AuthorizationContext.Properties["Identities"] = new List<IIdentity> { principal.Identity, };

							// Only allow this method call if the access token scope permits it.
							string[] scopes = accessToken.Scope.Split('|');
							if (scopes.Contains(operationContext.IncomingMessageHeaders.Action)) {
								return true;
							}
						}
					} catch (ProtocolException ex) {
						Global.Logger.ErrorException("Error processing OAuth messages.", ex);
					}

					return false;
				}).GetAwaiter().GetResult();
		}
	}
}