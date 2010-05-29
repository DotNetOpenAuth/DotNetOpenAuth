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
	using System.Text;
	using DotNetOpenAuth.Messaging;
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
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public WebAppRequest ReadAuthorizationRequest() {
			return this.ReadAuthorizationRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public WebAppRequest ReadAuthorizationRequest(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);
			WebAppRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		public void ApproveAuthorizationRequest(WebAppRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			var response = this.PrepareApproveAuthorizationRequest(authorizationRequest, callback);
			this.Channel.Send(response);
		}

		public void RejectAuthorizationRequest(WebAppRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			var response = this.PrepareRejectAuthorizationRequest(authorizationRequest, callback);
			this.Channel.Send(response);
		}

		internal WebAppFailedResponse PrepareRejectAuthorizationRequest(WebAppRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var response = new WebAppFailedResponse(callback, authorizationRequest);
			return response;
		}

		internal WebAppSuccessResponse PrepareApproveAuthorizationRequest(WebAppRequest authorizationRequest, Uri callback = null) {
			Contract.Requires<ArgumentNullException>(authorizationRequest != null, "authorizationRequest");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = GetClient(authorizationRequest.ClientIdentifier);
			var response = new WebAppSuccessResponse(callback, authorizationRequest) {
				VerificationCode = OAuth.ServiceProvider.CreateVerificationCode(client.VerificationCodeFormat, client.VerificationCodeLength),
			};

			return response;
		}

		protected Uri GetCallback(WebAppRequest authorizationRequest) {
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
