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
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
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
		protected internal string ClientSecret { get; set; }

		/// <summary>
		/// Adds the necessary HTTP Authorization header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="accessToken">The access token previously obtained from the Authorization Server.</param>
		public void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(accessToken));

			OAuthUtilities.AuthorizeWithOAuthWrap(request, accessToken);
		}

		/// <summary>
		/// Adds the OAuth authorization token to an outgoing HTTP request, renewing a
		/// (nearly) expired access token if necessary.
		/// </summary>
		/// <param name="request">The request for protected resources from the service provider.</param>
		/// <param name="authorization">The authorization for this request previously obtained via OAuth.</param>
		public void AuthorizeRequest(HttpWebRequest request, IAuthorizationState authorization) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(authorization != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(authorization.AccessToken));
			Contract.Requires<ProtocolException>(!authorization.AccessTokenExpirationUtc.HasValue || authorization.AccessTokenExpirationUtc < DateTime.UtcNow || authorization.RefreshToken != null);

			if (authorization.AccessTokenExpirationUtc.HasValue && authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow) {
				ErrorUtilities.VerifyProtocol(authorization.RefreshToken != null, "Access token has expired and cannot be automatically refreshed.");
				this.RefreshToken(authorization);
			}

			this.AuthorizeRequest(request, authorization.AccessToken);
		}

		/// <summary>
		/// Refreshes a short-lived access token using a longer-lived refresh token.
		/// </summary>
		/// <param name="authorization">The authorization to update.</param>
		/// <param name="skipIfUsefulLifeExceeds">If given, the access token will <em>not</em> be refreshed if its remaining lifetime exceeds this value.</param>
		public void RefreshToken(IAuthorizationState authorization, TimeSpan? skipIfUsefulLifeExceeds = null) {
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(authorization.RefreshToken));

			if (skipIfUsefulLifeExceeds.HasValue && authorization.AccessTokenExpirationUtc.HasValue) {
				TimeSpan usefulLifeRemaining = authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
				if (usefulLifeRemaining > skipIfUsefulLifeExceeds.Value) {
					// There is useful life remaining in the access token.  Don't refresh.
					Logger.OAuth.DebugFormat("Skipping token refresh step because access token's remaining life is {0}, which exceeds {1}.", usefulLifeRemaining, skipIfUsefulLifeExceeds.Value);
					return;
				}
			}

			var request = new AccessTokenRefreshRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				ClientSecret = this.ClientSecret,
				RefreshToken = authorization.RefreshToken,
			};

			var response = this.Channel.Request<AccessTokenSuccessResponse>(request);
			authorization.AccessToken = response.AccessToken;
			authorization.AccessTokenExpirationUtc = DateTime.UtcNow + response.Lifetime;
			authorization.AccessTokenIssueDateUtc = DateTime.UtcNow;

			// Just in case the scope has changed...
			if (response.Scope != null) {
				authorization.Scope = response.Scope;
			}

			// The authorization server MAY choose to renew the refresh token itself.
			if (response.RefreshToken != null) {
				authorization.RefreshToken = response.RefreshToken;
			}

			authorization.SaveChanges();
		}

		/// <summary>
		/// Updates the authorization state maintained by the client with the content of an outgoing response.
		/// </summary>
		/// <param name="authorizationState">The authorization state maintained by the client.</param>
		/// <param name="accessTokenSuccess">The access token containing response message.</param>
		internal void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, AccessTokenSuccessResponse accessTokenSuccess) {
			Contract.Requires<ArgumentNullException>(authorizationState != null, "authorizationState");
			Contract.Requires<ArgumentNullException>(accessTokenSuccess != null, "accessTokenSuccess");

			authorizationState.AccessToken = accessTokenSuccess.AccessToken;
			authorizationState.RefreshToken = accessTokenSuccess.RefreshToken;
			authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
			authorizationState.AccessTokenIssueDateUtc = DateTime.UtcNow;
			if (accessTokenSuccess.Scope != null && accessTokenSuccess.Scope != authorizationState.Scope) {
				if (authorizationState.Scope != null) {
					Logger.OAuth.InfoFormat(
					                       "Requested scope of \"{0}\" changed to \"{1}\" by authorization server.",
					                       authorizationState.Scope,
					                       accessTokenSuccess.Scope);
				}

				authorizationState.Scope = accessTokenSuccess.Scope;
			}

			authorizationState.SaveChanges();
		}

		/// <summary>
		/// Updates the authorization state maintained by the client with the content of an outgoing response.
		/// </summary>
		/// <param name="authorizationState">The authorization state maintained by the client.</param>
		/// <param name="accessTokenSuccess">The access token containing response message.</param>
		internal void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAccessTokenResponse accessTokenSuccess) {
			Contract.Requires<ArgumentNullException>(authorizationState != null, "authorizationState");
			Contract.Requires<ArgumentNullException>(accessTokenSuccess != null, "accessTokenSuccess");

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

				authorizationState.Scope = accessTokenSuccess.Scope;
			}

			authorizationState.SaveChanges();
		}

		internal void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAuthCodeResponse authorizationSuccess) {
			Contract.Requires<ArgumentNullException>(authorizationState != null, "authorizationState");
			Contract.Requires<ArgumentNullException>(authorizationSuccess != null, "authorizationSuccess");

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
				this.UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
			} else {
				authorizationState.Delete();
				string error = failedAccessTokenResponse != null ? failedAccessTokenResponse.Error : "(unknown)";
				ErrorUtilities.ThrowProtocol(OAuthWrapStrings.CannotObtainAccessTokenWithReason, error);
			}
		}

		/// <summary>
		/// Calculates the fraction of life remaining in an access token.
		/// </summary>
		/// <param name="authorization">The authorization to measure.</param>
		/// <returns>A fractional number no greater than 1.  Could be negative if the access token has already expired.</returns>
		private double ProportionalLifeRemaining(IAuthorizationState authorization) {
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");
			Contract.Requires<ArgumentException>(authorization.AccessTokenIssueDateUtc.HasValue);
			Contract.Requires<ArgumentException>(authorization.AccessTokenExpirationUtc.HasValue);

			// Calculate what % of the total life this access token has left.
			TimeSpan totalLifetime = authorization.AccessTokenExpirationUtc.Value - authorization.AccessTokenIssueDateUtc.Value;
			TimeSpan elapsedLifetime = DateTime.UtcNow - authorization.AccessTokenIssueDateUtc.Value;
			double proportionLifetimeRemaining = 1 - (elapsedLifetime.TotalSeconds / totalLifetime.TotalSeconds);
			return proportionLifetimeRemaining;
		}
	}
}
