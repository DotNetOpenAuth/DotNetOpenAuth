//-----------------------------------------------------------------------
// <copyright file="WebConsumerOpenIdRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User
	/// and can attach OAuth requests to outbound OpenID authentication requests.
	/// </summary>
	/// <remarks>
	/// The methods on this class are thread-safe.  Provided the properties are set and not changed
	/// afterward, a single instance of this class may be used by an entire web application safely.
	/// </remarks>
	public class WebConsumerOpenIdRelyingParty : WebConsumer {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumerOpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public WebConsumerOpenIdRelyingParty(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
			: base(serviceDescription, tokenManager) {
		}

		/// <summary>
		/// Attaches an OAuth authorization request to an outgoing OpenID authentication request.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="scope">The scope of access that is requested of the service provider.</param>
		public void AttachAuthorizationRequest(IAuthenticationRequest openIdAuthenticationRequest, string scope) {
			Requires.NotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");

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
			Requires.NotNull(openIdAuthenticationResponse, "openIdAuthenticationResponse");
			Requires.ValidState(this.TokenManager is IOpenIdOAuthTokenManager);
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
			// We are careful to use a v1.0 message version so that the oauth_verifier is not required.
			var requestAccess = new AuthorizedTokenRequest(this.ServiceProvider.AccessTokenEndpoint, Protocol.V10.Version) {
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
	}
}
