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

	internal class StandardAccessTokenAnalyzer : IAccessTokenAnalyzer {
		internal StandardAccessTokenAnalyzer() {
		}

		internal RSAParameters AuthorizationServerPublicSigningKey { get; set; }

		internal RSAParameters ResourceServerPrivateEncryptionKey { get; set; }

		public bool TryValidateAccessToken(string accessToken, out string user, out string scope) {
			var token = AccessToken.Decode(this.AuthorizationServerPublicSigningKey, this.ResourceServerPrivateEncryptionKey, accessToken);
			user = token.User;
			scope = token.Scope;
			return true;
		}
	}
}
