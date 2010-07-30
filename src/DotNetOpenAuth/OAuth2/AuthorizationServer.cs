//-----------------------------------------------------------------------
// <copyright file="AuthorizationServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
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
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
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
		public EndUserAuthorizationRequest ReadAuthorizationRequest(HttpRequestInfo request = null) {
			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			EndUserAuthorizationRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Approves an authorization request and sends an HTTP response to the user agent to redirect the user back to the Client.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request to approve.</param>
		/// <param name="username">The username of the account that approved the request (or whose data will be accessed by the client).</param>
		/// <param name="scopes">The scope of access the client should be granted.  If <c>null</c>, all scopes in the original request will be granted.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		public void ApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string username, IEnumerable<string> scopes = null, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");

			var response = this.PrepareApproveAuthorizationRequest(authorizationRequest, username, scopes, callback);

			this.Channel.Send(response);
		}

		/// <summary>
		/// Rejects an authorization request and sends an HTTP response to the user agent to redirect the user back to the Client.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request to disapprove.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		public void RejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");

			var response = this.PrepareRejectAuthorizationRequest(authorizationRequest, callback);
			this.Channel.Send(response);
		}

		public bool TryPrepareAccessTokenResponse(out IDirectResponseProtocolMessage response) {
			return this.TryPrepareAccessTokenResponse(this.Channel.GetRequestFromContext(), out response);
		}

		public bool TryPrepareAccessTokenResponse(HttpRequestInfo httpRequestInfo, out IDirectResponseProtocolMessage response) {
			Contract.Requires<ArgumentNullException>(httpRequestInfo != null, "httpRequestInfo");

			var request = this.ReadAccessTokenRequest(httpRequestInfo);
			if (request != null) {
				// This convenience method only encrypts access tokens assuming that this auth server
				// doubles as the resource server.
				RSAParameters resourceServerPublicKey = this.AuthorizationServerServices.AccessTokenSigningPrivateKey;
				response = this.PrepareAccessTokenResponse(request, resourceServerPublicKey);
				return true;
			}

			response = null;
			return false;
		}

		public AccessTokenRequestBase ReadAccessTokenRequest(HttpRequestInfo requestInfo = null) {
			if (requestInfo == null) {
				requestInfo = this.Channel.GetRequestFromContext();
			}

			AccessTokenRequestBase request;
			this.Channel.TryReadFromRequest(requestInfo, out request);
			return request;
		}

		public EndUserAuthorizationFailedResponse PrepareRejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
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
		/// <param name="username">The username of the account that approved the request (or whose data will be accessed by the client).</param>
		/// <param name="scopes">The scope of access the client should be granted.  If <c>null</c>, all scopes in the original request will be granted.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		/// <returns>The authorization response message to send to the Client.</returns>
		public EndUserAuthorizationSuccessResponseBase PrepareApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string username, IEnumerable<string> scopes = null, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(username));
			Contract.Ensures(Contract.Result<EndUserAuthorizationSuccessResponseBase>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);
			EndUserAuthorizationSuccessResponseBase response;
			switch (authorizationRequest.ResponseType) {
				case EndUserAuthorizationResponseType.AccessToken:
					response = new EndUserAuthorizationSuccessAccessTokenResponse(callback, authorizationRequest);
					break;
				case EndUserAuthorizationResponseType.Both:
				case EndUserAuthorizationResponseType.AuthorizationCode:
					response = new EndUserAuthorizationSuccessAuthCodeResponse(callback, authorizationRequest);
					break;
				default:
					throw ErrorUtilities.ThrowInternal("Unexpected response type.");
			}

			response.AuthorizingUsername = username;

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
		/// <param name="accessTokenEncryptingPublicKey">The public key to encrypt the access token to, such that the resource server will be able to decrypt it.</param>
		/// <param name="accessTokenLifetime">The access token's lifetime.</param>
		/// <param name="includeRefreshToken">If set to <c>true</c>, the response will include a long-lived refresh token.</param>
		/// <returns>The response message to send to the client.</returns>
		public virtual IDirectResponseProtocolMessage PrepareAccessTokenResponse(AccessTokenRequestBase request, RSAParameters accessTokenEncryptingPublicKey, TimeSpan? accessTokenLifetime = null, bool includeRefreshToken = true) {
			Contract.Requires<ArgumentNullException>(request != null, "request");

			var tokenRequest = (ITokenCarryingRequest)request;
			var accessTokenFormatter = AccessToken.CreateFormatter(this.AuthorizationServerServices.AccessTokenSigningPrivateKey, accessTokenEncryptingPublicKey);
			var accessToken = new AccessToken(tokenRequest.AuthorizationDescription, accessTokenLifetime);

			var response = new AccessTokenSuccessResponse(request) {
				AccessToken = accessTokenFormatter.Serialize(accessToken),
				Lifetime = accessToken.Lifetime,
			};
			response.Scope.ResetContents(tokenRequest.AuthorizationDescription.Scope);

			if (includeRefreshToken) {
				var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServerServices.Secret);
				var refreshToken = new RefreshToken(tokenRequest.AuthorizationDescription);
				response.RefreshToken = refreshTokenFormatter.Serialize(refreshToken);
			}

			return response;
		}

		protected Uri GetCallback(EndUserAuthorizationRequest authorizationRequest) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<Uri>() != null);

			// Prefer a request-specific callback to the pre-registered one (if any).
			if (authorizationRequest.Callback != null) {
				return authorizationRequest.Callback;
			}

			var client = this.AuthorizationServerServices.GetClient(authorizationRequest.ClientIdentifier);
			if (client.AllowedCallbacks.Any()) {
				return client.AllowedCallbacks.First();
			}

			throw ErrorUtilities.ThrowProtocol(OAuthStrings.NoCallback);
		}
	}
}
