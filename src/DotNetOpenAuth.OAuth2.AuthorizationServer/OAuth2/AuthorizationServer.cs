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
			this.OAuthChannel = new OAuth2AuthorizationServerChannel(authorizationServer);
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		public Channel Channel {
			get { return this.OAuthChannel; }
		}

		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.</value>
		public IAuthorizationServer AuthorizationServerServices {
			get { return this.OAuthChannel.AuthorizationServer; }
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		internal OAuth2AuthorizationServerChannel OAuthChannel { get; private set; }

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "unauthorizedclient", Justification = "Protocol required.")]
		public EndUserAuthorizationRequest ReadAuthorizationRequest(HttpRequestInfo request = null) {
			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			EndUserAuthorizationRequest message;
			if (this.Channel.TryReadFromRequest(request, out message)) {
				if (message.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
					// Clients with no secrets can only request implicit grant types.
					var client = this.AuthorizationServerServices.GetClientOrThrow(message.ClientIdentifier);
					ErrorUtilities.VerifyProtocol(!String.IsNullOrEmpty(client.Secret), Protocol.unauthorized_client);
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
		/// Checks the incoming HTTP request for an access token request and prepares a response if the request message was found.
		/// </summary>
		/// <param name="response">The formulated response, or <c>null</c> if the request was not found..</param>
		/// <returns>A value indicating whether any access token request was found in the HTTP request.</returns>
		/// <remarks>
		/// This method assumes that the authorization server and the resource server are the same and that they share a single
		/// asymmetric key for signing and encrypting the access token.  If this is not true, use the <see cref="ReadAccessTokenRequest"/> method instead.
		/// </remarks>
		public bool TryPrepareAccessTokenResponse(out IDirectResponseProtocolMessage response) {
			return this.TryPrepareAccessTokenResponse(this.Channel.GetRequestFromContext(), out response);
		}

		/// <summary>
		/// Checks the incoming HTTP request for an access token request and prepares a response if the request message was found.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="response">The formulated response, or <c>null</c> if the request was not found..</param>
		/// <returns>A value indicating whether any access token request was found in the HTTP request.</returns>
		/// <remarks>
		/// This method assumes that the authorization server and the resource server are the same and that they share a single
		/// asymmetric key for signing and encrypting the access token.  If this is not true, use the <see cref="ReadAccessTokenRequest"/> method instead.
		/// </remarks>
		public bool TryPrepareAccessTokenResponse(HttpRequestInfo httpRequestInfo, out IDirectResponseProtocolMessage response) {
			Requires.NotNull(httpRequestInfo, "httpRequestInfo");
			Contract.Ensures(Contract.Result<bool>() == (Contract.ValueAtReturn<IDirectResponseProtocolMessage>(out response) != null));

			var request = this.ReadAccessTokenRequest(httpRequestInfo);
			if (request != null) {
				response = this.PrepareAccessTokenResponse(request);
				return true;
			}

			response = null;
			return false;
		}

		/// <summary>
		/// Reads the access token request.
		/// </summary>
		/// <param name="requestInfo">The request info.</param>
		/// <returns>The Client's request for an access token; or <c>null</c> if no such message was found in the request.</returns>
		public AccessTokenRequestBase ReadAccessTokenRequest(HttpRequestInfo requestInfo = null) {
			if (requestInfo == null) {
				requestInfo = this.Channel.GetRequestFromContext();
			}

			AccessTokenRequestBase request;
			this.Channel.TryReadFromRequest(requestInfo, out request);
			return request;
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
					accessTokenResponse.Lifetime = this.AuthorizationServerServices.GetAccessTokenLifetime(authorizationRequest);
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
		/// Prepares the response to an access token request.
		/// </summary>
		/// <param name="request">The request for an access token.</param>
		/// <param name="includeRefreshToken">If set to <c>true</c>, the response will include a long-lived refresh token.</param>
		/// <returns>The response message to send to the client.</returns>
		public virtual IDirectResponseProtocolMessage PrepareAccessTokenResponse(AccessTokenRequestBase request, bool includeRefreshToken = true) {
			Requires.NotNull(request, "request");

			var tokenRequest = (IAuthorizationCarryingRequest)request;
			var response = new AccessTokenSuccessResponse(request) {
				Lifetime = this.AuthorizationServerServices.GetAccessTokenLifetime(request),
				HasRefreshToken = includeRefreshToken,
			};
			response.Scope.ResetContents(tokenRequest.AuthorizationDescription.Scope);
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
	}
}
