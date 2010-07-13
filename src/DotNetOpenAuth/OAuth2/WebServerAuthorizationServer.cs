//-----------------------------------------------------------------------
// <copyright file="WebServerAuthorizationServer.cs" company="Andrew Arnott">
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
	public class WebServerAuthorizationServer : AuthorizationServerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerAuthorizationServer"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public WebServerAuthorizationServer(IAuthorizationServer authorizationServer)
			: base(authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
		}

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

		public void ApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string username, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");

			var response = this.PrepareApproveAuthorizationRequest(authorizationRequest, username, callback);
			this.Channel.Send(response);
		}

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
				response = this.PrepareAccessTokenResponse(request, this.AuthorizationServer.AccessTokenSigningPrivateKey);
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

		internal EndUserAuthorizationFailedResponse PrepareRejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<EndUserAuthorizationFailedResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var response = new EndUserAuthorizationFailedResponse(callback, authorizationRequest);
			return response;
		}

		public EndUserAuthorizationSuccessResponseBase PrepareApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string username, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(username));
			Contract.Ensures(Contract.Result<EndUserAuthorizationSuccessResponseBase>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = this.AuthorizationServer.GetClientOrThrow(authorizationRequest.ClientIdentifier);
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
			return response;
		}

		protected Uri GetCallback(EndUserAuthorizationRequest authorizationRequest) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<Uri>() != null);

			// Prefer a request-specific callback to the pre-registered one (if any).
			if (authorizationRequest.Callback != null) {
				return authorizationRequest.Callback;
			}

			var client = this.AuthorizationServer.GetClient(authorizationRequest.ClientIdentifier);
			if (client.Callback != null) {
				return client.Callback;
			}

			throw ErrorUtilities.ThrowProtocol(OAuthWrapStrings.NoCallback);
		}
	}
}
