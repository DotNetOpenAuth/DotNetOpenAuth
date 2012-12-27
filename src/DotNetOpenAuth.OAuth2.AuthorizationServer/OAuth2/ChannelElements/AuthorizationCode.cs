//-----------------------------------------------------------------------
// <copyright file="AuthorizationCode.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Represents the authorization code created when a user approves authorization that
	/// allows the client to request an access/refresh token.
	/// </summary>
	internal class AuthorizationCode : AuthorizationDataBag {
		/// <summary>
		/// The name of the bucket for symmetric keys used to sign authorization codes.
		/// </summary>
		internal const string AuthorizationCodeKeyBucket = "https://localhost/dnoa/oauth_authorization_code";

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationCode"/> class.
		/// </summary>
		public AuthorizationCode() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationCode"/> class.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="callback">The callback the client used to obtain authorization, if one was explicitly included in the request.</param>
		/// <param name="scopes">The authorized scopes.</param>
		/// <param name="username">The name on the account that authorized access.</param>
		internal AuthorizationCode(string clientIdentifier, Uri callback, IEnumerable<string> scopes, string username) {
			Requires.NotNullOrEmpty(clientIdentifier, "clientIdentifier");

			this.ClientIdentifier = clientIdentifier;
			this.CallbackHash = CalculateCallbackHash(callback);
			this.Scope.ResetContents(scopes);
			this.User = username;
			this.UtcCreationDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets the maximum message age from the standard expiration binding element.
		/// </summary>
		/// <value>This interval need not account for clock skew because it is only compared within a single authorization server or farm of servers.</value>
		internal static TimeSpan MaximumMessageAge {
			get { return Configuration.DotNetOpenAuthSection.Messaging.MaximumMessageLifetimeNoSkew; }
		}

		/// <summary>
		/// Gets or sets the hash of the callback URL.
		/// </summary>
		[MessagePart("cb")]
		private byte[] CallbackHash { get; set; }

		/// <summary>
		/// Creates a serializer/deserializer for this type.
		/// </summary>
		/// <param name="authorizationServer">The authorization server that will be serializing/deserializing this authorization code.  Must not be null.</param>
		/// <returns>A DataBag formatter.</returns>
		internal static IDataBagFormatter<AuthorizationCode> CreateFormatter(IAuthorizationServerHost authorizationServer) {
			Requires.NotNull(authorizationServer, "authorizationServer");

			var cryptoStore = authorizationServer.CryptoKeyStore;
			ErrorUtilities.VerifyHost(cryptoStore != null, OAuthStrings.ResultShouldNotBeNull, authorizationServer.GetType(), "CryptoKeyStore");

			return new UriStyleMessageFormatter<AuthorizationCode>(
				cryptoStore,
				AuthorizationCodeKeyBucket,
				signed: true,
				encrypted: true,
				compressed: false,
				maximumAge: MaximumMessageAge,
				decodeOnceOnly: authorizationServer.NonceStore);
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
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "redirecturimismatch", Justification = "Protocol requirement")]
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DotNetOpenAuth.Messaging.ErrorUtilities.VerifyProtocol(System.Boolean,System.String,System.Object[])", Justification = "Protocol requirement")]
		internal void VerifyCallback(Uri callback) {
			ErrorUtilities.VerifyProtocol(MessagingUtilities.AreEquivalentConstantTime(this.CallbackHash, CalculateCallbackHash(callback)), Protocol.redirect_uri_mismatch);
		}

		/// <summary>
		/// Calculates the hash of the callback URL.
		/// </summary>
		/// <param name="callback">The callback whose hash should be calculated.</param>
		/// <returns>
		/// A base64 encoding of the hash of the URL.
		/// </returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
		private static byte[] CalculateCallbackHash(Uri callback) {
			if (callback == null) {
				return null;
			}

			using (var hasher = SHA256.Create()) {
				return hasher.ComputeHash(Encoding.UTF8.GetBytes(callback.AbsoluteUri));
			}
		}
	}
}
