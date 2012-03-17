//-----------------------------------------------------------------------
// <copyright file="AuthorizationServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Authorization Server supporting the web server flow.
	/// </summary>
	public class AuthorizationServer {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServer"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public AuthorizationServer(IAuthorizationServer authorizationServer) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.Channel = new OAuth2AuthorizationServerChannel(authorizationServer);
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.</value>
		public IAuthorizationServer AuthorizationServerServices {
			get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).AuthorizationServer; }
		}

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "unauthorizedclient", Justification = "Protocol required.")]
		public EndUserAuthorizationRequest ReadAuthorizationRequest(HttpRequestBase request = null) {
			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			EndUserAuthorizationRequest message;
			if (this.Channel.TryReadFromRequest(request, out message)) {
				if (message.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
					// Clients with no secrets can only request implicit grant types.
					var client = this.AuthorizationServerServices.GetClientOrThrow(message.ClientIdentifier);
					ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(client.Secret), Protocol.unauthorized_client);
				}
			}

			return message;
		}

		/// <summary>
		/// Approves an authorization request and sends an HTTP response to the user agent to redirect the user back to the Client.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request to approve.</param>
		/// <param name="userName">The username of the account that approved the request (or whose data will be accessed by the client).</param>
		/// <param name="scopes">The scope of access the client should be granted.  If <c>null</c>, all scopes in the original request will be granted.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		public void ApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string userName, IEnumerable<string> scopes = null, Uri callback = null) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");

			var response = this.PrepareApproveAuthorizationRequest(authorizationRequest, userName, scopes, callback);
			this.Channel.Respond(response);
		}

		/// <summary>
		/// Rejects an authorization request and sends an HTTP response to the user agent to redirect the user back to the Client.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request to disapprove.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		public void RejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");

			var response = this.PrepareRejectAuthorizationRequest(authorizationRequest, callback);
			this.Channel.Respond(response);
		}

		/// <summary>
		/// Handles an incoming request to the authorization server's token endpoint.
		/// </summary>
		/// <param name="request">The HTTP request.</param>
		/// <returns>The HTTP response to send to the client.</returns>
		public OutgoingWebResponse HandleTokenRequest(HttpRequestBase request = null) {
			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			AccessTokenRequestBase requestMessage;
			IProtocolMessage responseMessage;
			try {
				if (this.Channel.TryReadFromRequest(request, out requestMessage)) {
					// TODO: refreshToken should be set appropriately based on authorization server policy.
					responseMessage = this.PrepareAccessTokenResponse(requestMessage);
				} else {
					responseMessage = new AccessTokenFailedResponse() {
						Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest,
					};
				}
			} catch (ProtocolException) {
				responseMessage = new AccessTokenFailedResponse() {
					Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest,
				};
			}

			return this.Channel.PrepareResponse(responseMessage);
		}

		/// <summary>
		/// Prepares a response to inform the Client that the user has rejected the Client's authorization request.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		/// <returns>The authorization response message to send to the Client.</returns>
		public EndUserAuthorizationFailedResponse PrepareRejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");
			Contract.Ensures(Contract.Result<EndUserAuthorizationFailedResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var response = new EndUserAuthorizationFailedResponse(callback, authorizationRequest);
			return response;
		}

		/// <summary>
		/// Approves an authorization request.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request to approve.</param>
		/// <param name="userName">The username of the account that approved the request (or whose data will be accessed by the client).</param>
		/// <param name="scopes">The scope of access the client should be granted.  If <c>null</c>, all scopes in the original request will be granted.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		/// <returns>The authorization response message to send to the Client.</returns>
		public EndUserAuthorizationSuccessResponseBase PrepareApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string userName, IEnumerable<string> scopes = null, Uri callback = null) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");
			Requires.NotNullOrEmpty(userName, "userName");
			Contract.Ensures(Contract.Result<EndUserAuthorizationSuccessResponseBase>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);
			EndUserAuthorizationSuccessResponseBase response;
			switch (authorizationRequest.ResponseType) {
				case EndUserAuthorizationResponseType.AccessToken:
					var accessTokenResponse = new EndUserAuthorizationSuccessAccessTokenResponse(callback, authorizationRequest);
					response = accessTokenResponse;
					break;
				case EndUserAuthorizationResponseType.AuthorizationCode:
					response = new EndUserAuthorizationSuccessAuthCodeResponse(callback, authorizationRequest);
					break;
				default:
					throw ErrorUtilities.ThrowInternal("Unexpected response type.");
			}

			response.AuthorizingUsername = userName;

			// Customize the approved scope if the authorization server has decided to do so.
			if (scopes != null) {
				response.Scope.ResetContents(scopes);
			}

			return response;
		}

		/// <summary>
		/// Gets the redirect URL to use for a particular authorization request.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request.</param>
		/// <returns>The URL to redirect to.  Never <c>null</c>.</returns>
		/// <exception cref="ProtocolException">Thrown if no callback URL could be determined.</exception>
		protected Uri GetCallback(EndUserAuthorizationRequest authorizationRequest) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");
			Contract.Ensures(Contract.Result<Uri>() != null);

			var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);

			// Prefer a request-specific callback to the pre-registered one (if any).
			if (authorizationRequest.Callback != null) {
				// The OAuth channel has already validated the callback parameter against
				// the authorization server's whitelist for this client.
				return authorizationRequest.Callback;
			}

			// Since the request didn't include a callback URL, look up the callback from
			// the client's preregistration with this authorization server.
			Uri defaultCallback = client.DefaultCallback;
			ErrorUtilities.VerifyProtocol(defaultCallback != null, OAuthStrings.NoCallback);
			return defaultCallback;
		}

		/// <summary>
		/// Prepares the response to an access token request.
		/// </summary>
		/// <param name="request">The request for an access token.</param>
		/// <param name="includeRefreshToken">If set to <c>true</c>, the response will include a long-lived refresh token.</param>
		/// <returns>The response message to send to the client.</returns>
		private IDirectResponseProtocolMessage PrepareAccessTokenResponse(AccessTokenRequestBase request, bool includeRefreshToken = true) {
			Requires.NotNull(request, "request");

			if (includeRefreshToken) {
				if (request is AccessTokenClientCredentialsRequest) {
					// Per OAuth 2.0 section 4.4.3 (draft 23), refresh tokens should never be included
					// in a response to an access token request that used the client credential grant type.
					Logger.OAuth.Debug("Suppressing refresh token in access token response because the grant type used by the client disallows it.");
					includeRefreshToken = false;
				}
			}

			var tokenRequest = (IAuthorizationCarryingRequest)request;
			var accessTokenRequest = (IAccessTokenRequestInternal)request;
			var response = new AccessTokenSuccessResponse(request) {
				HasRefreshToken = includeRefreshToken,
			};
			response.Scope.ResetContents(tokenRequest.AuthorizationDescription.Scope);
			return response;
		}
	}
}
