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

	internal class RefreshToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private RefreshToken(byte[] secret)
			: base(secret, true, true) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
		}

		internal RefreshToken(byte[] secret, IAuthorizationDescription authorization)
			: this(secret) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
		}

		internal static RefreshToken Decode(byte[] secret, string value, IProtocolMessage containingMessage = null) {
			Contract.Requires<ArgumentNullException>(secret != null, "secret");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Ensures(Contract.Result<RefreshToken>() != null);

			var self = new RefreshToken(secret);
			self.Decode(value, containingMessage);
			return self;
		}
	}
}
