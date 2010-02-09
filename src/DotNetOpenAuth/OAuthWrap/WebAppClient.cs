//-----------------------------------------------------------------------
// <copyright file="WebAppClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// An OAuth WRAP consumer designed for web applications.
	/// </summary>
	public class WebAppClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public WebAppClient(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
		}

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret shared with the Authorization Server.
		/// </summary>
		public string ClientSecret { get; set; }

		public IClientTokenManager TokenManager { get; set; }

		public WebAppRequest PrepareRequestUserAuthorization(IWrapAuthorization authorizationState) {
			Contract.Requires<ArgumentNullException>(authorizationState != null);
			Contract.Requires<InvalidOperationException>(authorizationState.Callback != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<WebAppRequest>() != null);
			Contract.Ensures(Contract.Result<WebAppRequest>().Callback == authorizationState.Callback);
			Contract.Ensures(Contract.Result<WebAppRequest>().ClientIdentifier == this.ClientIdentifier);

			if (authorizationState.Callback == null) {
				authorizationState.Callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting;
			}

			var request = new WebAppRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorizationState.Callback,
				Scope = authorizationState.Scope,
			};

			return request;
		}

		public IWrapAuthorization ProcessUserAuthorization() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			return this.ProcessUserAuthorization(this.Channel.GetRequestFromContext());
		}

		public IWrapAuthorization ProcessUserAuthorization(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientSecret));
			var response = this.Channel.ReadFromRequest<IMessageWithClientState>(request);
			if (response != null) {
				IWrapAuthorization authorizationState = this.TokenManager.GetAuthorizationState(request.UrlBeforeRewriting, response.ClientState);
				ErrorUtilities.VerifyProtocol(authorizationState != null, "Unexpected OAuth WRAP authorization response received with callback and client state that does not match an expected value.");
				var success = response as WebAppSuccessResponse;
				var failure = response as WebAppFailedResponse;
				ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
				if (success != null) {
					var accessTokenRequest = new WebAppAccessTokenRequest(this.AuthorizationServer) {
						ClientSecret = this.ClientSecret,
						ClientIdentifier = this.ClientIdentifier,
						Callback = authorizationState.Callback,
						VerificationCode = success.VerificationCode,
					};
					IProtocolMessage accessTokenResponse = this.Channel.Request(accessTokenRequest);
					var accessTokenSuccess = accessTokenResponse as WebAppAccessTokenSuccessResponse;
					var badClientAccessTokenResponse = accessTokenResponse as WebAppAccessTokenBadClientResponse;
					var failedAccessTokenResponse = accessTokenResponse as WebAppAccessTokenFailedResponse;
					if (accessTokenSuccess != null) {
						authorizationState.AccessToken = accessTokenSuccess.AccessToken;
						authorizationState.RefreshToken = accessTokenSuccess.RefreshToken;
						authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
					} else if (badClientAccessTokenResponse != null) {
						ErrorUtilities.ThrowProtocol("Failed to obtain access token due to invalid Client Identifier or Client Secret.");
					} else { // failedAccessTokenResponse != null
						ErrorUtilities.ThrowProtocol("Failed to obtain access token.  Authorization Server reports reason: {0}", failedAccessTokenResponse.ErrorReason);
					}
				} else { // failure
				}

				return authorizationState;
			}

			return null;
		}
	}
}
