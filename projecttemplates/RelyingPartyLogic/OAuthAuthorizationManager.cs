//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorizationManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using DotNetOpenAuth.Messaging;
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
			var requestUri = operationContext.RequestContext.RequestMessage.Properties.Via;

			using (var crypto = OAuthResourceServer.CreateRSA()) {
				var tokenAnalyzer = new SpecialAccessTokenAnalyzer(crypto, crypto);
				var resourceServer = new ResourceServer(tokenAnalyzer);

				try {
					IPrincipal principal = resourceServer.GetPrincipal(httpDetails, requestUri, operationContext.IncomingMessageHeaders.Action);
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

					return true;
				} catch (ProtocolFaultResponseException ex) {
					// Return the appropriate unauthorized response to the client.
					ex.CreateErrorResponse().Send();
				} catch (DotNetOpenAuth.Messaging.ProtocolException/* ex*/) {
					////Logger.Error("Error processing OAuth messages.", ex);
				}
			}

			return false;
		}
	}
}
