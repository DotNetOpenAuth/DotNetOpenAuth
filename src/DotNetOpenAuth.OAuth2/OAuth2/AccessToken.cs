//-----------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Validation;

	/// <summary>
	/// A short-lived token that accompanies HTTP requests to protected data to authorize the request.
	/// </summary>
	public class AccessToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		public AccessToken() {
		}

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Encoder = typeof(TimespanSecondsEncoder))]
		public TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Gets the type of this instance.
		/// </summary>
		/// <value>The type of the bag.</value>
		/// <remarks>
		/// This ensures that one token cannot be misused as another kind of token.
		/// </remarks>
		protected override Type BagType {
			get {
				// different roles (authorization server vs. Client) may derive from AccessToken, but they are all interoperable.
				return typeof(AccessToken);
			}
		}

		/// <summary>
		/// Creates a formatter capable of serializing/deserializing an access token.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the authorization server's private key used to asymmetrically sign the access token.</param>
		/// <param name="encryptingKey">The crypto service provider with the resource server's public key used to encrypt the access token.</param>
		/// <returns>An access token serializer.</returns>
		internal static IDataBagFormatter<AccessToken> CreateFormatter(RSACryptoServiceProvider signingKey, RSACryptoServiceProvider encryptingKey) {
			return new UriStyleMessageFormatter<AccessToken>(signingKey, encryptingKey);
		}

		/// <summary>
		/// Creates a formatter capable of serializing/deserializing an access token.
		/// </summary>
		/// <param name="symmetricKeyStore">The symmetric key store.</param>
		/// <returns>
		/// An access token serializer.
		/// </returns>
		internal static IDataBagFormatter<AccessToken> CreateFormatter(ICryptoKeyStore symmetricKeyStore) {
			Requires.NotNull(symmetricKeyStore, "symmetricKeyStore");
			return new UriStyleMessageFormatter<AccessToken>(symmetricKeyStore, bucket: "AccessTokens", signed: true, encrypted: true);
		}

		/// <summary>
		/// Initializes this instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="authorization">The authorization to apply to this access token.</param>
		internal void ApplyAuthorization(IAuthorizationDescription authorization) {
			Requires.NotNull(authorization, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope.ResetContents(authorization.Scope);
		}

		/// <summary>
		/// Initializes this instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="scopes">The scopes.</param>
		/// <param name="username">The username of the account that authorized this token.</param>
		/// <param name="lifetime">The lifetime for this access token.</param>
		/// <remarks>
		/// The <see cref="AuthorizationDataBag.ClientIdentifier"/> is left <c>null</c> in this case because this constructor
		/// is invoked in the case where the client is <em>not</em> authenticated, and therefore no
		/// trust in the client_id is appropriate.
		/// </remarks>
		internal void ApplyAuthorization(IEnumerable<string> scopes, string username, TimeSpan? lifetime) {
			this.Scope.ResetContents(scopes);
			this.User = username;
			this.Lifetime = lifetime;
			this.UtcCreationDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Serializes this instance to a simple string for transmission to the client.
		/// </summary>
		/// <returns>A non-empty string.</returns>
		protected internal virtual string Serialize() {
			throw new NotSupportedException();
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
