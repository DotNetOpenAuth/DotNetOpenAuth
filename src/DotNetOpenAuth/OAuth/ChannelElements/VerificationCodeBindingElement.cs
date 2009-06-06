//-----------------------------------------------------------------------
// <copyright file="VerificationCodeBindingElement.cs" company="Andrew Arnott">
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
	/// A binding element for Service Providers to manage the verification code on applicable messages.
	/// </summary>
	internal class VerificationCodeBindingElement : IChannelBindingElement {
		/// <summary>
		/// The length of the verifier code (in raw bytes before base64 encoding) to generate.
		/// </summary>
		private const int VerifierCodeLength = 5;

		/// <summary>
		/// The token manager offered by the service provider.
		/// </summary>
		private IServiceProviderTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCodeBindingElement"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		internal VerificationCodeBindingElement(IServiceProviderTokenManager tokenManager) {
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

			var response = message as UserAuthorizationResponse;
			if (response != null && response.Version >= Protocol.V10a.Version) {
				ErrorUtilities.VerifyInternal(response.VerificationCode == null, "VerificationCode was unexpectedly already set.");
				response.VerificationCode = MessagingUtilities.GetCryptoRandomDataAsBase64(VerifierCodeLength);
				this.tokenManager.SetRequestTokenVerifier(response.RequestToken, response.VerificationCode);
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

			var request = message as AuthorizedTokenRequest;
			if (request != null && request.Version >= Protocol.V10a.Version) {
				string expectedVerifier = this.tokenManager.GetRequestTokenVerifier(request.RequestToken);
				ErrorUtilities.VerifyProtocol(string.Equals(request.VerificationCode, expectedVerifier, StringComparison.Ordinal), OAuthStrings.IncorrectVerifier);
				return MessageProtections.None;
			}

			return null;
		}

		#endregion
	}
}
