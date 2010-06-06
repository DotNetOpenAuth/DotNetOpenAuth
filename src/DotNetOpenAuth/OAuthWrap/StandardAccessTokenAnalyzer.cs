//-----------------------------------------------------------------------
// <copyright file="StandardAccessTokenAnalyzer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	public class StandardAccessTokenAnalyzer : IAccessTokenAnalyzer {
		public StandardAccessTokenAnalyzer(RSAParameters authorizationServerPublicSigningKey, RSAParameters resourceServerPrivateEncryptionKey) {
			this.AuthorizationServerPublicSigningKey = authorizationServerPublicSigningKey;
			this.ResourceServerPrivateEncryptionKey = resourceServerPrivateEncryptionKey;
		}

		public RSAParameters AuthorizationServerPublicSigningKey { get; private set; }

		public RSAParameters ResourceServerPrivateEncryptionKey { get; private set; }

		public bool TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out string scope) {
			var token = AccessToken.Decode(this.AuthorizationServerPublicSigningKey, this.ResourceServerPrivateEncryptionKey, accessToken, message);
			user = token.User;
			scope = token.Scope;
			return true;
		}
	}
}
