//-----------------------------------------------------------------------
// <copyright file="StandardAccessTokenAnalyzer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// An access token reader that understands DotNetOpenAuth authorization server issued tokens.
	/// </summary>
	public class StandardAccessTokenAnalyzer : IAccessTokenAnalyzer {
		/// <summary>
		/// Initializes a new instance of the <see cref="StandardAccessTokenAnalyzer"/> class.
		/// </summary>
		/// <param name="authorizationServerPublicSigningKey">The authorization server public signing key.</param>
		/// <param name="resourceServerPrivateEncryptionKey">The resource server private encryption key.</param>
		public StandardAccessTokenAnalyzer(RSAParameters authorizationServerPublicSigningKey, RSAParameters resourceServerPrivateEncryptionKey) {
			this.AuthorizationServerPublicSigningKey = authorizationServerPublicSigningKey;
			this.ResourceServerPrivateEncryptionKey = resourceServerPrivateEncryptionKey;
		}

		/// <summary>
		/// Gets the authorization server public signing key.
		/// </summary>
		/// <value>The authorization server public signing key.</value>
		public RSAParameters AuthorizationServerPublicSigningKey { get; private set; }

		/// <summary>
		/// Gets the resource server private encryption key.
		/// </summary>
		/// <value>The resource server private encryption key.</value>
		public RSAParameters ResourceServerPrivateEncryptionKey { get; private set; }

		/// <summary>
		/// Reads an access token to find out what data it authorizes access to.
		/// </summary>
		/// <param name="message">The message carrying the access token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="user">The user whose data is accessible with this access token.</param>
		/// <param name="scope">The scope of access authorized by this access token.</param>
		/// <returns>
		/// A value indicating whether this access token is valid.
		/// </returns>
		public bool TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out string scope) {
			var token = AccessToken.Decode(this.AuthorizationServerPublicSigningKey, this.ResourceServerPrivateEncryptionKey, accessToken, message);
			user = token.User;
			scope = token.Scope;
			return true;
		}
	}
}
