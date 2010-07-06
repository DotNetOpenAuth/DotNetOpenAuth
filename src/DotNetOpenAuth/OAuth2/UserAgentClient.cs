//-----------------------------------------------------------------------
// <copyright file="UserAgentClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// The OAuth client for the user-agent flow, providing services for installed apps
	/// and in-browser Javascript widgets.
	/// </summary>
	public class UserAgentClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientSecret">The client secret.</param>
		public UserAgentClient(AuthorizationServerDescription authorizationServer, string clientIdentifier = null, string clientSecret = null)
			: base(authorizationServer, clientIdentifier, clientSecret) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		public UserAgentClient(Uri authorizationEndpoint)
			: base(new AuthorizationServerDescription { AuthorizationEndpoint = authorizationEndpoint }) {
			Contract.Requires<ArgumentNullException>(authorizationEndpoint != null, "authorizationEndpoint");
		}

		/// <summary>
		/// Generates a URL that the user's browser can be directed to in order to authorize
		/// this client to access protected data at some resource server.
		/// </summary>
		/// <param name="scope">The scope of authorized access requested.</param>
		/// <returns>A fully-qualified URL suitable to initiate the authorization flow.</returns>
		public Uri RequestUserAuthoroization(string scope = null) {
			var authorization = new AuthorizationState { Scope = scope };
			return this.RequestUserAuthorization(authorization);
		}

		/// <summary>
		/// Generates a URL that the user's browser can be directed to in order to authorize
		/// this client to access protected data at some resource server.
		/// </summary>
		/// <param name="authorization">The authorization state that is tracking this particular request.  Optional.</param>
		/// <returns>A fully-qualified URL suitable to initiate the authorization flow.</returns>
		public Uri RequestUserAuthorization(IAuthorizationState authorization) {
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));

			if (authorization.Callback == null) {
				authorization.Callback = new Uri("http://localhost/");
			}

			var request = new EndUserAuthorizationRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Scope = authorization.Scope,
				Callback = authorization.Callback,
			};

			return this.Channel.PrepareResponse(request).GetDirectUriRequest(this.Channel);
		}

		/// <summary>
		/// Scans the incoming request for an authorization response message.
		/// </summary>
		/// <param name="actualRedirectUrl">The actual URL of the incoming HTTP request.</param>
		/// <param name="authorizationState">The authorization.</param>
		/// <returns>The granted authorization, or <c>null</c> if the incoming HTTP request did not contain an authorization server response or authorization was rejected.</returns>
		public IAuthorizationState ProcessUserAuthorization(Uri actualRedirectUrl, IAuthorizationState authorizationState = null) {
			Contract.Requires<ArgumentNullException>(actualRedirectUrl != null, "actualRedirectUrl");

			if (authorizationState == null) {
				authorizationState = new AuthorizationState();
			}

			var carrier = new HttpRequestInfo("GET", actualRedirectUrl, actualRedirectUrl.PathAndQuery, new System.Net.WebHeaderCollection(), null);
			IDirectedProtocolMessage response = this.Channel.ReadFromRequest(carrier);
			if (response == null) {
				return null;
			}

			EndUserAuthorizationSuccessResponse success;
			EndUserAuthorizationFailedResponse failure;
			if ((success = response as EndUserAuthorizationSuccessResponse) != null) {
				var accessTokenRequest = new AccessTokenAuthorizationCodeRequest(this.AuthorizationServer) {
					ClientIdentifier = this.ClientIdentifier,
					ClientSecret = this.ClientSecret,
					Callback = authorizationState.Callback,
					AuthorizationCode = success.AuthorizationCode,
				};
				IProtocolMessage accessTokenResponse = this.Channel.Request(accessTokenRequest);
				var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
				var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
				if (accessTokenSuccess != null) {
					this.UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
				} else if (failedAccessTokenResponse != null) {
					authorizationState.Delete();
					return null;
				} else {
					ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}
			} else if ((failure = response as EndUserAuthorizationFailedResponse) != null) {
				authorizationState.Delete();
				return null;
			}

			return authorizationState;
		}
	}
}
