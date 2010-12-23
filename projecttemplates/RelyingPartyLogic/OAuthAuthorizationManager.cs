//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorizationManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Policy;
	using System.Linq;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using DotNetOpenAuth;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth2;

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

			var httpDetails = operationContext.RequestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
			var requestUri = operationContext.RequestContext.RequestMessage.Properties["OriginalHttpRequestUri"] as Uri;

			var tokenAnalyzer = new SpecialAccessTokenAnalyzer(OAuthAuthorizationServer.AsymmetricKey, OAuthAuthorizationServer.AsymmetricKey);
			var resourceServer = new ResourceServer(tokenAnalyzer);

			try {
				IPrincipal principal;
				var errorResponse = resourceServer.VerifyAccess(httpDetails, requestUri, out principal);
				if (errorResponse == null) {
					var policy = new OAuthPrincipalAuthorizationPolicy(principal);
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

					securityContext.AuthorizationContext.Properties["Identities"] = new List<IIdentity> {
						principal.Identity,
					};

					// Only allow this method call if the access token scope permits it.
					if (principal.IsInRole(operationContext.IncomingMessageHeaders.Action)) {
						return true;
					}
				}
			} catch (ProtocolException /*ex*/) {
				////Logger.Error("Error processing OAuth messages.", ex);
			}

			return false;
		}
	}
}