//-----------------------------------------------------------------------
// <copyright file="ClientBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// A base class for common OAuth Client behaviors.
	/// </summary>
	public class ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientBase"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientSecret">The client secret.</param>
		protected ClientBase(AuthorizationServerDescription authorizationServer, string clientIdentifier = null, string clientSecret = null) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
			this.Channel = new OAuth2ClientChannel();
			this.ClientIdentifier = clientIdentifier;
			this.ClientSecret = clientSecret;
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
		public Channel Channel { get; private set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret shared with the Authorization Server.
		/// </summary>
		public string ClientSecret { get; set; }

		/// <summary>
		/// Adds the necessary HTTP Authorization header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			Requires.NotNull(request, "request");
			Requires.NotNullOrEmpty(accessToken, "accessToken");

			OAuthUtilities.AuthorizeWithBearerToken(request, accessToken);
		}

		/// <summary>
		/// Adds the OAuth authorization token to an outgoing HTTP request, renewing a
		/// (nearly) expired access token if necessary.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="authorization">The authorization for this request previously obtained via OAuth.</param>
		public void AuthorizeRequest(HttpWebRequest request, IAuthorizationState authorization) {
			Requires.NotNull(request, "request");
			Requires.NotNull(authorization, "authorization");
			Requires.True(!string.IsNullOrEmpty(authorization.AccessToken), "authorization");
			Contract.Requires<ProtocolException>(!authorization.AccessTokenExpirationUtc.HasValue || authorization.AccessTokenExpirationUtc < DateTime.UtcNow || authorization.RefreshToken != null);

			if (authorization.AccessTokenExpirationUtc.HasValue && authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow) {
				ErrorUtilities.VerifyProtocol(authorization.RefreshToken != null, "Access token has expired and cannot be automatically refreshed.");
				this.RefreshAuthorization(authorization);
			}

			AuthorizeRequest(request, authorization.AccessToken);
		}

		/// <summary>
		/// Refreshes a short-lived access token using a longer-lived refresh token
		/// with a new access token that has the same scope as the refresh token.
		/// The refresh token itself may also be refreshed.
		/// </summary>
		/// <param name="authorization">The authorization to update.</param>
		/// <param name="skipIfUsefulLifeExceeds">If given, the access token will <em>not</em> be refreshed if its remaining lifetime exceeds this value.</param>
		/// <returns>A value indicating whether the access token was actually renewed; <c>true</c> if it was renewed, or <c>false</c> if it still had useful life remaining.</returns>
		/// <remarks>
		/// This method may modify the value of the <see cref="IAuthorizationState.RefreshToken"/> property on
		/// the <paramref name="authorization"/> parameter if the authorization server has cycled out your refresh token.
		/// If the parameter value was updated, this method calls <see cref="IAuthorizationState.SaveChanges"/> on that instance.
		/// </remarks>
		public bool RefreshAuthorization(IAuthorizationState authorization, TimeSpan? skipIfUsefulLifeExceeds = null) {
			Requires.NotNull(authorization, "authorization");
			Requires.True(!string.IsNullOrEmpty(authorization.RefreshToken), "authorization");

			if (skipIfUsefulLifeExceeds.HasValue && authorization.AccessTokenExpirationUtc.HasValue) {
				TimeSpan usefulLifeRemaining = authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
				if (usefulLifeRemaining > skipIfUsefulLifeExceeds.Value) {
					// There is useful life remaining in the access token.  Don't refresh.
					Logger.OAuth.DebugFormat("Skipping token refresh step because access token's remaining life is {0}, which exceeds {1}.", usefulLifeRemaining, skipIfUsefulLifeExceeds.Value);
					return false;
				}
			}

			var request = new AccessTokenRefreshRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				ClientSecret = this.ClientSecret,
				RefreshToken = authorization.RefreshToken,
			};

			var response = this.Channel.Request<AccessTokenSuccessResponse>(request);
			UpdateAuthorizationWithResponse(authorization, response);
			return true;
		}

		/// <summary>
		/// Gets an access token that may be used for only a subset of the scope for which a given
		/// refresh token is authorized.
		/// </summary>
		/// <param name="refreshToken">The refresh token.</param>
		/// <param name="scope">The scope subset desired in the access token.</param>
		/// <returns>A description of the obtained access token, and possibly a new refresh token.</returns>
		/// <remarks>
		/// If the return value includes a new refresh token, the old refresh token should be discarded and
		/// replaced with the new one.
		/// </remarks>
		public IAuthorizationState GetScopedAccessToken(string refreshToken, HashSet<string> scope) {
			Requires.NotNullOrEmpty(refreshToken, "refreshToken");
			Requires.NotNull(scope, "scope");
			Contract.Ensures(Contract.Result<IAuthorizationState>() != null);

			var request = new AccessTokenRefreshRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				ClientSecret = this.ClientSecret,
				RefreshToken = refreshToken,
			};

			var response = this.Channel.Request<AccessTokenSuccessResponse>(request);
			var authorization = new AuthorizationState();
			UpdateAuthorizationWithResponse(authorization, response);

			return authorization;
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
		internal void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAuthCodeResponse authorizationSuccess) {
			Requires.NotNull(authorizationState, "authorizationState");
			Requires.NotNull(authorizationSuccess, "authorizationSuccess");

			var accessTokenRequest = new AccessTokenAuthorizationCodeRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				ClientSecret = this.ClientSecret,
				Callback = authorizationState.Callback,
				AuthorizationCode = authorizationSuccess.AuthorizationCode,
			};
			IProtocolMessage accessTokenResponse = this.Channel.Request(accessTokenRequest);
			var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
			var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
			if (accessTokenSuccess != null) {
				UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
			} else {
				authorizationState.Delete();
				string error = failedAccessTokenResponse != null ? failedAccessTokenResponse.Error : "(unknown)";
				ErrorUtilities.ThrowProtocol(OAuthStrings.CannotObtainAccessTokenWithReason, error);
			}
		}

		/// <summary>
		/// Calculates the fraction of life remaining in an access token.
		/// </summary>
		/// <param name="authorization">The authorization to measure.</param>
		/// <returns>A fractional number no greater than 1.  Could be negative if the access token has already expired.</returns>
		private static double ProportionalLifeRemaining(IAuthorizationState authorization) {
			Requires.NotNull(authorization, "authorization");
			Requires.True(authorization.AccessTokenIssueDateUtc.HasValue, "authorization");
			Requires.True(authorization.AccessTokenExpirationUtc.HasValue, "authorization");

			// Calculate what % of the total life this access token has left.
			TimeSpan totalLifetime = authorization.AccessTokenExpirationUtc.Value - authorization.AccessTokenIssueDateUtc.Value;
			TimeSpan elapsedLifetime = DateTime.UtcNow - authorization.AccessTokenIssueDateUtc.Value;
			double proportionLifetimeRemaining = 1 - (elapsedLifetime.TotalSeconds / totalLifetime.TotalSeconds);
			return proportionLifetimeRemaining;
		}
	}
}
