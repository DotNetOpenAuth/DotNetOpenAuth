//-----------------------------------------------------------------------
// <copyright file="VerificationCode.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	internal class VerificationCode : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="scope">The scope.</param>
		/// <param name="username">The username.</param>
		internal VerificationCode(OAuthWrapAuthorizationServerChannel channel, string clientIdentifier, Uri callback, string scope, string username)
			: this(channel) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentNullException>(callback != null, "callback");

			this.ClientIdentifier = clientIdentifier;
			this.CallbackHash = this.CalculateCallbackHash(callback);
			this.Scope = scope;
			this.User = username;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private VerificationCode(OAuthWrapAuthorizationServerChannel channel)
			: base(channel, true, true, false, MaximumMessageAge, channel.AuthorizationServer.VerificationCodeNonceStore) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(channel.AuthorizationServer != null);
		}

		[MessagePart("cb")]
		private string CallbackHash { get; set; }

		/// <summary>
		/// Gets the maximum message age from the standard expiration binding element.
		/// </summary>
		private static TimeSpan MaximumMessageAge {
			get { return StandardExpirationBindingElement.MaximumMessageAge; }
		}

		internal static VerificationCode Decode(OAuthWrapAuthorizationServerChannel channel, string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<VerificationCode>() != null);

			var self = new VerificationCode(channel);
			self.Decode(value, containingMessage);
			return self;
		}

		internal void VerifyCallback(Uri callback) {
			ErrorUtilities.VerifyProtocol(string.Equals(this.CallbackHash, this.CalculateCallbackHash(callback), StringComparison.Ordinal), Protocol.redirect_uri_mismatch);
		}

		private string CalculateCallbackHash(Uri callback) {
			return this.Hasher.ComputeHash(callback.AbsoluteUri);
		}
	}
}
