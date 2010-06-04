//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerBase.cs" company="Andrew Arnott">
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
	using ChannelElements;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;
	using OAuth.ChannelElements;

	public abstract class AuthorizationServerBase {
		protected AuthorizationServerBase(IAuthorizationServer authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
			this.OAuthChannel = new OAuthWrapAuthorizationServerChannel(authorizationServer);
		}

		public Channel Channel {
			get { return this.OAuthChannel; }
		}

		public IAuthorizationServer AuthorizationServer {
			get { return this.OAuthChannel.AuthorizationServer; }
		}

		internal OAuthWrapAuthorizationServerChannel OAuthChannel { get; private set; }

		public virtual IDirectResponseProtocolMessage PrepareAccessTokenResponse(IAccessTokenRequest request, RSAParameters accessTokenEncryptingPublicKey, TimeSpan? accessTokenLifetime = null, bool includeRefreshToken = true) {
			Contract.Requires<ArgumentNullException>(request != null, "request");

			var tokenRequest = (ITokenCarryingRequest)request;
			var accessToken = new AccessToken(
				this.AuthorizationServer.AccessTokenSigningPrivateKey,
				accessTokenEncryptingPublicKey,
				tokenRequest.AuthorizationDescription,
				accessTokenLifetime);

			var response = new AccessTokenSuccessResponse(request) {
				Scope = tokenRequest.AuthorizationDescription.Scope,
				AccessToken = accessToken.Encode(),
				Lifetime = accessToken.Lifetime,
			};

			if (includeRefreshToken) {
				var refreshToken = new RefreshToken(this.AuthorizationServer.Secret, tokenRequest.AuthorizationDescription);
				response.RefreshToken = refreshToken.Encode();
			}

			return response;
		}
	}
}
