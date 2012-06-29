//-----------------------------------------------------------------------
// <copyright file="TokenCodeSerializationBindingElement.cs" company="Outercurve Foundation">
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
	/// Serializes and deserializes authorization codes, refresh tokens and access tokens
	/// on incoming and outgoing messages.
	/// </summary>
	internal class TokenCodeSerializationBindingElement : AuthServerBindingElementBase {
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
			var directResponse = message as IDirectResponseProtocolMessage;
			var request = directResponse != null ? directResponse.OriginatingRequest as IAccessTokenRequestInternal : null;

			// Serialize the authorization code, if there is one.
			var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
			if (authCodeCarrier != null) {
				var codeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
				var code = authCodeCarrier.AuthorizationDescription;
				authCodeCarrier.Code = codeFormatter.Serialize(code);
				return MessageProtections.None;
			}

			// Serialize the refresh token, if applicable.
			var refreshTokenResponse = message as AccessTokenSuccessResponse;
			if (refreshTokenResponse != null && refreshTokenResponse.HasRefreshToken) {
				var refreshTokenCarrier = (IAuthorizationCarryingRequest)message;
				var refreshToken = new RefreshToken(refreshTokenCarrier.AuthorizationDescription);
				var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
				refreshTokenResponse.RefreshToken = refreshTokenFormatter.Serialize(refreshToken);
			}

			// Serialize the access token, if applicable.
			var accessTokenResponse = message as IAccessTokenIssuingResponse;
			if (accessTokenResponse != null && accessTokenResponse.AuthorizationDescription != null) {
				ErrorUtilities.VerifyInternal(request != null, "We should always have a direct request message for this case.");
				accessTokenResponse.AccessToken = accessTokenResponse.AuthorizationDescription.Serialize();
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
			var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
			if (authCodeCarrier != null) {
				var authorizationCodeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
				var authorizationCode = new AuthorizationCode();
				authorizationCodeFormatter.Deserialize(authorizationCode, message, authCodeCarrier.Code, Protocol.code);
				authCodeCarrier.AuthorizationDescription = authorizationCode;
			}

			var refreshTokenCarrier = message as IRefreshTokenCarryingRequest;
			if (refreshTokenCarrier != null) {
				var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
				var refreshToken = new RefreshToken();
				refreshTokenFormatter.Deserialize(refreshToken, message, refreshTokenCarrier.RefreshToken, Protocol.refresh_token);
				refreshTokenCarrier.AuthorizationDescription = refreshToken;
			}

			return null;
		}
	}
}
