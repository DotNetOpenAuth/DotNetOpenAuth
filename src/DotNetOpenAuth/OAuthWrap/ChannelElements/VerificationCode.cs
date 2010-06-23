//-----------------------------------------------------------------------
// <copyright file="VerificationCode.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Represents the verification code created when a user approves authorization that
	/// allows the client to request an access/refresh token.
	/// </summary>
	internal class VerificationCode : AuthorizationDataBag {
		/// <summary>
		/// The hash algorithm used on the callback URI.
		/// </summary>
		private readonly HashAlgorithm hasher = new SHA256Managed();

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="secret">The symmetric secret to use in signing/encryption.</param>
		/// <param name="nonceStore">The nonce store to use to ensure verification codes are used only once.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="callback">The callback the client used to obtain authorization.</param>
		/// <param name="scope">The scope.</param>
		/// <param name="username">The name on the account that authorized access.</param>
		internal VerificationCode(byte[] secret, INonceStore nonceStore, string clientIdentifier, Uri callback, string scope, string username)
			: this(secret, nonceStore) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(nonceStore != null, "nonceStore");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(callback != null, "callback");

			this.ClientIdentifier = clientIdentifier;
			this.CallbackHash = this.CalculateCallbackHash(callback);
			this.Scope = scope;
			this.User = username;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="secret">The symmetric secret to use in signing/encryption.</param>
		/// <param name="nonceStore">The nonce store to use to ensure verification codes are used only once.</param>
		private VerificationCode(byte[] secret, INonceStore nonceStore)
			: base(secret, true, true, false, MaximumMessageAge, nonceStore) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(nonceStore != null, "nonceStore");
		}

		/// <summary>
		/// Gets the maximum message age from the standard expiration binding element.
		/// </summary>
		private static TimeSpan MaximumMessageAge {
			get { return StandardExpirationBindingElement.MaximumMessageAge; }
		}

		/// <summary>
		/// Gets or sets the hash of the callback URL.
		/// </summary>
		[MessagePart("cb")]
		private byte[] CallbackHash { get; set; }

		/// <summary>
		/// Deserializes a verification code.
		/// </summary>
		/// <param name="secret">The symmetric secret used to sign and encrypt the token.</param>
		/// <param name="nonceStore">The nonce store to use to ensure verification codes can only be exchanged once.</param>
		/// <param name="value">The verification token.</param>
		/// <param name="containingMessage">The message containing this verification code.</param>
		/// <returns>The verification code.</returns>
		internal static VerificationCode Decode(byte[] secret, INonceStore nonceStore, string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(nonceStore != null, "nonceStore");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<VerificationCode>() != null);

			var self = new VerificationCode(secret, nonceStore);
			self.Decode(value, containingMessage);
			return self;
		}

		/// <summary>
		/// Verifies the the given callback URL matches the callback originally given in the authorization request.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <remarks>
		/// This method serves to verify that the callback URL given in the original authorization request
		/// and the callback URL given in the access token request match.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown when the callback URLs do not match.</exception>
		internal void VerifyCallback(Uri callback) {
			ErrorUtilities.VerifyProtocol(MessagingUtilities.AreEquivalent(this.CallbackHash, this.CalculateCallbackHash(callback)), Protocol.redirect_uri_mismatch);
		}

		/// <summary>
		/// Calculates the hash of the callback URL.
		/// </summary>
		/// <param name="callback">The callback whose hash should be calculated.</param>
		/// <returns>
		/// A base64 encoding of the hash of the URL.
		/// </returns>
		private byte[] CalculateCallbackHash(Uri callback) {
			return this.hasher.ComputeHash(Encoding.UTF8.GetBytes(callback.AbsoluteUri));
		}
	}
}
