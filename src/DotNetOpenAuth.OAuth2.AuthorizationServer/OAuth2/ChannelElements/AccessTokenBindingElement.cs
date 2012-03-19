//-----------------------------------------------------------------------
// <copyright file="AccessTokenBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Serializes access tokens inside an outgoing message.
	/// </summary>
	internal class AccessTokenBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenBindingElement"/> class.
		/// </summary>
		internal AccessTokenBindingElement() {
		}

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <value>Always <c>MessageProtections.None</c></value>
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
		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			var directResponse = message as IDirectResponseProtocolMessage;
			var request = directResponse != null ? directResponse.OriginatingRequest as IAccessTokenRequestInternal : null;
			var authCarryingRequest = request as IAuthorizationCarryingRequest;
			var accessTokenResponse = message as IAccessTokenIssuingResponse;
			var implicitGrantResponse = message as EndUserAuthorizationSuccessAccessTokenResponse;

			if (request != null) {
				request.AccessTokenCreationParameters = this.AuthorizationServer.GetAccessTokenParameters(request);
				ErrorUtilities.VerifyHost(request.AccessTokenCreationParameters != null, "IAuthorizationServer.GetAccessTokenParameters must not return null.");

				if (accessTokenResponse != null) {
					accessTokenResponse.Lifetime = request.AccessTokenCreationParameters.AccessTokenLifetime;
				}
			}

			AccessToken accessToken = null;
			if (authCarryingRequest != null) {
				ErrorUtilities.VerifyInternal(request != null, MessagingStrings.UnexpectedMessageReceived, typeof(IAccessTokenRequestInternal), request.GetType());
				accessToken = new AccessToken(authCarryingRequest.AuthorizationDescription, accessTokenResponse.Lifetime);
			} else if (implicitGrantResponse != null) {
				IAccessTokenCarryingRequest tokenCarryingResponse = implicitGrantResponse;
				accessToken = new AccessToken(
					request.ClientIdentifier,
					implicitGrantResponse.Scope,
					implicitGrantResponse.AuthorizingUsername,
					implicitGrantResponse.Lifetime);
			}

			if (accessToken != null) {
				accessTokenResponse.AuthorizationDescription = accessToken;
				var accessTokenFormatter = AccessToken.CreateFormatter(this.AuthorizationServer.AccessTokenSigningKey, request.AccessTokenCreationParameters.ResourceServerEncryptionKey);
				accessTokenResponse.AccessToken = accessTokenFormatter.Serialize(accessToken);
			}

			var refreshTokenResponse = message as AccessTokenSuccessResponse;
			if (refreshTokenResponse != null && refreshTokenResponse.HasRefreshToken) {
				var refreshToken = new RefreshToken(authCarryingRequest.AuthorizationDescription);
				var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
				refreshTokenResponse.RefreshToken = refreshTokenFormatter.Serialize(refreshToken);
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
		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			return null;
		}
	}
}
