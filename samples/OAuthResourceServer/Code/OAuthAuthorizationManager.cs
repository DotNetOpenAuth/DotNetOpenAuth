namespace OAuthResourceServer.Code {
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Policy;
	using System.Linq;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using System.ServiceModel.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using ProtocolException = System.ServiceModel.ProtocolException;

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

			try {
				var principal = VerifyOAuth2(httpDetails, requestUri, operationContext.IncomingMessageHeaders.Action ?? operationContext.IncomingMessageHeaders.To.AbsolutePath);
				if (principal != null) {
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
				} else {
					return false;
				}
			} catch (ProtocolFaultResponseException ex) {
				Global.Logger.Error("Error processing OAuth messages.", ex);

				// Return the appropriate unauthorized response to the client.
				var outgoingResponse = ex.CreateErrorResponse();
				outgoingResponse.Respond(WebOperationContext.Current.OutgoingResponse);
			} catch (ProtocolException ex) {
				Global.Logger.Error("Error processing OAuth messages.", ex);
			}

			return false;
		}

		private static IPrincipal VerifyOAuth2(HttpRequestMessageProperty httpDetails, Uri requestUri, params string[] requiredScopes) {
			// for this sample where the auth server and resource server are the same site,
			// we use the same public/private key.
			using (var signing = Global.CreateAuthorizationServerSigningServiceProvider()) {
				using (var encrypting = Global.CreateResourceServerEncryptionServiceProvider()) {
					var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(signing, encrypting));
					return resourceServer.GetPrincipal(httpDetails, requestUri, requiredScopes);
				}
			}
		}
	}
}
