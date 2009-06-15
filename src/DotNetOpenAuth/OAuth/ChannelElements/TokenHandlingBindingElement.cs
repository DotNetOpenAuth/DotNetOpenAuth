//-----------------------------------------------------------------------
// <copyright file="TokenHandlingBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// A binding element for Service Providers to manage the 
	/// callbacks and verification codes on applicable messages.
	/// </summary>
	internal class TokenHandlingBindingElement : IChannelBindingElement {
		/// <summary>
		/// The token manager offered by the service provider.
		/// </summary>
		private IServiceProviderTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenHandlingBindingElement"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		internal TokenHandlingBindingElement(IServiceProviderTokenManager tokenManager) {
			Contract.Requires(tokenManager != null);
			ErrorUtilities.VerifyArgumentNotNull(tokenManager, "tokenManager");

			this.tokenManager = tokenManager;
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public MessageProtections Protection {
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
		public MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var userAuthResponse = message as UserAuthorizationResponse;
			if (userAuthResponse != null && userAuthResponse.Version >= Protocol.V10a.Version) {
				this.tokenManager.GetRequestToken(userAuthResponse.RequestToken).VerificationCode = userAuthResponse.VerificationCode;
				return MessageProtections.None;
			}

			// Hook to store the token and secret on its way down to the Consumer.
			var grantRequestTokenResponse = message as UnauthorizedTokenResponse;
			if (grantRequestTokenResponse != null) {
				this.tokenManager.StoreNewRequestToken(grantRequestTokenResponse.RequestMessage, grantRequestTokenResponse);
				this.tokenManager.GetRequestToken(grantRequestTokenResponse.RequestToken).ConsumerVersion = grantRequestTokenResponse.Version;
				if (grantRequestTokenResponse.RequestMessage.Callback != null) {
					this.tokenManager.GetRequestToken(grantRequestTokenResponse.RequestToken).Callback = grantRequestTokenResponse.RequestMessage.Callback;
				}

				return MessageProtections.None;
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
		public MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var authorizedTokenRequest = message as AuthorizedTokenRequest;
			if (authorizedTokenRequest != null && authorizedTokenRequest.Version >= Protocol.V10a.Version) {
				string expectedVerifier = this.tokenManager.GetRequestToken(authorizedTokenRequest.RequestToken).VerificationCode;
				ErrorUtilities.VerifyProtocol(string.Equals(authorizedTokenRequest.VerificationCode, expectedVerifier, StringComparison.Ordinal), OAuthStrings.IncorrectVerifier);
				return MessageProtections.None;
			}

			return null;
		}

		#endregion
	}
}
