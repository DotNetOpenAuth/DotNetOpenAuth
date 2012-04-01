//-----------------------------------------------------------------------
// <copyright file="SpecialAccessTokenAnalyzer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	using DotNetOpenAuth.OAuth2;

	internal class SpecialAccessTokenAnalyzer : StandardAccessTokenAnalyzer {
		/// <summary>
		/// Initializes a new instance of the <see cref="SpecialAccessTokenAnalyzer"/> class.
		/// </summary>
		/// <param name="authorizationServerPublicSigningKey">The authorization server public signing key.</param>
		/// <param name="resourceServerPrivateEncryptionKey">The resource server private encryption key.</param>
		internal SpecialAccessTokenAnalyzer(RSACryptoServiceProvider authorizationServerPublicSigningKey, RSACryptoServiceProvider resourceServerPrivateEncryptionKey)
			: base(authorizationServerPublicSigningKey, resourceServerPrivateEncryptionKey) {
		}

		public override AccessToken DeserializeAccessToken(DotNetOpenAuth.Messaging.IDirectedProtocolMessage message, string accessToken) {
			var token = base.DeserializeAccessToken(message, accessToken);

			// Ensure that clients coming in this way always belong to the oauth_client role.
			token.Scope.Add("oauth_client");

			return token;
		}
	}
}
