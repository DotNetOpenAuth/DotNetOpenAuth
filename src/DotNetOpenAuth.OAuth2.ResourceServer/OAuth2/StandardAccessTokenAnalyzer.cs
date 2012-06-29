//-----------------------------------------------------------------------
// <copyright file="StandardAccessTokenAnalyzer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// An access token reader that understands DotNetOpenAuth authorization server issued tokens.
	/// </summary>
	public class StandardAccessTokenAnalyzer : IAccessTokenAnalyzer {
		/// <summary>
		/// Initializes a new instance of the <see cref="StandardAccessTokenAnalyzer"/> class.
		/// </summary>
		/// <param name="authorizationServerPublicSigningKey">The crypto service provider with the authorization server public signing key.</param>
		/// <param name="resourceServerPrivateEncryptionKey">The crypto service provider with the resource server private encryption key.</param>
		public StandardAccessTokenAnalyzer(RSACryptoServiceProvider authorizationServerPublicSigningKey, RSACryptoServiceProvider resourceServerPrivateEncryptionKey) {
			Requires.NotNull(authorizationServerPublicSigningKey, "authorizationServerPublicSigningKey");
			Requires.NotNull(resourceServerPrivateEncryptionKey, "resourceServerPrivateEncryptionKey");
			Requires.True(!resourceServerPrivateEncryptionKey.PublicOnly, "resourceServerPrivateEncryptionKey");
			this.AuthorizationServerPublicSigningKey = authorizationServerPublicSigningKey;
			this.ResourceServerPrivateEncryptionKey = resourceServerPrivateEncryptionKey;
		}

		/// <summary>
		/// Gets the authorization server public signing key.
		/// </summary>
		/// <value>The authorization server public signing key.</value>
		public RSACryptoServiceProvider AuthorizationServerPublicSigningKey { get; private set; }

		/// <summary>
		/// Gets the resource server private encryption key.
		/// </summary>
		/// <value>The resource server private encryption key.</value>
		public RSACryptoServiceProvider ResourceServerPrivateEncryptionKey { get; private set; }

		/// <summary>
		/// Reads an access token to find out what data it authorizes access to.
		/// </summary>
		/// <param name="message">The message carrying the access token.</param>
		/// <param name="accessToken">The access token's serialized representation.</param>
		/// <returns>The deserialized, validated token.</returns>
		/// <exception cref="ProtocolException">Thrown if the access token is expired, invalid, or from an untrusted authorization server.</exception>
		public virtual AccessToken DeserializeAccessToken(IDirectedProtocolMessage message, string accessToken) {
			var accessTokenFormatter = AccessToken.CreateFormatter(this.AuthorizationServerPublicSigningKey, this.ResourceServerPrivateEncryptionKey);
			var token = new AccessToken();
			accessTokenFormatter.Deserialize(token, message, accessToken, Protocol.access_token);
			return token;
		}
	}
}
