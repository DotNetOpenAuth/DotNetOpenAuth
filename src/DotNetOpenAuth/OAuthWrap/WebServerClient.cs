//-----------------------------------------------------------------------
// <copyright file="WebServerClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// An OAuth WRAP consumer designed for web applications.
	/// </summary>
	public class WebServerClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public WebServerClient(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
		}

		/// <summary>
		/// Gets or sets the token manager.
		/// </summary>
		/// <value>The token manager.</value>
		public IClientTokenManager TokenManager { get; set; }

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <returns>The authorization request.</returns>
		public EndUserAuthorizationRequest PrepareRequestUserAuthorization() {
			return this.PrepareRequestUserAuthorization(new AuthorizationState());
		}

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="authorization">The authorization state to associate with this particular request.</param>
		/// <returns>The authorization request.</returns>
		public EndUserAuthorizationRequest PrepareRequestUserAuthorization(IAuthorizationState authorization) {
			Contract.Requires<ArgumentNullException>(authorization != null);
			Contract.Requires<InvalidOperationException>(authorization.Callback != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<EndUserAuthorizationRequest>() != null);
			Contract.Ensures(Contract.Result<EndUserAuthorizationRequest>().ClientIdentifier == this.ClientIdentifier);
			Contract.Ensures(Contract.Result<EndUserAuthorizationRequest>().Callback == authorization.Callback);

			if (authorization.Callback == null) {
				authorization.Callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting
					.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(WebServerSuccessResponse), Protocol.Default.Version));
				authorization.SaveChanges();
			}

			var request = new EndUserAuthorizationRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorization.Callback,
				Scope = authorization.Scope,
			};

			return request;
		}

		/// <summary>
		/// Processes the authorization response from an authorization server, if available.
		/// </summary>
		/// <param name="request">The incoming HTTP request that may carry an authorization response.</param>
		/// <returns>The authorization state that contains the details of the authorization.</returns>
		public IAuthorizationState ProcessUserAuthorization(HttpRequestInfo request = null) {
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientSecret));
			Contract.Requires<InvalidOperationException>(this.TokenManager != null);

			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			IMessageWithClientState response;
			if (this.Channel.TryReadFromRequest<IMessageWithClientState>(request, out response)) {
				Uri callback = MessagingUtilities.StripMessagePartsFromQueryString(request.UrlBeforeRewriting, this.Channel.MessageDescriptions.Get(response));
				IAuthorizationState authorizationState = this.TokenManager.GetAuthorizationState(callback, response.ClientState);
				ErrorUtilities.VerifyProtocol(authorizationState != null, "Unexpected OAuth authorization response received with callback and client state that does not match an expected value.");
				var success = response as WebServerSuccessResponse;
				var failure = response as WebServerFailedResponse;
				ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
				if (success != null) {
					var accessTokenRequest = new WebServerAccessTokenRequest(this.AuthorizationServer) {
						ClientIdentifier = this.ClientIdentifier,
						ClientSecret = this.ClientSecret,
						Callback = authorizationState.Callback,
						VerificationCode = success.VerificationCode,
					};
					IProtocolMessage accessTokenResponse = this.Channel.Request(accessTokenRequest);
					var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
					var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
					if (accessTokenSuccess != null) {
						this.UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
					} else {
						authorizationState.Delete();
						string error = failedAccessTokenResponse != null ? failedAccessTokenResponse.Error : "(unknown)";
						ErrorUtilities.ThrowProtocol(OAuthWrapStrings.CannotObtainAccessTokenWithReason, error);
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
