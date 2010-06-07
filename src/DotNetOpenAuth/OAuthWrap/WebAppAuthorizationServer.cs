//-----------------------------------------------------------------------
// <copyright file="WebAppAuthorizationServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;
	using DotNetOpenAuth.OAuthWrap.Messages;

	public class WebAppAuthorizationServer : AuthorizationServerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppAuthorizationServer"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public WebAppAuthorizationServer(IAuthorizationServer authorizationServer)
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
		public WebServerRequest ReadAuthorizationRequest(HttpRequestInfo request = null) {
			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			WebServerRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		public void ApproveAuthorizationRequest(WebServerRequest authorizationRequest, string username, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");

			var response = this.PrepareApproveAuthorizationRequest(authorizationRequest, callback);
			response.AuthorizingUsername = username;
			this.Channel.Send(response);
		}

		public void RejectAuthorizationRequest(WebServerRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");

			var response = this.PrepareRejectAuthorizationRequest(authorizationRequest, callback);
			this.Channel.Send(response);
		}

		public bool TryPrepareAccessTokenResponse(out IDirectResponseProtocolMessage response)
		{
			return this.TryPrepareAccessTokenResponse(this.Channel.GetRequestFromContext(), out response);
		}

		public bool TryPrepareAccessTokenResponse(HttpRequestInfo httpRequestInfo, out IDirectResponseProtocolMessage response)
		{
			Contract.Requires<ArgumentNullException>(httpRequestInfo != null, "httpRequestInfo");

			var request = ReadAccessTokenRequest(httpRequestInfo);
			if (request != null)
			{
				// This convenience method only encrypts access tokens assuming that this auth server
				// doubles as the resource server.
				response = this.PrepareAccessTokenResponse(request, this.AuthorizationServer.AccessTokenSigningPrivateKey);
				return true;
			}

			response = null;
			return false;
		}

		public IAccessTokenRequest ReadAccessTokenRequest(HttpRequestInfo requestInfo = null) {
			if (requestInfo == null) {
				requestInfo = this.Channel.GetRequestFromContext();
			}

			IAccessTokenRequest request;
			this.Channel.TryReadFromRequest(requestInfo, out request);
			return request;
		}

		internal WebServerFailedResponse PrepareRejectAuthorizationRequest(WebServerRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<WebServerFailedResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var response = new WebServerFailedResponse(callback, authorizationRequest);
			return response;
		}

		internal WebServerSuccessResponse PrepareApproveAuthorizationRequest(WebServerRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<WebServerSuccessResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = this.AuthorizationServer.GetClientOrThrow(authorizationRequest.ClientIdentifier);
			var response = new WebServerSuccessResponse(callback, authorizationRequest);
			return response;
		}

		protected Uri GetCallback(WebServerRequest authorizationRequest) {
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
