//-----------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The refresh token issued to a client by an authorization server that allows the client
	/// to periodically obtain new short-lived access tokens.
	/// </summary>
	internal class RefreshToken : AuthorizationDataBag {
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
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope.ResetContents(authorization.Scope);
		}

		/// <summary>
		/// Creates a formatter capable of serializing/deserializing a refresh token.
		/// </summary>
		/// <param name="symmetricSecret">The symmetric secret used by the authorization server to sign/encrypt refresh tokens.  Must not be null.</param>
		/// <returns>A DataBag formatter.  Never null.</returns>
		internal static IDataBagFormatter<RefreshToken> CreateFormatter(byte[] symmetricSecret)
		{
			Contract.Requires<ArgumentNullException>(symmetricSecret != null, "symmetricSecret");
			Contract.Ensures(Contract.Result<IDataBagFormatter<RefreshToken>>() != null);

			return new UriStyleMessageFormatter<RefreshToken>(symmetricSecret, true, true);
		}
	}
}
