//-----------------------------------------------------------------------
// <copyright file="AuthorizationServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net.Http;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// Authorization Server supporting the web server flow.
	/// </summary>
	public class AuthorizationServer {
		/// <summary>
		/// A reusable instance of the scope satisfied checker.
		/// </summary>
		private static readonly IScopeSatisfiedCheck DefaultScopeSatisfiedCheck = new StandardScopeSatisfiedCheck();

		/// <summary>
		/// The list of modules that verify client authentication data.
		/// </summary>
		private readonly List<ClientAuthenticationModule> clientAuthenticationModules = new List<ClientAuthenticationModule>();

		/// <summary>
		/// The lone aggregate client authentication module that uses the <see cref="clientAuthenticationModules"/> and applies aggregating policy.
		/// </summary>
		private readonly ClientAuthenticationModule aggregatingClientAuthenticationModule;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServer"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		public AuthorizationServer(IAuthorizationServerHost authorizationServer) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.aggregatingClientAuthenticationModule = new AggregatingClientCredentialReader(this.clientAuthenticationModules);
			this.Channel = new OAuth2AuthorizationServerChannel(authorizationServer, this.aggregatingClientAuthenticationModule);
			this.clientAuthenticationModules.AddRange(OAuth2AuthorizationServerSection.Configuration.ClientAuthenticationModules.CreateInstances(true, null));
			this.ScopeSatisfiedCheck = DefaultScopeSatisfiedCheck;
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
		public IAuthorizationServerHost AuthorizationServerServices {
			get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).AuthorizationServer; }
		}

		/// <summary>
		/// Gets the extension modules that can read client authentication data from incoming messages.
		/// </summary>
		public IList<ClientAuthenticationModule> ClientAuthenticationModules {
			get { return this.clientAuthenticationModules; }
		}

		/// <summary>
		/// Gets or sets the service that checks whether a granted set of scopes satisfies a required set of scopes.
		/// </summary>
		public IScopeSatisfiedCheck ScopeSatisfiedCheck {
			get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).ScopeSatisfiedCheck; }
			set { ((IOAuth2ChannelWithAuthorizationServer)this.Channel).ScopeSatisfiedCheck = value; }
		}

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "unauthorizedclient", Justification = "Protocol required.")]
		public async Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");

			var message = await this.Channel.TryReadFromRequestAsync<EndUserAuthorizationRequest>(request, cancellationToken);
			if (message != null) {
				if (message.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
					// Clients with no secrets can only request implicit grant types.
					var client = this.AuthorizationServerServices.GetClientOrThrow(message.ClientIdentifier);
					ErrorUtilities.VerifyProtocol(client.HasNonEmptySecret, Protocol.EndUserAuthorizationRequestErrorCodes.UnauthorizedClient);
				}
			}

			return message;
		}

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(
			HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();
			return this.ReadAuthorizationRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Reads in a client's request for the Authorization Server to obtain permission from
		/// the user to authorize the Client's access of some protected resource(s).
		/// </summary>
		/// <param name="requestUri">The URL that carries the authorization request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(Uri requestUri, CancellationToken cancellationToken = default(CancellationToken)) {
			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			return this.ReadAuthorizationRequestAsync(request, cancellationToken);
		}

		/// <summary>
		/// Handles an incoming request to the authorization server's token endpoint.
		/// </summary>
		/// <param name="request">The HTTP request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response to send to the client.
		/// </returns>
		public async Task<HttpResponseMessage> HandleTokenRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");

			IProtocolMessage responseMessage;
			try {
				AccessTokenRequestBase requestMessage = await this.Channel.TryReadFromRequestAsync<AccessTokenRequestBase>(request, cancellationToken);
				if (requestMessage != null) {
					var accessTokenResult = this.AuthorizationServerServices.CreateAccessToken(requestMessage);
					ErrorUtilities.VerifyHost(accessTokenResult != null, "IAuthorizationServerHost.CreateAccessToken must not return null.");

					IAccessTokenRequestInternal accessRequestInternal = requestMessage;
					accessRequestInternal.AccessTokenResult = accessTokenResult;

					var successResponseMessage = this.PrepareAccessTokenResponse(requestMessage, accessTokenResult.AllowRefreshToken);
					successResponseMessage.Lifetime = accessTokenResult.AccessToken.Lifetime;

					var authCarryingRequest = requestMessage as IAuthorizationCarryingRequest;
					if (authCarryingRequest != null) {
						accessTokenResult.AccessToken.ApplyAuthorization(authCarryingRequest.AuthorizationDescription);
						IAccessTokenIssuingResponse accessTokenIssuingResponse = successResponseMessage;
						accessTokenIssuingResponse.AuthorizationDescription = accessTokenResult.AccessToken;
					}

					responseMessage = successResponseMessage;
				} else {
					responseMessage = new AccessTokenFailedResponse() { Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest };
				}
			} catch (TokenEndpointProtocolException ex) {
				responseMessage = ex.GetResponse();
			} catch (ProtocolException) {
				responseMessage = new AccessTokenFailedResponse() { Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest };
			}

			return await this.Channel.PrepareResponseAsync(responseMessage, cancellationToken);
		}

		/// <summary>
		/// Handles an incoming request to the authorization server's token endpoint.
		/// </summary>
		/// <param name="request">The HTTP request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response to send to the client.
		/// </returns>
		public Task<HttpResponseMessage> HandleTokenRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();
			return this.HandleTokenRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Prepares a response to inform the Client that the user has rejected the Client's authorization request.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request.</param>
		/// <param name="callback">The Client callback URL to use when formulating the redirect to send the user agent back to the Client.</param>
		/// <returns>The authorization response message to send to the Client.</returns>
		public EndUserAuthorizationFailedResponse PrepareRejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");

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

			if (callback == null) {
				callback = this.GetCallback(authorizationRequest);
			}

			var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);
			EndUserAuthorizationSuccessResponseBase response;
			switch (authorizationRequest.ResponseType) {
				case EndUserAuthorizationResponseType.AccessToken:
					IAccessTokenRequestInternal accessRequestInternal = (EndUserAuthorizationImplicitRequest)authorizationRequest;
					var accessTokenResult = this.AuthorizationServerServices.CreateAccessToken(accessRequestInternal);
					ErrorUtilities.VerifyHost(accessTokenResult != null, "IAuthorizationServerHost.CreateAccessToken must not return null.");

					accessRequestInternal.AccessTokenResult = accessTokenResult;

					var implicitGrantResponse = new EndUserAuthorizationSuccessAccessTokenResponse(callback, authorizationRequest);
					implicitGrantResponse.Lifetime = accessTokenResult.AccessToken.Lifetime;
					accessTokenResult.AccessToken.ApplyAuthorization(implicitGrantResponse.Scope, userName, implicitGrantResponse.Lifetime);

					IAccessTokenCarryingRequest tokenCarryingResponse = implicitGrantResponse;
					tokenCarryingResponse.AuthorizationDescription = accessTokenResult.AccessToken;

					response = implicitGrantResponse;
					break;
				case EndUserAuthorizationResponseType.AuthorizationCode:
					var authCodeResponse = new EndUserAuthorizationSuccessAuthCodeResponseAS(callback, authorizationRequest);
					IAuthorizationCodeCarryingRequest codeCarryingResponse = authCodeResponse;
					codeCarryingResponse.AuthorizationDescription = new AuthorizationCode(
						authorizationRequest.ClientIdentifier,
						authorizationRequest.Callback,
						authCodeResponse.Scope,
						userName);
					response = authCodeResponse;
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
		/// Decodes a refresh token into its authorization details.
		/// </summary>
		/// <param name="refreshToken">The encoded refresh token as it would appear to the client.</param>
		/// <returns>A description of the authorization represented by the refresh token.</returns>
		/// <exception cref="ProtocolException">Thrown if the refresh token is not valid due to expiration, corruption or not being authentic.</exception>
		/// <remarks>
		/// This can be useful if the authorization server supports the client revoking its own access (on uninstall, for example).
		/// Outside the scope of the OAuth 2 spec, the client may contact the authorization server host requesting that its refresh
		/// token be revoked.  The authorization server would need to decode the refresh token so it knows which authorization in
		/// the database to delete.
		/// </remarks>
		public IAuthorizationDescription DecodeRefreshToken(string refreshToken) {
			var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServerServices.CryptoKeyStore);
			var token = new RefreshToken();
			refreshTokenFormatter.Deserialize(token, refreshToken);
			return token;
		}

		/// <summary>
		/// Gets the redirect URL to use for a particular authorization request.
		/// </summary>
		/// <param name="authorizationRequest">The authorization request.</param>
		/// <returns>The URL to redirect to.  Never <c>null</c>.</returns>
		/// <exception cref="ProtocolException">Thrown if no callback URL could be determined.</exception>
		protected Uri GetCallback(EndUserAuthorizationRequest authorizationRequest) {
			Requires.NotNull(authorizationRequest, "authorizationRequest");

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
			ErrorUtilities.VerifyProtocol(defaultCallback != null, AuthServerStrings.NoCallback);
			return defaultCallback;
		}

		/// <summary>
		/// Prepares the response to an access token request.
		/// </summary>
		/// <param name="request">The request for an access token.</param>
		/// <param name="allowRefreshToken">If set to <c>true</c>, the response will include a long-lived refresh token.</param>
		/// <returns>The response message to send to the client.</returns>
		private AccessTokenSuccessResponse PrepareAccessTokenResponse(AccessTokenRequestBase request, bool allowRefreshToken = true) {
			Requires.NotNull(request, "request");

			if (allowRefreshToken) {
				if (request is AccessTokenClientCredentialsRequest) {
					// Per OAuth 2.0 section 4.4.3 (draft 23), refresh tokens should never be included
					// in a response to an access token request that used the client credential grant type.
					Logger.OAuth.Debug("Suppressing refresh token in access token response because the grant type used by the client disallows it.");
					allowRefreshToken = false;
				}
			}

			var tokenRequest = (IAuthorizationCarryingRequest)request;
			var accessTokenRequest = (IAccessTokenRequestInternal)request;
			var response = new AccessTokenSuccessResponse(request) {
				HasRefreshToken = allowRefreshToken,
			};
			response.Scope.ResetContents(tokenRequest.AuthorizationDescription.Scope);
			return response;
		}
	}
}
