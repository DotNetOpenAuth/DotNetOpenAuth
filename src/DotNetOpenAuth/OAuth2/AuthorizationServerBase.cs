//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerBase.cs" company="Andrew Arnott">
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
	using ChannelElements;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;
	using OAuth.ChannelElements;

	/// <summary>
	/// A base class for authorization server facade classes.
	/// </summary>
	public abstract class AuthorizationServerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerBase"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		protected AuthorizationServerBase(IAuthorizationServer authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
			this.OAuthChannel = new OAuthWrapAuthorizationServerChannel(authorizationServer);
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
		public IAuthorizationServer AuthorizationServer {
			get { return this.OAuthChannel.AuthorizationServer; }
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		internal OAuthWrapAuthorizationServerChannel OAuthChannel { get; private set; }

		/// <summary>
		/// Prepares the response to an access token request.
		/// </summary>
		/// <param name="request">The request for an access token.</param>
		/// <param name="accessTokenEncryptingPublicKey">The public key to encrypt the access token to, such that the resource server will be able to decrypt it.</param>
		/// <param name="accessTokenLifetime">The access token's lifetime.</param>
		/// <param name="includeRefreshToken">If set to <c>true</c>, the response will include a long-lived refresh token.</param>
		/// <returns>The response message to send to the client.</returns>
		public virtual IDirectResponseProtocolMessage PrepareAccessTokenResponse(IAccessTokenRequest request, RSAParameters accessTokenEncryptingPublicKey, TimeSpan? accessTokenLifetime = null, bool includeRefreshToken = true) {
			Contract.Requires<ArgumentNullException>(request != null, "request");

			var tokenRequest = (ITokenCarryingRequest)request;
			var accessTokenFormatter = AccessToken.CreateFormatter(this.AuthorizationServer.AccessTokenSigningPrivateKey, accessTokenEncryptingPublicKey);
			var accessToken = new AccessToken(tokenRequest.AuthorizationDescription, accessTokenLifetime);

			var response = new AccessTokenSuccessResponse(request) {
				Scope = tokenRequest.AuthorizationDescription.Scope,
				AccessToken = accessTokenFormatter.Serialize(accessToken),
				Lifetime = accessToken.Lifetime,
			};

			if (includeRefreshToken) {
				var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.Secret);
				var refreshToken = new RefreshToken(tokenRequest.AuthorizationDescription);
				response.RefreshToken = refreshTokenFormatter.Serialize(refreshToken);
			}

			return response;
		}
	}
}
