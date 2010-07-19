//-----------------------------------------------------------------------
// <copyright file="AuthorizationCode.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Represents the authorization code created when a user approves authorization that
	/// allows the client to request an access/refresh token.
	/// </summary>
	internal class AuthorizationCode : AuthorizationDataBag {
		/// <summary>
		/// The hash algorithm used on the callback URI.
		/// </summary>
		private readonly HashAlgorithm hasher = new SHA256Managed();

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationCode"/> class.
		/// </summary>
		public AuthorizationCode() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationCode"/> class.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="callback">The callback the client used to obtain authorization.</param>
		/// <param name="scopes">The authorized scopes.</param>
		/// <param name="username">The name on the account that authorized access.</param>
		internal AuthorizationCode(string clientIdentifier, Uri callback, IEnumerable<string> scopes, string username) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Requires<ArgumentNullException>(callback != null, "callback");

			this.ClientIdentifier = clientIdentifier;
			this.CallbackHash = this.CalculateCallbackHash(callback);
			this.Scope.ResetContents(scopes);
			this.User = username;
		}

		/// <summary>
		/// Gets or sets the hash of the callback URL.
		/// </summary>
		[MessagePart("cb")]
		private byte[] CallbackHash { get; set; }

		internal static IDataBagFormatter<AuthorizationCode> CreateFormatter(IAuthorizationServer authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
			Contract.Ensures(Contract.Result<IDataBagFormatter<AuthorizationCode>>() != null);

			return new UriStyleMessageFormatter<AuthorizationCode>(
				authorizationServer.Secret,
				true,
				true,
				false,
				AuthorizationCodeBindingElement.MaximumMessageAge,
				authorizationServer.VerificationCodeNonceStore);
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
