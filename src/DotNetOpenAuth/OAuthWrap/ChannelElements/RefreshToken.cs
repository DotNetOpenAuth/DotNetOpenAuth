//-----------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The refresh token issued to a client by an authorization server that allows the client
	/// to periodically obtain new short-lived access tokens.
	/// </summary>
	[Serializable]
	internal class RefreshToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="secret">The symmetric secret used to sign/encrypt refresh tokens.</param>
		/// <param name="authorization">The authorization this refresh token should describe.</param>
		internal RefreshToken(byte[] secret, IAuthorizationDescription authorization)
			: this(secret) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="secret">The symmetric secret used to sign/encrypt refresh tokens.</param>
		private RefreshToken(byte[] secret)
			: base(secret, true, true) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			}

		/// <summary>
		/// Deserializes a refresh token.
		/// </summary>
		/// <param name="secret">The symmetric secret used to sign and encrypt the token.</param>
		/// <param name="value">The token.</param>
		/// <param name="containingMessage">The message containing this token.</param>
		/// <returns>The refresh token.</returns>
		internal static RefreshToken Decode(byte[] secret, string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<RefreshToken>() != null);

			var self = new RefreshToken(secret);
			self.Decode(value, containingMessage);
			return self;
		}
	}
}
