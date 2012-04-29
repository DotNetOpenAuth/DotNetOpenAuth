//-----------------------------------------------------------------------
// <copyright file="JwtHmacShaSigningAlgorithm.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class JwtHmacShaSigningAlgorithm : JwtSigningAlgorithm {
		private readonly HashAlgorithm algorithm;

		internal enum Algorithm {
			HmacSha256,
			HmacSha384,
			HmacSha512,
		}

		private JwtHmacShaSigningAlgorithm(string algorithmName, HMAC algorithm)
			: base(algorithmName) {
			Requires.NotNull(algorithm, "algorithm");
			this.algorithm = algorithm;
		}

		internal static JwtSigningAlgorithm Create(Algorithm algorithm, string keyHandle, byte[] key) {
			Requires.NotNull(key, "key");

			string webAlgorithmName, cryptoName;
			switch (algorithm) {
				case Algorithm.HmacSha256:
					cryptoName = "HMAC-SHA256";
					webAlgorithmName = JsonWebSignatureAlgorithms.HmacSha256;
					break;
				case Algorithm.HmacSha384:
					cryptoName = "HMAC-SHA384";
					webAlgorithmName = JsonWebSignatureAlgorithms.HmacSha384;
					break;
				case Algorithm.HmacSha512:
					cryptoName = "HMAC-SHA512";
					webAlgorithmName = JsonWebSignatureAlgorithms.HmacSha512;
					break;
				default:
					Requires.InRange(false, "algorithm");
					throw Assumes.NotReachable();
			}

			HMAC hmac = null;
			try {
				hmac = HMAC.Create(cryptoName);
				hmac.Key = key;
				var result = new JwtHmacShaSigningAlgorithm(webAlgorithmName, hmac);
				result.Header.KeyIdentity = keyHandle;
				return result;
			} catch {
				if (hmac != null) {
					hmac.Dispose();
				}

				throw;
			}
		}

		internal override byte[] Sign(byte[] securedInput) {
			return algorithm.ComputeHash(securedInput);
		}

		internal override bool Verify(byte[] securedInput, byte[] signature) {
			return MessagingUtilities.AreEquivalentConstantTime(this.Sign(securedInput), signature);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				this.algorithm.Dispose();
			}

			base.Dispose();
		}
	}
}
