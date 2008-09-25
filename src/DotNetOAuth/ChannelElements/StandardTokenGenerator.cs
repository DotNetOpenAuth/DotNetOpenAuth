//-----------------------------------------------------------------------
// <copyright file="StandardTokenGenerator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Security.Cryptography;

	internal class StandardTokenGenerator : ITokenGenerator {
		RandomNumberGenerator cryptoProvider = new RNGCryptoServiceProvider();

		#region ITokenGenerator Members

		public string GenerateRequestToken(string consumerKey) {
			return GenerateCryptographicallyStrongString();
		}

		public string GenerateAccessToken(string consumerKey) {
			return GenerateCryptographicallyStrongString();
		}

		public string GenerateSecret() {
			return GenerateCryptographicallyStrongString();
		}

		#endregion

		private string GenerateCryptographicallyStrongString() {
			byte[] buffer = new byte[20];
			cryptoProvider.GetBytes(buffer);
			return Convert.ToBase64String(buffer);
		}
	}
}
