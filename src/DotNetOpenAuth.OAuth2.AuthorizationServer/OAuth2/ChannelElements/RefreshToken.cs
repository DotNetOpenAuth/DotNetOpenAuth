//-----------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// The refresh token issued to a client by an authorization server that allows the client
	/// to periodically obtain new short-lived access tokens.
	/// </summary>
	internal class RefreshToken : AuthorizationDataBag {
		/// <summary>
		/// The name of the bucket for symmetric keys used to sign refresh tokens.
		/// </summary>
		internal const string RefreshTokenKeyBucket = "https://localhost/dnoa/oauth_refresh_token";

		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		public RefreshToken() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="authorization">The authorization this refresh token should describe.</param>
		internal RefreshToken(IAuthorizationDescription authorization) {
			Requires.NotNull(authorization, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope.ResetContents(authorization.Scope);
		}

		/// <summary>
		/// Creates a formatter capable of serializing/deserializing a refresh token.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store.</param>
		/// <returns>
		/// A DataBag formatter.  Never null.
		/// </returns>
		internal static IDataBagFormatter<RefreshToken> CreateFormatter(ICryptoKeyStore cryptoKeyStore) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");

			return new UriStyleMessageFormatter<RefreshToken>(cryptoKeyStore, RefreshTokenKeyBucket, signed: true, encrypted: true);
		}
	}
}
