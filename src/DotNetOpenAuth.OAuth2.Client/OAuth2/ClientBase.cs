//-----------------------------------------------------------------------
// <copyright file="ClientBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Security;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// A base class for common OAuth Client behaviors.
	/// </summary>
	public class ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientBase" /> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientCredentialApplicator">The tool to use to apply client credentials to authenticated requests to the Authorization Server.
		/// May be <c>null</c> for clients with no secret or other means of authentication.</param>
		/// <param name="hostFactories">The host factories.</param>
		protected ClientBase(AuthorizationServerDescription authorizationServer, string clientIdentifier = null, ClientCredentialApplicator clientCredentialApplicator = null, IHostFactories hostFactories = null) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
			this.Channel = new OAuth2ClientChannel(hostFactories);
			this.ClientIdentifier = clientIdentifier;
			this.ClientCredentialApplicator = clientCredentialApplicator;
		}

		/// <summary>
		/// Gets the token issuer.
		/// </summary>
		/// <value>The token issuer.</value>
		public AuthorizationServerDescription AuthorizationServer { get; private set; }

		/// <summary>
		/// Gets the OAuth channel.
		/// </summary>
		/// <value>The channel.</value>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		public string ClientIdentifier {
			get { return this.OAuthChannel.ClientIdentifier; }
			set { this.OAuthChannel.ClientIdentifier = value; }
		}

		/// <summary>
		/// Gets or sets the tool to use to apply client credentials to authenticated requests to the Authorization Server.
		/// </summary>
		/// <value>May be <c>null</c> if this client has no client secret.</value>
		public ClientCredentialApplicator ClientCredentialApplicator {
			get { return this.OAuthChannel.ClientCredentialApplicator; }
			set { this.OAuthChannel.ClientCredentialApplicator = value; }
		}

		/// <summary>
		/// Gets quotas used when deserializing JSON.
		/// </summary>
		public XmlDictionaryReaderQuotas JsonReaderQuotas {
			get { return this.OAuthChannel.JsonReaderQuotas; }
		}

		/// <summary>
		/// Gets the OAuth client channel.
		/// </summary>
		internal IOAuth2ChannelWithClient OAuthChannel {
			get { return (IOAuth2ChannelWithClient)this.Channel; }
		}

		/// <summary>
		/// Adds the necessary HTTP Authorization header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			Requires.NotNull(request, "request");
			Requires.NotNullOrEmpty(accessToken, "accessToken");

			AuthorizeRequest(request.Headers, accessToken);
		}

		/// <summary>
		/// Adds the necessary HTTP Authorization header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="requestHeaders">The headers on the request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public static void AuthorizeRequest(WebHeaderCollection requestHeaders, string accessToken) {
			Requires.NotNull(requestHeaders, "requestHeaders");
			Requires.NotNullOrEmpty(accessToken, "accessToken");

			OAuthUtilities.AuthorizeWithBearerToken(requestHeaders, accessToken);
		}

		/// <summary>
		/// Adds the OAuth authorization token to an outgoing HTTP request, renewing a
		/// (nearly) expired access token if necessary.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="authorization">The authorization for this request previously obtained via OAuth.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public Task AuthorizeRequestAsync(HttpWebRequest request, IAuthorizationState authorization, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");
			Requires.NotNull(authorization, "authorization");

			return this.AuthorizeRequestAsync(request.Headers, authorization, cancellationToken);
		}

		/// <summary>
		/// Adds the OAuth authorization token to an outgoing HTTP request, renewing a
		/// (nearly) expired access token if necessary.
		/// </summary>
		/// <param name="requestHeaders">The headers on the request for protected resources from the service provider.</param>
		/// <param name="authorization">The authorization for this request previously obtained via OAuth.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public async Task AuthorizeRequestAsync(WebHeaderCollection requestHeaders, IAuthorizationState authorization, CancellationToken cancellationToken) {
			Requires.NotNull(requestHeaders, "requestHeaders");
			Requires.NotNull(authorization, "authorization");
			Requires.That(!string.IsNullOrEmpty(authorization.AccessToken), "authorization", "AccessToken required.");
			ErrorUtilities.VerifyProtocol(!authorization.AccessTokenExpirationUtc.HasValue || authorization.AccessTokenExpirationUtc >= DateTime.UtcNow || authorization.RefreshToken != null, ClientStrings.AuthorizationExpired);

			if (authorization.AccessTokenExpirationUtc.HasValue && authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow) {
				ErrorUtilities.VerifyProtocol(authorization.RefreshToken != null, ClientStrings.AccessTokenRefreshFailed);
				await this.RefreshAuthorizationAsync(authorization, cancellationToken: cancellationToken);
			}

			AuthorizeRequest(requestHeaders, authorization.AccessToken);
		}

		/// <summary>
		/// Creates an HTTP handler that automatically applies an OAuth 2 (bearer) access token to outbound HTTP requests.
		/// The result of this method can be supplied to the <see cref="HttpClient(HttpMessageHandler)"/> constructor.
		/// </summary>
		/// <param name="bearerAccessToken">The bearer token to apply to each outbound HTTP message.</param>
		/// <param name="innerHandler">The inner HTTP handler to use.  The default uses <see cref="HttpClientHandler"/> as the inner handler.</param>
		/// <returns>An <see cref="HttpMessageHandler"/> instance.</returns>
		public DelegatingHandler CreateAuthorizingHandler(string bearerAccessToken, HttpMessageHandler innerHandler = null) {
			Requires.NotNullOrEmpty(bearerAccessToken, "bearerAccessToken");
			return new BearerTokenHttpMessageHandler(bearerAccessToken, innerHandler ?? new HttpClientHandler());
		}

		/// <summary>
		/// Creates an HTTP handler that automatically applies the OAuth 2 access token to outbound HTTP requests.
		/// The result of this method can be supplied to the <see cref="HttpClient(HttpMessageHandler)"/> constructor.
		/// </summary>
		/// <param name="authorization">The authorization to apply to the message.</param>
		/// <param name="innerHandler">The inner HTTP handler to use.  The default uses <see cref="HttpClientHandler"/> as the inner handler.</param>
		/// <returns>An <see cref="HttpMessageHandler"/> instance.</returns>
		public DelegatingHandler CreateAuthorizingHandler(IAuthorizationState authorization, HttpMessageHandler innerHandler = null) {
			Requires.NotNull(authorization, "authorization");
			return new BearerTokenHttpMessageHandler(this, authorization, innerHandler ?? new HttpClientHandler());
		}

		/// <summary>
		/// Refreshes a short-lived access token using a longer-lived refresh token
		/// with a new access token that has the same scope as the refresh token.
		/// The refresh token itself may also be refreshed.
		/// </summary>
		/// <param name="authorization">The authorization to update.</param>
		/// <param name="skipIfUsefulLifeExceeds">If given, the access token will <em>not</em> be refreshed if its remaining lifetime exceeds this value.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A value indicating whether the access token was actually renewed; <c>true</c> if it was renewed, or <c>false</c> if it still had useful life remaining.</returns>
		/// <remarks>
		/// This method may modify the value of the <see cref="IAuthorizationState.RefreshToken"/> property on
		/// the <paramref name="authorization"/> parameter if the authorization server has cycled out your refresh token.
		/// If the parameter value was updated, this method calls <see cref="IAuthorizationState.SaveChanges"/> on that instance.
		/// </remarks>
		public async Task<bool> RefreshAuthorizationAsync(IAuthorizationState authorization, TimeSpan? skipIfUsefulLifeExceeds = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(authorization, "authorization");
			Requires.That(!string.IsNullOrEmpty(authorization.RefreshToken), "authorization", "RefreshToken required.");

			if (skipIfUsefulLifeExceeds.HasValue && authorization.AccessTokenExpirationUtc.HasValue) {
				TimeSpan usefulLifeRemaining = authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
				if (usefulLifeRemaining > skipIfUsefulLifeExceeds.Value) {
					// There is useful life remaining in the access token.  Don't refresh.
					Logger.OAuth.DebugFormat("Skipping token refresh step because access token's remaining life is {0}, which exceeds {1}.", usefulLifeRemaining, skipIfUsefulLifeExceeds.Value);
					return false;
				}
			}

			var request = new AccessTokenRefreshRequestC(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				RefreshToken = authorization.RefreshToken,
			};

			this.ApplyClientCredential(request);

			var response = await this.Channel.RequestAsync<AccessTokenSuccessResponse>(request, cancellationToken);
			UpdateAuthorizationWithResponse(authorization, response);
			return true;
		}

		/// <summary>
		/// Gets an access token that may be used for only a subset of the scope for which a given
		/// refresh token is authorized.
		/// </summary>
		/// <param name="refreshToken">The refresh token.</param>
		/// <param name="scope">The scope subset desired in the access token.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A description of the obtained access token, and possibly a new refresh token.</returns>
		/// <remarks>
		/// If the return value includes a new refresh token, the old refresh token should be discarded and
		/// replaced with the new one.
		/// </remarks>
		public async Task<IAuthorizationState> GetScopedAccessTokenAsync(string refreshToken, HashSet<string> scope, CancellationToken cancellationToken) {
			Requires.NotNullOrEmpty(refreshToken, "refreshToken");
			Requires.NotNull(scope, "scope");

			var request = new AccessTokenRefreshRequestC(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				RefreshToken = refreshToken,
			};

			this.ApplyClientCredential(request);

			var response = await this.Channel.RequestAsync<AccessTokenSuccessResponse>(request, cancellationToken);
			var authorization = new AuthorizationState();
			UpdateAuthorizationWithResponse(authorization, response);

			return authorization;
		}

		/// <summary>
		/// Exchanges a resource owner's password credential for OAuth 2.0 refresh and access tokens.
		/// </summary>
		/// <param name="userName">The resource owner's username, as it is known by the authorization server.</param>
		/// <param name="password">The resource owner's account password.</param>
		/// <param name="scopes">The desired scope of access.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The result, containing the tokens if successful.
		/// </returns>
		public Task<IAuthorizationState> ExchangeUserCredentialForTokenAsync(string userName, string password, IEnumerable<string> scopes = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNullOrEmpty(userName, "userName");
			Requires.NotNull(password, "password");

			var request = new AccessTokenResourceOwnerPasswordCredentialsRequest(this.AuthorizationServer.TokenEndpoint, this.AuthorizationServer.Version) {
				RequestingUserName = userName,
				Password = password,
			};

			return this.RequestAccessTokenAsync(request, scopes, cancellationToken);
		}

		/// <summary>
		/// Obtains an access token for accessing client-controlled resources on the resource server.
		/// </summary>
		/// <param name="scopes">The desired scopes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The result of the authorization request.
		/// </returns>
		public Task<IAuthorizationState> GetClientAccessTokenAsync(IEnumerable<string> scopes = null, CancellationToken cancellationToken = default(CancellationToken)) {
			var request = new AccessTokenClientCredentialsRequest(this.AuthorizationServer.TokenEndpoint, this.AuthorizationServer.Version);
			return this.RequestAccessTokenAsync(request, scopes, cancellationToken);
		}

		/// <summary>
		/// Updates the authorization state maintained by the client with the content of an outgoing response.
		/// </summary>
		/// <param name="authorizationState">The authorization state maintained by the client.</param>
		/// <param name="accessTokenSuccess">The access token containing response message.</param>
		internal static void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, AccessTokenSuccessResponse accessTokenSuccess) {
			Requires.NotNull(authorizationState, "authorizationState");
			Requires.NotNull(accessTokenSuccess, "accessTokenSuccess");

			authorizationState.AccessToken = accessTokenSuccess.AccessToken;
			authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
			authorizationState.AccessTokenIssueDateUtc = DateTime.UtcNow;

			// The authorization server MAY choose to renew the refresh token itself.
			if (accessTokenSuccess.RefreshToken != null) {
				authorizationState.RefreshToken = accessTokenSuccess.RefreshToken;
			}

			// An included scope parameter in the response only describes the access token's scope.
			// Don't update the whole authorization state object with that scope because that represents
			// the refresh token's original scope.
			if ((authorizationState.Scope == null || authorizationState.Scope.Count == 0) && accessTokenSuccess.Scope != null) {
				authorizationState.Scope.ResetContents(accessTokenSuccess.Scope);
			}

			authorizationState.SaveChanges();
		}

		/// <summary>
		/// Updates the authorization state maintained by the client with the content of an outgoing response.
		/// </summary>
		/// <param name="authorizationState">The authorization state maintained by the client.</param>
		/// <param name="accessTokenSuccess">The access token containing response message.</param>
		internal static void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAccessTokenResponse accessTokenSuccess) {
			Requires.NotNull(authorizationState, "authorizationState");
			Requires.NotNull(accessTokenSuccess, "accessTokenSuccess");

			authorizationState.AccessToken = accessTokenSuccess.AccessToken;
			authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
			authorizationState.AccessTokenIssueDateUtc = DateTime.UtcNow;
			if (accessTokenSuccess.Scope != null && accessTokenSuccess.Scope != authorizationState.Scope) {
				if (authorizationState.Scope != null) {
					Logger.OAuth.InfoFormat(
										   "Requested scope of \"{0}\" changed to \"{1}\" by authorization server.",
										   authorizationState.Scope,
										   accessTokenSuccess.Scope);
				}

				authorizationState.Scope.ResetContents(accessTokenSuccess.Scope);
			}

			authorizationState.SaveChanges();
		}

		/// <summary>
		/// Updates authorization state with a success response from the Authorization Server.
		/// </summary>
		/// <param name="authorizationState">The authorization state to update.</param>
		/// <param name="authorizationSuccess">The authorization success message obtained from the authorization server.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		internal async Task UpdateAuthorizationWithResponseAsync(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAuthCodeResponse authorizationSuccess, CancellationToken cancellationToken) {
			Requires.NotNull(authorizationState, "authorizationState");
			Requires.NotNull(authorizationSuccess, "authorizationSuccess");

			var accessTokenRequest = new AccessTokenAuthorizationCodeRequestC(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorizationState.Callback,
				AuthorizationCode = authorizationSuccess.AuthorizationCode,
			};
			this.ApplyClientCredential(accessTokenRequest);
			IProtocolMessage accessTokenResponse = await this.Channel.RequestAsync(accessTokenRequest, cancellationToken);
			var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
			var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
			if (accessTokenSuccess != null) {
				UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
			} else {
				authorizationState.Delete();
				string error = failedAccessTokenResponse != null ? failedAccessTokenResponse.Error : "(unknown)";
				ErrorUtilities.ThrowProtocol(ClientStrings.CannotObtainAccessTokenWithReason, error);
			}
		}

		/// <summary>
		/// Applies the default client authentication mechanism given a client secret.
		/// </summary>
		/// <param name="secret">The client secret.  May be <c>null</c></param>
		/// <returns>The client credential applicator.</returns>
		protected static ClientCredentialApplicator DefaultSecretApplicator(string secret) {
			return secret == null ? ClientCredentialApplicator.NoSecret() : ClientCredentialApplicator.NetworkCredential(secret);
		}

		/// <summary>
		/// Applies any applicable client credential to an authenticated outbound request to the authorization server.
		/// </summary>
		/// <param name="request">The request to apply authentication information to.</param>
		protected void ApplyClientCredential(AuthenticatedClientRequestBase request) {
			Requires.NotNull(request, "request");

			if (this.ClientCredentialApplicator != null) {
				this.ClientCredentialApplicator.ApplyClientCredential(this.ClientIdentifier, request);
			}
		}

		/// <summary>
		/// Calculates the fraction of life remaining in an access token.
		/// </summary>
		/// <param name="authorization">The authorization to measure.</param>
		/// <returns>A fractional number no greater than 1.  Could be negative if the access token has already expired.</returns>
		private static double ProportionalLifeRemaining(IAuthorizationState authorization) {
			Requires.NotNull(authorization, "authorization");
			Requires.That(authorization.AccessTokenIssueDateUtc.HasValue, "authorization", "AccessTokenIssueDateUtc required");
			Requires.That(authorization.AccessTokenExpirationUtc.HasValue, "authorization", "AccessTokenExpirationUtc required");

			// Calculate what % of the total life this access token has left.
			TimeSpan totalLifetime = authorization.AccessTokenExpirationUtc.Value - authorization.AccessTokenIssueDateUtc.Value;
			TimeSpan elapsedLifetime = DateTime.UtcNow - authorization.AccessTokenIssueDateUtc.Value;
			double proportionLifetimeRemaining = 1 - (elapsedLifetime.TotalSeconds / totalLifetime.TotalSeconds);
			return proportionLifetimeRemaining;
		}

		/// <summary>
		/// Requests an access token using a partially .initialized request message.
		/// </summary>
		/// <param name="request">The request message.</param>
		/// <param name="scopes">The scopes requested by the client.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The result of the request.
		/// </returns>
		private async Task<IAuthorizationState> RequestAccessTokenAsync(ScopedAccessTokenRequest request, IEnumerable<string> scopes, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			var authorizationState = new AuthorizationState(scopes);

			request.ClientIdentifier = this.ClientIdentifier;
			this.ApplyClientCredential(request);
			request.Scope.UnionWith(authorizationState.Scope);

			var response = await this.Channel.RequestAsync(request, cancellationToken);
			var success = response as AccessTokenSuccessResponse;
			var failure = response as AccessTokenFailedResponse;
			ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
			if (success != null) {
				authorizationState.Scope.Clear(); // clear the scope we requested so that the response will repopulate it.
				UpdateAuthorizationWithResponse(authorizationState, success);
			} else { // failure
				Logger.OAuth.Info("Credentials rejected by the Authorization Server.");
				authorizationState.Delete();
			}

			return authorizationState;
		}
	}
}
