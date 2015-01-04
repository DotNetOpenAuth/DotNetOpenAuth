//-----------------------------------------------------------------------
// <copyright file="TokenHandlingBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

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
		/// The security settings for this service provider.
		/// </summary>
		private ServiceProviderSecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenHandlingBindingElement"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="securitySettings">The security settings.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Requires<System.ArgumentNullException>(System.Boolean,System.String,System.String)", Justification = "Code contract"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "securitySettings", Justification = "Code contracts")]
		internal TokenHandlingBindingElement(IServiceProviderTokenManager tokenManager, ServiceProviderSecuritySettings securitySettings) {
			Requires.NotNull(tokenManager, "tokenManager");
			Requires.NotNull(securitySettings, "securitySettings");

			this.tokenManager = tokenManager;
			this.securitySettings = securitySettings;
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var userAuthResponse = message as UserAuthorizationResponse;
			if (userAuthResponse != null && userAuthResponse.Version >= Protocol.V10a.Version) {
				var requestToken = this.tokenManager.GetRequestToken(userAuthResponse.RequestToken);
				requestToken.VerificationCode = userAuthResponse.VerificationCode;
				this.tokenManager.UpdateToken(requestToken);
				return MessageProtectionTasks.None;
			}

			// Hook to store the token and secret on its way down to the Consumer.
			var grantRequestTokenResponse = message as UnauthorizedTokenResponse;
			if (grantRequestTokenResponse != null) {
				this.tokenManager.StoreNewRequestToken(grantRequestTokenResponse.RequestMessage, grantRequestTokenResponse);

				// The host may have already set these properties, but just to make sure...
				var requestToken = this.tokenManager.GetRequestToken(grantRequestTokenResponse.RequestToken);
				requestToken.ConsumerVersion = grantRequestTokenResponse.Version;
				if (grantRequestTokenResponse.RequestMessage.Callback != null) {
					requestToken.Callback = grantRequestTokenResponse.RequestMessage.Callback;
				}
				this.tokenManager.UpdateToken(requestToken);

				return MessageProtectionTasks.None;
			}

			return MessageProtectionTasks.Null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var authorizedTokenRequest = message as AuthorizedTokenRequest;
			if (authorizedTokenRequest != null) {
				if (authorizedTokenRequest.Version >= Protocol.V10a.Version) {
					string expectedVerifier = this.tokenManager.GetRequestToken(authorizedTokenRequest.RequestToken).VerificationCode;
					ErrorUtilities.VerifyProtocol(string.Equals(authorizedTokenRequest.VerificationCode, expectedVerifier, StringComparison.Ordinal), OAuthStrings.IncorrectVerifier);
					return MessageProtectionTasks.None;
				}

				this.VerifyThrowTokenTimeToLive(authorizedTokenRequest);
			}

			var userAuthorizationRequest = message as UserAuthorizationRequest;
			if (userAuthorizationRequest != null) {
				this.VerifyThrowTokenTimeToLive(userAuthorizationRequest);
			}

			var accessResourceRequest = message as AccessProtectedResourceRequest;
			if (accessResourceRequest != null) {
				this.VerifyThrowTokenNotExpired(accessResourceRequest);
			}

			return MessageProtectionTasks.Null;
		}

		#endregion

		/// <summary>
		/// Ensures that access tokens have not yet expired.
		/// </summary>
		/// <param name="message">The incoming message carrying the access token.</param>
		private void VerifyThrowTokenNotExpired(AccessProtectedResourceRequest message) {
			Requires.NotNull(message, "message");

			try {
				IServiceProviderAccessToken token = this.tokenManager.GetAccessToken(message.AccessToken);
				if (token.ExpirationDate.HasValue && DateTime.Now >= token.ExpirationDate.Value.ToLocalTimeSafe()) {
					Logger.OAuth.ErrorFormat(
						"OAuth access token {0} rejected because it expired at {1}, and it is now {2}.",
						token.Token,
						token.ExpirationDate.Value,
						DateTime.Now);
					ErrorUtilities.ThrowProtocol(OAuthStrings.TokenNotFound);
				}
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuthStrings.TokenNotFound);
			}
		}

		/// <summary>
		/// Ensures that short-lived request tokens included in incoming messages have not expired.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		/// <exception cref="ProtocolException">Thrown when the token in the message has expired.</exception>
		private void VerifyThrowTokenTimeToLive(ITokenContainingMessage message) {
			ErrorUtilities.VerifyInternal(!(message is AccessProtectedResourceRequest), "We shouldn't be verifying TTL on access tokens.");
			if (message == null || string.IsNullOrEmpty(message.Token)) {
				return;
			}

			try {
				IServiceProviderRequestToken token = this.tokenManager.GetRequestToken(message.Token);
				TimeSpan ttl = this.securitySettings.MaximumRequestTokenTimeToLive;
				if (DateTime.Now >= token.CreatedOn.ToLocalTimeSafe() + ttl) {
					Logger.OAuth.ErrorFormat(
						"OAuth request token {0} rejected because it was originally issued at {1}, expired at {2}, and it is now {3}.",
						token.Token,
						token.CreatedOn,
						token.CreatedOn + ttl,
						DateTime.Now);
					ErrorUtilities.ThrowProtocol(OAuthStrings.TokenNotFound);
				}
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuthStrings.TokenNotFound);
			}
		}
	}
}
