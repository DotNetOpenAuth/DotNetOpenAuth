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

	/// <summary>
	/// A short-lived token that accompanies HTTP requests to protected data to authorize the request.
	/// </summary>
	internal class AccessToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="signingKey">The signing key.</param>
		/// <param name="encryptingKey">The encrypting key.</param>
		/// <param name="authorization">The authorization to be described by the access token.</param>
		/// <param name="lifetime">The lifetime of the access token.</param>
		internal AccessToken(RSAParameters signingKey, RSAParameters encryptingKey, IAuthorizationDescription authorization, TimeSpan? lifetime)
			: this(signingKey, encryptingKey) {
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
			this.Lifetime = lifetime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="signingKey">The signing key.</param>
		/// <param name="encryptingKey">The encrypting key.</param>
		private AccessToken(RSAParameters signingKey, RSAParameters encryptingKey)
			: base(signingKey, encryptingKey) {
			}

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Deserializes an access token.
		/// </summary>
		/// <param name="signingKey">The signing public key.</param>
		/// <param name="encryptingKey">The encrypting private key.</param>
		/// <param name="value">The access token.</param>
		/// <param name="containingMessage">The message containing this token.</param>
		/// <returns>The access token.</returns>
		internal static AccessToken Decode(RSAParameters signingKey, RSAParameters encryptingKey, string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<AccessToken>() != null);

			var self = new AccessToken(signingKey, encryptingKey);
			self.Decode(value, containingMessage);
			return self;
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			// Has this token expired?
			if (this.Lifetime.HasValue) {
				DateTime expirationDate = this.UtcCreationDate + this.Lifetime.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, this.ContainingMessage);
				}
			}
		}
	}
}
