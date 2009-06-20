//-----------------------------------------------------------------------
// <copyright file="WebConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User.
	/// </summary>
	/// <remarks>
	/// The methods on this class are thread-safe.  Provided the properties are set and not changed
	/// afterward, a single instance of this class may be used by an entire web application safely.
	/// </remarks>
	public class WebConsumer : ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public WebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
			: base(serviceDescription, tokenManager) {
		}

		/// <summary>
		/// Begins an OAuth authorization request and redirects the user to the Service Provider
		/// to provide that authorization.  Upon successful authorization, the user is redirected
		/// back to the current page.
		/// </summary>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public UserAuthorizationRequest PrepareRequestUserAuthorization() {
			Uri callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting.StripQueryArgumentsWithPrefix(Protocol.ParameterPrefix);
			return this.PrepareRequestUserAuthorization(callback, null, null);
		}

		/// <summary>
		/// Prepares an OAuth message that begins an authorization request that will 
		/// redirect the user to the Service Provider to provide that authorization.
		/// </summary>
		/// <param name="callback">
		/// An optional Consumer URL that the Service Provider should redirect the 
		/// User Agent to upon successful authorization.
		/// </param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="redirectParameters">Extra parameters to add to the redirect to Service Provider message.  Optional.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public UserAuthorizationRequest PrepareRequestUserAuthorization(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters) {
			string token;
			return this.PrepareRequestUserAuthorization(callback, requestParameters, redirectParameters, out token);
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <returns>The access token, or null if no incoming authorization message was recognized.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public AuthorizedTokenResponse ProcessUserAuthorization() {
			return this.ProcessUserAuthorization(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Attaches an OAuth authorization request to an outgoing OpenID authentication request.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="scope">The scope of access that is requested of the service provider.</param>
		public void AttachAuthorizationRequest(IAuthenticationRequest openIdAuthenticationRequest, string scope) {
			Contract.Requires(openIdAuthenticationRequest != null);
			ErrorUtilities.VerifyArgumentNotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");

			var authorizationRequest = new AuthorizationRequest {
				Consumer = this.ConsumerKey,
				Scope = scope,
			};

			openIdAuthenticationRequest.AddExtension(authorizationRequest);
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <param name="openIdAuthenticationResponse">The OpenID authentication response that may be carrying an authorized request token.</param>
		/// <returns>
		/// The access token, or null if OAuth authorization was denied by the user or service provider.
		/// </returns>
		/// <remarks>
		/// The access token, if granted, is automatically stored in the <see cref="ConsumerBase.TokenManager"/>.
		/// The token manager instance must implement <see cref="IOpenIdOAuthTokenManager"/>.
		/// </remarks>
		public AuthorizedTokenResponse ProcessUserAuthorization(IAuthenticationResponse openIdAuthenticationResponse) {
			Contract.Requires(openIdAuthenticationResponse != null);
			Contract.Requires(this.TokenManager is IOpenIdOAuthTokenManager);
			ErrorUtilities.VerifyArgumentNotNull(openIdAuthenticationResponse, "openIdAuthenticationResponse");
			var openidTokenManager = this.TokenManager as IOpenIdOAuthTokenManager;
			ErrorUtilities.VerifyOperation(openidTokenManager != null, OAuthStrings.OpenIdOAuthExtensionRequiresSpecialTokenManagerInterface, typeof(IOpenIdOAuthTokenManager).FullName);

			// The OAuth extension is only expected in positive assertion responses.
			if (openIdAuthenticationResponse.Status != AuthenticationStatus.Authenticated) {
				return null;
			}

			// Retrieve the OAuth extension
			var positiveAuthorization = openIdAuthenticationResponse.GetExtension<AuthorizationApprovedResponse>();
			if (positiveAuthorization == null) {
				return null;
			}

			// Prepare a message to exchange the request token for an access token.
			var requestAccess = new AuthorizedTokenRequest(this.ServiceProvider.AccessTokenEndpoint, this.ServiceProvider.Version) {
				RequestToken = positiveAuthorization.RequestToken,
				ConsumerKey = this.ConsumerKey,
			};

			// Retrieve the access token and store it in the token manager.
			openidTokenManager.StoreOpenIdAuthorizedRequestToken(this.ConsumerKey, positiveAuthorization);
			var grantAccess = this.Channel.Request<AuthorizedTokenResponse>(requestAccess);
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, positiveAuthorization.RequestToken, grantAccess.AccessToken, grantAccess.TokenSecret);

			// Provide the caller with the access token so it may be associated with the user
			// that is logging in.
			return grantAccess;
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The access token, or null if no incoming authorization message was recognized.</returns>
		public AuthorizedTokenResponse ProcessUserAuthorization(HttpRequestInfo request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			UserAuthorizationResponse authorizationMessage;
			if (this.Channel.TryReadFromRequest<UserAuthorizationResponse>(request, out authorizationMessage)) {
				string requestToken = authorizationMessage.RequestToken;
				string verifier = authorizationMessage.VerificationCode;
				return this.ProcessUserAuthorization(requestToken, verifier);
			} else {
				return null;
			}
		}
	}
}
