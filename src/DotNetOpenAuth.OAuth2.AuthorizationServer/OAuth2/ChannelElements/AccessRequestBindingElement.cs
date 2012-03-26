//-----------------------------------------------------------------------
// <copyright file="AccessRequestBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.AuthServer.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Decodes authorization codes and refresh tokens on incoming messages.
	/// </summary>
	/// <remarks>
	/// This binding element also ensures that the code/token coming in is issued to
	/// the same client that is sending the code/token and that the authorization has
	/// not been revoked and that an access token has not expired.
	/// </remarks>
	internal class AccessRequestBindingElement : AuthServerBindingElementBase {
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
			var responseWithOriginatingRequest = message as IDirectResponseProtocolMessage;
			var accessRequest = responseWithOriginatingRequest.OriginatingRequest as IAccessTokenRequestInternal;

			var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
			if (authCodeCarrier != null) {
				var codeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
				var code = authCodeCarrier.AuthorizationDescription;
				authCodeCarrier.Code = codeFormatter.Serialize(code);
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
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "unauthorizedclient", Justification = "Protocol requirement")]
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "incorrectclientcredentials", Justification = "Protocol requirement")]
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "authorizationexpired", Justification = "Protocol requirement")]
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DotNetOpenAuth.Messaging.ErrorUtilities.VerifyProtocol(System.Boolean,System.String,System.Object[])", Justification = "Protocol requirement")]
		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var tokenRequest = message as IAuthorizationCarryingRequest;
			if (tokenRequest != null) {
				try {
					var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
					var refreshTokenCarrier = message as IRefreshTokenCarryingRequest;
					var resourceOwnerPasswordCarrier = message as AccessTokenResourceOwnerPasswordCredentialsRequest;
					var clientCredentialOnly = message as AccessTokenClientCredentialsRequest;
					if (authCodeCarrier != null) {
						var authorizationCodeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
						var authorizationCode = authorizationCodeFormatter.Deserialize(message, authCodeCarrier.Code, Protocol.code);
						authCodeCarrier.AuthorizationDescription = authorizationCode;
					} else if (refreshTokenCarrier != null) {
						var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
						var refreshToken = refreshTokenFormatter.Deserialize(message, refreshTokenCarrier.RefreshToken, Protocol.refresh_token);
						refreshTokenCarrier.AuthorizationDescription = refreshToken;
					} else if (resourceOwnerPasswordCarrier != null) {
						try {
							if (this.AuthorizationServer.IsResourceOwnerCredentialValid(resourceOwnerPasswordCarrier.UserName, resourceOwnerPasswordCarrier.Password)) {
								resourceOwnerPasswordCarrier.CredentialsValidated = true;
							} else {
								Logger.OAuth.WarnFormat(
									"Resource owner password credential for user \"{0}\" rejected by authorization server host.",
									resourceOwnerPasswordCarrier.UserName);

								// TODO: fix this to report the appropriate error code for a bad credential.
								throw new ProtocolException();
							}
						} catch (NotSupportedException) {
							// TODO: fix this to return the appropriate error code for not supporting resource owner password credentials
							throw new ProtocolException();
						} catch (NotImplementedException) {
							// TODO: fix this to return the appropriate error code for not supporting resource owner password credentials
							throw new ProtocolException();
						}
					} else if (clientCredentialOnly != null) {
						// this method will throw later if the credentials are false.
						clientCredentialOnly.CredentialsValidated = true;
					} else {
						throw ErrorUtilities.ThrowInternal("Unexpected message type: " + tokenRequest.GetType());
					}
				} catch (ExpiredMessageException ex) {
					throw ErrorUtilities.Wrap(ex, Protocol.authorization_expired);
				}

				var accessRequest = tokenRequest as AccessTokenRequestBase;
				if (accessRequest != null) {
					// Make sure the client sending us this token is the client we issued the token to.
					ErrorUtilities.VerifyProtocol(string.Equals(accessRequest.ClientIdentifier, tokenRequest.AuthorizationDescription.ClientIdentifier, StringComparison.Ordinal), Protocol.incorrect_client_credentials);

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
