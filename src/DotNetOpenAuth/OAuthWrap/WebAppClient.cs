//-----------------------------------------------------------------------
// <copyright file="WebAppClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Net;

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

		public WebAppRequest PrepareRequestUserAuthorization() {
			return PrepareRequestUserAuthorization(new AuthorizationState());
		}

		public WebAppRequest PrepareRequestUserAuthorization(IAuthorizationState authorization) {
			Contract.Requires<ArgumentNullException>(authorization != null);
			Contract.Requires<InvalidOperationException>(authorization.Callback != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<WebAppRequest>() != null);
			Contract.Ensures(Contract.Result<WebAppRequest>().ClientIdentifier == this.ClientIdentifier);
			Contract.Ensures(Contract.Result<WebAppRequest>().Callback == authorization.Callback);

			if (authorization.Callback == null) {
				authorization.Callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting;
				authorization.SaveChanges();
			}

			var request = new WebAppRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorization.Callback,
			};

			return request;
		}

		public IAuthorizationState ProcessUserAuthorization() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			return this.ProcessUserAuthorization(this.Channel.GetRequestFromContext());
		}

		public IAuthorizationState ProcessUserAuthorization(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientSecret));

			var response = this.Channel.ReadFromRequest<IMessageWithClientState>(request);
			if (response != null) {
				IAuthorizationState authorizationState = this.TokenManager.GetAuthorizationState(request.UrlBeforeRewriting, response.ClientState);
				ErrorUtilities.VerifyProtocol(authorizationState != null, "Unexpected OAuth authorization response received with callback and client state that does not match an expected value.");
				var success = response as WebAppSuccessResponse;
				var failure = response as WebAppFailedResponse;
				ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
				if (success != null) {
					var accessTokenRequest = new WebAppAccessTokenRequest(this.AuthorizationServer) {
						ClientIdentifier = this.ClientIdentifier,
						ClientSecret = this.ClientSecret,
						Callback = authorizationState.Callback,
						VerificationCode = success.VerificationCode,
					};
					IProtocolMessage accessTokenResponse = this.Channel.Request(accessTokenRequest);
					var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
					var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
					if (accessTokenSuccess != null) {
						authorizationState.AccessToken = accessTokenSuccess.AccessToken;
						authorizationState.AccessTokenSecret = accessTokenSuccess.AccessTokenSecret;
						authorizationState.RefreshToken = accessTokenSuccess.RefreshToken;
						authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
						authorizationState.SaveChanges();
					} else {
						authorizationState.Delete();
						ErrorUtilities.ThrowProtocol(OAuthWrapStrings.CannotObtainAccessTokenWithReason, failedAccessTokenResponse.Error);
					}
				} else { // failure
					Logger.Wrap.Info("User refused to grant the requested authorization at the Authorization Server.");
					authorizationState.Delete();
				}

				return authorizationState;
			}

			return null;
		}
	}
}
