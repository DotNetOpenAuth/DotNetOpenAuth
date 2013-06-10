//-----------------------------------------------------------------------
// <copyright file="ServiceProviderOpenIdProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// A web application that allows access via OAuth and can respond to OpenID+OAuth requests.
	/// </summary>
	/// <remarks>
	/// <para>The Service Provider’s documentation should include:</para>
	/// <list>
	/// <item>The URLs (Request URLs) the Consumer will use when making OAuth requests, and the HTTP methods (i.e. GET, POST, etc.) used in the Request Token URL and Access Token URL.</item>
	/// <item>Signature methods supported by the Service Provider.</item>
	/// <item>Any additional request parameters that the Service Provider requires in order to obtain a Token. Service Provider specific parameters MUST NOT begin with oauth_.</item>
	/// </list>
	/// </remarks>
	public class ServiceProviderOpenIdProvider : ServiceProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderOpenIdProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public ServiceProviderOpenIdProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager)
			: base(serviceDescription, tokenManager) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderOpenIdProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="messageTypeProvider">The message type provider.</param>
		public ServiceProviderOpenIdProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, OAuthServiceProviderMessageFactory messageTypeProvider)
			: base(serviceDescription, tokenManager, messageTypeProvider) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderOpenIdProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="nonceStore">The nonce store.</param>
		public ServiceProviderOpenIdProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, INonceStore nonceStore)
			: base(serviceDescription, tokenManager, nonceStore) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderOpenIdProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="nonceStore">The nonce store.</param>
		/// <param name="messageTypeProvider">The message type provider.</param>
		public ServiceProviderOpenIdProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, INonceStore nonceStore, OAuthServiceProviderMessageFactory messageTypeProvider)
			: base(serviceDescription, tokenManager, nonceStore, messageTypeProvider) {
		}

		/// <summary>
		/// Gets the OAuth authorization request included with an OpenID authentication
		/// request, if there is one.
		/// </summary>
		/// <param name="openIdRequest">The OpenID authentication request.</param>
		/// <returns>
		/// The scope of access the relying party is requesting, or null if no OAuth request
		/// is present.
		/// </returns>
		/// <remarks>
		/// <para>Call this method rather than simply extracting the OAuth extension
		/// out from the authentication request directly to ensure that the additional
		/// security measures that are required are taken.</para>
		/// </remarks>
		public AuthorizationRequest ReadAuthorizationRequest(IHostProcessedRequest openIdRequest) {
			Requires.NotNull(openIdRequest, "openIdRequest");
			RequiresEx.ValidState(this.TokenManager is ICombinedOpenIdProviderTokenManager);
			var openidTokenManager = this.TokenManager as ICombinedOpenIdProviderTokenManager;
			ErrorUtilities.VerifyOperation(openidTokenManager != null, OAuthStrings.OpenIdOAuthExtensionRequiresSpecialTokenManagerInterface, typeof(IOpenIdOAuthTokenManager).FullName);

			var authzRequest = openIdRequest.GetExtension<AuthorizationRequest>();
			if (authzRequest == null) {
				return null;
			}

			// OpenID+OAuth spec section 9:
			// The Combined Provider SHOULD verify that the consumer key passed in the
			// request is authorized to be used for the realm passed in the request.
			string expectedConsumerKey = openidTokenManager.GetConsumerKey(openIdRequest.Realm);
			ErrorUtilities.VerifyProtocol(
				string.Equals(expectedConsumerKey, authzRequest.Consumer, StringComparison.Ordinal),
				OAuthStrings.OpenIdOAuthRealmConsumerKeyDoNotMatch);

			return authzRequest;
		}

		/// <summary>
		/// Attaches the authorization response to an OpenID authentication response.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="consumerKey">The consumer key.  Must be <c>null</c> if and only if <paramref name="scope"/> is null.</param>
		/// <param name="scope">The approved access scope.  Use <c>null</c> to indicate no access was granted.  The empty string will be interpreted as some default level of access is granted.</param>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to take IAuthenticationRequest because that's the only supported use case.")]
		[Obsolete("Call the overload that doesn't take a consumerKey instead.")]
		public void AttachAuthorizationResponse(IHostProcessedRequest openIdAuthenticationRequest, string consumerKey, string scope) {
			Requires.NotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");
			Requires.That((consumerKey == null) == (scope == null), null, "consumerKey and scope must either be both provided or both omitted.");
			RequiresEx.ValidState(this.TokenManager is ICombinedOpenIdProviderTokenManager);
			var openidTokenManager = (ICombinedOpenIdProviderTokenManager)this.TokenManager;
			ErrorUtilities.VerifyArgument(consumerKey == null || consumerKey == openidTokenManager.GetConsumerKey(openIdAuthenticationRequest.Realm), OAuthStrings.OpenIdOAuthRealmConsumerKeyDoNotMatch);

			this.AttachAuthorizationResponse(openIdAuthenticationRequest, scope);
		}

		/// <summary>
		/// Attaches the authorization response to an OpenID authentication response.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="scope">The approved access scope.  Use <c>null</c> to indicate no access was granted.  The empty string will be interpreted as some default level of access is granted.</param>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to take IAuthenticationRequest because that's the only supported use case.")]
		public void AttachAuthorizationResponse(IHostProcessedRequest openIdAuthenticationRequest, string scope) {
			Requires.NotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");
			RequiresEx.ValidState(this.TokenManager is ICombinedOpenIdProviderTokenManager);

			var openidTokenManager = this.TokenManager as ICombinedOpenIdProviderTokenManager;
			IOpenIdMessageExtension response;
			if (scope != null) {
				// Generate an authorized request token to return to the relying party.
				string consumerKey = openidTokenManager.GetConsumerKey(openIdAuthenticationRequest.Realm);
				var approvedResponse = new AuthorizationApprovedResponse {
					RequestToken = this.TokenGenerator.GenerateRequestToken(consumerKey),
					Scope = scope,
				};
				openidTokenManager.StoreOpenIdAuthorizedRequestToken(consumerKey, approvedResponse);
				response = approvedResponse;
			} else {
				response = new AuthorizationDeclinedResponse();
			}

			openIdAuthenticationRequest.AddResponseExtension(response);
		}
	}
}
