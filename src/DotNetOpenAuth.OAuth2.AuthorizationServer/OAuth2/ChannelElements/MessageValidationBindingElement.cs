//-----------------------------------------------------------------------
// <copyright file="MessageValidationBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2.Messages;
	using Messaging;

	/// <summary>
	/// A guard for all messages to or from an Authorization Server to ensure that they are well formed,
	/// have valid secrets, callback URIs, etc.
	/// </summary>
	internal class MessageValidationBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
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
			bool applied = false;

			// Check that the client secret is correct for client authenticated messages.
			var authenticatedClientRequest = message as AuthenticatedClientRequestBase;
			if (authenticatedClientRequest != null) {
				var client = this.AuthorizationServer.GetClientOrThrow(authenticatedClientRequest.ClientIdentifier);
				string secret = client.Secret;
				AuthServerUtilities.TokenEndpointVerify(!string.IsNullOrEmpty(secret), Protocol.AccessTokenRequestErrorCodes.UnauthorizedClient); // an empty secret is not allowed for client authenticated calls.
				AuthServerUtilities.TokenEndpointVerify(MessagingUtilities.EqualsConstantTime(secret, authenticatedClientRequest.ClientSecret), Protocol.AccessTokenRequestErrorCodes.InvalidClient, AuthServerStrings.ClientSecretMismatch);
				applied = true;
			}

			// Check that authorization requests come with an acceptable callback URI.
			var authorizationRequest = message as EndUserAuthorizationRequest;
			if (authorizationRequest != null) {
				var client = this.AuthorizationServer.GetClientOrThrow(authorizationRequest.ClientIdentifier);
				ErrorUtilities.VerifyProtocol(authorizationRequest.Callback == null || client.IsCallbackAllowed(authorizationRequest.Callback), OAuthStrings.ClientCallbackDisallowed, authorizationRequest.Callback);
				ErrorUtilities.VerifyProtocol(authorizationRequest.Callback != null || client.DefaultCallback != null, OAuthStrings.NoCallback);
				applied = true;
			}

			// Check that the callback URI in a direct message from the client matches the one in the indirect message received earlier.
			var request = message as AccessTokenAuthorizationCodeRequestAS;
			if (request != null) {
				IAuthorizationCodeCarryingRequest tokenRequest = request;
				tokenRequest.AuthorizationDescription.VerifyCallback(request.Callback);
				applied = true;
			}

			return applied ? (MessageProtections?)MessageProtections.None : null;
		}
	}
}
