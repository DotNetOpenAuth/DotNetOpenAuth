//-----------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	internal class AccessToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private AccessToken(RSAParameters signingKey, RSAParameters encryptingKey)
			: base(signingKey, encryptingKey) {
		}

		internal AccessToken(RSAParameters signingKey, RSAParameters encryptingKey, IAuthorizationDescription authorization, TimeSpan? lifetime)
			: this(signingKey, encryptingKey) {
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
			this.Lifetime = lifetime;
		}

		internal TimeSpan? Lifetime { get; set; }

		internal static AccessToken Decode(RSAParameters signingKey, RSAParameters encryptingKey, string value, IProtocolMessage containingMessage = null) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Ensures(Contract.Result<AccessToken>() != null);

			var self = new AccessToken(signingKey, encryptingKey);
			self.Decode(value, containingMessage);
			return self;
		}

		protected override void Decode(string value, IProtocolMessage containingMessage) {
			base.Decode(value, containingMessage);

			// Has this token expired?
			if (this.Lifetime.HasValue) {
				DateTime expirationDate = this.UtcCreationDate + this.Lifetime.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, containingMessage);
				}
			}
		}
	}
}
