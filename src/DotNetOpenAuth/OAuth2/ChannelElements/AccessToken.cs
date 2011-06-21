//-----------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using System.Collections.Generic;

	/// <summary>
	/// A short-lived token that accompanies HTTP requests to protected data to authorize the request.
	/// </summary>
	internal class AccessToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		public AccessToken() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="authorization">The authorization to be described by the access token.</param>
		/// <param name="lifetime">The lifetime of the access token.</param>
		internal AccessToken(IAuthorizationDescription authorization, TimeSpan? lifetime) {
			Contract.Requires<ArgumentNullException>(authorization != null);

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope.ResetContents(authorization.Scope);
			this.Lifetime = lifetime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="scopes">The scopes.</param>
		/// <param name="username">The username of the account that authorized this token.</param>
		/// <param name="lifetime">The lifetime for this access token.</param>
		internal AccessToken(string clientIdentifier, IEnumerable<string> scopes, string username, TimeSpan? lifetime) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));

			this.ClientIdentifier = clientIdentifier;
			this.Scope.ResetContents(scopes);
			this.User = username;
			this.Lifetime = lifetime;
			this.UtcCreationDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Creates a formatter capable of serializing/deserializing an access token.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the authorization server's private key used to asymmetrically sign the access token.</param>
		/// <param name="encryptingKey">The crypto service provider with the resource server's public key used to encrypt the access token.</param>
		/// <returns>An access token serializer.</returns>
		internal static IDataBagFormatter<AccessToken> CreateFormatter(RSACryptoServiceProvider signingKey, RSACryptoServiceProvider encryptingKey) {
			Contract.Requires(signingKey != null || !signingKey.PublicOnly);
			Contract.Requires(encryptingKey != null);
			Contract.Ensures(Contract.Result<IDataBagFormatter<AccessToken>>() != null);

			return new UriStyleMessageFormatter<AccessToken>(signingKey, encryptingKey);
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
