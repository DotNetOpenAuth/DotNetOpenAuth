//-----------------------------------------------------------------------
// <copyright file="UserAgentClient.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using System.Web;

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
			: this(authorizationServer, clientIdentifier, DefaultSecretApplicator(clientSecret)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		/// <param name="tokenEndpoint">The token endpoint.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientSecret">The client secret.</param>
		public UserAgentClient(Uri authorizationEndpoint, Uri tokenEndpoint, string clientIdentifier = null, string clientSecret = null)
			: this(authorizationEndpoint, tokenEndpoint, clientIdentifier, DefaultSecretApplicator(clientSecret)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		/// <param name="tokenEndpoint">The token endpoint.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientCredentialApplicator">
		/// The tool to use to apply client credentials to authenticated requests to the Authorization Server.  
		/// May be <c>null</c> for clients with no secret or other means of authentication.
		/// </param>
		public UserAgentClient(Uri authorizationEndpoint, Uri tokenEndpoint, string clientIdentifier, ClientCredentialApplicator clientCredentialApplicator)
			: this(new AuthorizationServerDescription { AuthorizationEndpoint = authorizationEndpoint, TokenEndpoint = tokenEndpoint }, clientIdentifier, clientCredentialApplicator) {
			Requires.NotNull(authorizationEndpoint, "authorizationEndpoint");
			Requires.NotNull(tokenEndpoint, "tokenEndpoint");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientCredentialApplicator">
		/// The tool to use to apply client credentials to authenticated requests to the Authorization Server.  
		/// May be <c>null</c> for clients with no secret or other means of authentication.
		/// </param>
		public UserAgentClient(AuthorizationServerDescription authorizationServer, string clientIdentifier, ClientCredentialApplicator clientCredentialApplicator)
			: base(authorizationServer, clientIdentifier, clientCredentialApplicator) {
		}

		/// <summary>
		/// Generates a URL that the user's browser can be directed to in order to authorize
		/// this client to access protected data at some resource server.
		/// </summary>
		/// <param name="scope">The scope of authorized access requested.</param>
		/// <param name="state">The client state that should be returned with the authorization response.</param>
		/// <param name="returnTo">The URL that the authorization response should be sent to via a user-agent redirect.</param>
		/// <returns>
		/// A fully-qualified URL suitable to initiate the authorization flow.
		/// </returns>
		public Uri RequestUserAuthorization(IEnumerable<string> scope = null, string state = null, Uri returnTo = null) {
			var authorization = new AuthorizationState(scope) {
				Callback = returnTo,
			};

			return this.RequestUserAuthorization(authorization, state: state);
		}

		/// <summary>
		/// Generates a URL that the user's browser can be directed to in order to authorize
		/// this client to access protected data at some resource server.
		/// </summary>
		/// <param name="authorization">The authorization state that is tracking this particular request.  Optional.</param>
		/// <param name="implicitResponseType">
		/// <c>true</c> to request an access token in the fragment of the response's URL;
		/// <c>false</c> to authenticate to the authorization server and acquire the access token (and possibly a refresh token) via a private channel.
		/// </param>
		/// <param name="state">The client state that should be returned with the authorization response.</param>
		/// <returns>
		/// A fully-qualified URL suitable to initiate the authorization flow.
		/// </returns>
		public Uri RequestUserAuthorization(IAuthorizationState authorization, bool implicitResponseType = false, string state = null) {
			Requires.NotNull(authorization, "authorization");
			Requires.ValidState(!string.IsNullOrEmpty(this.ClientIdentifier));

			var request = this.PrepareRequestUserAuthorization(authorization, implicitResponseType, state);
			return this.Channel.PrepareResponse(request).GetDirectUriRequest(this.Channel);
		}

		/// <summary>
		/// Scans the incoming request for an authorization response message.
		/// </summary>
		/// <param name="actualRedirectUrl">The actual URL of the incoming HTTP request.</param>
		/// <param name="authorizationState">The authorization.</param>
		/// <returns>The granted authorization, or <c>null</c> if the incoming HTTP request did not contain an authorization server response or authorization was rejected.</returns>
		public IAuthorizationState ProcessUserAuthorization(Uri actualRedirectUrl, IAuthorizationState authorizationState = null) {
			Requires.NotNull(actualRedirectUrl, "actualRedirectUrl");

			if (authorizationState == null) {
				authorizationState = new AuthorizationState();
			}

			var carrier = new HttpRequestInfo("GET", actualRedirectUrl);
			IDirectedProtocolMessage response = this.Channel.ReadFromRequest(carrier);
			if (response == null) {
				return null;
			}

			return this.ProcessUserAuthorization(authorizationState, response);
		}

		/// <summary>
		/// Scans the incoming request for an authorization response message.
		/// </summary>
		/// <param name="authorizationState">The authorization.</param>
		/// <param name="response">The incoming authorization response message.</param>
		/// <returns>
		/// The granted authorization, or <c>null</c> if the incoming HTTP request did not contain an authorization server response or authorization was rejected.
		/// </returns>
		internal IAuthorizationState ProcessUserAuthorization(IAuthorizationState authorizationState, IDirectedProtocolMessage response) {
			Requires.NotNull(authorizationState, "authorizationState");
			Requires.NotNull(response, "response");

			EndUserAuthorizationSuccessAccessTokenResponse accessTokenSuccess;
			EndUserAuthorizationSuccessAuthCodeResponse authCodeSuccess;
			if ((accessTokenSuccess = response as EndUserAuthorizationSuccessAccessTokenResponse) != null) {
				UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
			} else if ((authCodeSuccess = response as EndUserAuthorizationSuccessAuthCodeResponse) != null) {
				this.UpdateAuthorizationWithResponse(authorizationState, authCodeSuccess);
			} else if (response is EndUserAuthorizationFailedResponse) {
				authorizationState.Delete();
				return null;
			}

			return authorizationState;
		}

		/// <summary>
		/// Generates a URL that the user's browser can be directed to in order to authorize
		/// this client to access protected data at some resource server.
		/// </summary>
		/// <param name="authorization">The authorization state that is tracking this particular request.  Optional.</param>
		/// <param name="implicitResponseType">
		/// <c>true</c> to request an access token in the fragment of the response's URL;
		/// <c>false</c> to authenticate to the authorization server and acquire the access token (and possibly a refresh token) via a private channel.
		/// </param>
		/// <param name="state">The client state that should be returned with the authorization response.</param>
		/// <returns>
		/// A message to send to the authorization server.
		/// </returns>
		internal EndUserAuthorizationRequest PrepareRequestUserAuthorization(IAuthorizationState authorization, bool implicitResponseType = false, string state = null) {
			Requires.NotNull(authorization, "authorization");
			Requires.ValidState(!string.IsNullOrEmpty(this.ClientIdentifier));

			if (authorization.Callback == null) {
				authorization.Callback = new Uri("http://localhost/");
			}

			var request = implicitResponseType ? (EndUserAuthorizationRequest)new EndUserAuthorizationImplicitRequestC(this.AuthorizationServer) : new EndUserAuthorizationRequestC(this.AuthorizationServer);
			request.ClientIdentifier = this.ClientIdentifier;
			request.Callback = authorization.Callback;
			request.ClientState = state;
			request.Scope.ResetContents(authorization.Scope);

			return request;
		}
	}
}
