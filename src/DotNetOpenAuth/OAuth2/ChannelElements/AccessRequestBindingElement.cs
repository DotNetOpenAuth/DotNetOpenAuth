//-----------------------------------------------------------------------
// <copyright file="AccessRequestBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.Messages;
	using System.Security.Cryptography;

	/// <summary>
	/// Decodes verification codes, refresh tokens and access tokens on incoming messages.
	/// </summary>
	/// <remarks>
	/// This binding element also ensures that the code/token coming in is issued to
	/// the same client that is sending the code/token and that the authorization has
	/// not been revoked and that an access token has not expired.
	/// </remarks>
	internal class AccessRequestBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessRequestBindingElement"/> class.
		/// </summary>
		internal AccessRequestBindingElement() {
		}

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public override MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			var response = message as IAuthorizationCarryingRequest;
			if (response != null) {
				switch (response.CodeOrTokenType) {
					case CodeOrTokenType.AuthorizationCode:
						var codeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
						var code = (AuthorizationCode)response.AuthorizationDescription;
						response.CodeOrToken = codeFormatter.Serialize(code);
						break;
					case CodeOrTokenType.AccessToken:
						var responseWithOriginatingRequest = (IDirectResponseProtocolMessage)message;
						var request = (IAccessTokenRequest)responseWithOriginatingRequest.OriginatingRequest;

						RSACryptoServiceProvider resourceServerKey;
						TimeSpan lifetime;
						this.AuthorizationServer.PrepareAccessToken(request, out resourceServerKey, out lifetime);
						try {
							var tokenFormatter = AccessToken.CreateFormatter(this.AuthorizationServer.AccessTokenSigningKey, resourceServerKey);
							var token = (AccessToken)response.AuthorizationDescription;
							response.CodeOrToken = tokenFormatter.Serialize(token);
							break;
						} finally {
							IDisposable disposableKey = resourceServerKey;
							disposableKey.Dispose();
						}
					default:
						throw ErrorUtilities.ThrowInternal(string.Format(CultureInfo.CurrentCulture, "Unexpected outgoing code or token type: {0}", response.CodeOrTokenType));
				}

				return MessageProtections.None;
			}

			var accessTokenResponse = message as AccessTokenSuccessResponse;
			if (accessTokenResponse != null) {
				var directResponseMessage = (IDirectResponseProtocolMessage)accessTokenResponse;
				var accessTokenRequest = (AccessTokenRequestBase)directResponseMessage.OriginatingRequest;
				ErrorUtilities.VerifyProtocol(accessTokenRequest.GrantType != GrantType.ClientCredentials || accessTokenResponse.RefreshToken == null, OAuthStrings.NoGrantNoRefreshToken);
			}

			return null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var tokenRequest = message as IAuthorizationCarryingRequest;
			if (tokenRequest != null) {
				try {
					switch (tokenRequest.CodeOrTokenType) {
						case CodeOrTokenType.AuthorizationCode:
							var verificationCodeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
							var verificationCode = verificationCodeFormatter.Deserialize(message, tokenRequest.CodeOrToken);
							tokenRequest.AuthorizationDescription = verificationCode;
							break;
						case CodeOrTokenType.RefreshToken:
							var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
							var refreshToken = refreshTokenFormatter.Deserialize(message, tokenRequest.CodeOrToken);
							tokenRequest.AuthorizationDescription = refreshToken;
							break;
						default:
							throw ErrorUtilities.ThrowInternal("Unexpected value for CodeOrTokenType: " + tokenRequest.CodeOrTokenType);
					}
				} catch (ExpiredMessageException ex) {
					throw ErrorUtilities.Wrap(ex, Protocol.authorization_expired);
				}

				var accessRequest = tokenRequest as AccessTokenRequestBase;
				if (accessRequest != null) {
					// Make sure the client sending us this token is the client we issued the token to.
					ErrorUtilities.VerifyProtocol(string.Equals(accessRequest.ClientIdentifier, tokenRequest.AuthorizationDescription.ClientIdentifier, StringComparison.Ordinal), Protocol.incorrect_client_credentials);

					// Check that the client secret is correct.
					var client = this.AuthorizationServer.GetClientOrThrow(accessRequest.ClientIdentifier);
					ErrorUtilities.VerifyProtocol(MessagingUtilities.EqualsConstantTime(client.Secret, accessRequest.ClientSecret), Protocol.incorrect_client_credentials);

					var scopedAccessRequest = accessRequest as ScopedAccessTokenRequest;
					if (scopedAccessRequest != null) {
						// Make sure the scope the client is requesting does not exceed the scope in the grant.
						ErrorUtilities.VerifyProtocol(scopedAccessRequest.Scope.IsSubsetOf(tokenRequest.AuthorizationDescription.Scope), OAuthStrings.AccessScopeExceedsGrantScope, scopedAccessRequest.Scope, tokenRequest.AuthorizationDescription.Scope);
					}
				}

				// Make sure the authorization this token represents hasn't already been revoked.
				ErrorUtilities.VerifyProtocol(this.AuthorizationServer.IsAuthorizationValid(tokenRequest.AuthorizationDescription), Protocol.authorization_expired);

				return MessageProtections.None;
			}

			return null;
		}
	}
}
