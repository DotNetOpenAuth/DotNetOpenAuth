//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerAccessToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// An access token minted by the authorization server that can be serialized for transmission to the client.
	/// </summary>
	public class AuthorizationServerAccessToken : AccessToken {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerAccessToken"/> class.
		/// </summary>
		public AuthorizationServerAccessToken() {
		}

		/// <summary>
		/// Gets or sets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <returns>A crypto service provider instance that contains the private key.</returns>
		/// <value>Must not be null, and must contain the private key.</value>
		/// <remarks>
		/// The public key in the private/public key pair will be used by the resource
		/// servers to validate that the access token is minted by a trusted authorization server.
		/// </remarks>
		public RSACryptoServiceProvider AccessTokenSigningKey { get; set; }

		/// <summary>
		/// Gets or sets the key to encrypt the access token.
		/// </summary>
		public RSACryptoServiceProvider ResourceServerEncryptionKey { get; set; }

		/// <summary>
		/// Gets or sets the symmetric key store to use if the asymmetric key properties are not set.
		/// </summary>
		public ICryptoKeyStore SymmetricKeyStore { get; set; }

		/// <summary>
		/// Serializes this instance to a simple string for transmission to the client.
		/// </summary>
		/// <returns>A non-empty string.</returns>
		protected internal override string Serialize() {
			ErrorUtilities.VerifyHost(this.AccessTokenSigningKey != null || this.SymmetricKeyStore != null, AuthServerStrings.AccessTokenSigningKeyMissing);
			IDataBagFormatter<AccessToken> formatter;
			if (this.AccessTokenSigningKey != null) {
				formatter = CreateFormatter(this.AccessTokenSigningKey, this.ResourceServerEncryptionKey);
			} else {
				formatter = CreateFormatter(this.SymmetricKeyStore);
			}

			return formatter.Serialize(this);
		}
	}
}
