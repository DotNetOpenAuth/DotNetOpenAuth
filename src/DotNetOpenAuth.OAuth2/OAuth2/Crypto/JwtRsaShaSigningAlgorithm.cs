//-----------------------------------------------------------------------
// <copyright file="JwtRsaShaSigningAlgorithm.cs" company="Andrew Arnott">
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

	internal class JwtRsaShaSigningAlgorithm : JwtSigningAlgorithm {
		private readonly RSACryptoServiceProvider algorithm;

		private readonly HashAlgorithm hashAlgorithm;

		internal enum HashSize {
			Sha256,
			Sha384,
			Sha512,
		}

		internal JwtRsaShaSigningAlgorithm(string algorithmName, RSACryptoServiceProvider algorithm, HashAlgorithm hashAlgorithm)
			: base(algorithmName) {
			Requires.NotNull(algorithm, "algorithm");
			Requires.NotNull(hashAlgorithm, "hashAlgorithm");
			this.algorithm = algorithm;
			this.hashAlgorithm = hashAlgorithm;
		}

		internal static JwtRsaShaSigningAlgorithm Create(RSACryptoServiceProvider algorithm, HashSize hashSize) {
			Requires.NotNull(algorithm, "algorithm");

			string webAlgorithmName, cryptoName;
			switch (hashSize) {
				case HashSize.Sha256:
					webAlgorithmName = JsonWebSignatureAlgorithms.RsaSha256;
					cryptoName = "SHA256";
					break;
				case HashSize.Sha384:
					webAlgorithmName = JsonWebSignatureAlgorithms.RsaSha384;
					cryptoName = "SHA384";
					break;
				case HashSize.Sha512:
					webAlgorithmName = JsonWebSignatureAlgorithms.RsaSha512;
					cryptoName = "SHA512";
					break;
				default:
					Requires.InRange(false, "algorithm");
					throw Assumes.NotReachable();
			}

			HashAlgorithm hashAlgorithm = HashAlgorithm.Create(cryptoName);
			try {
				return new JwtRsaShaSigningAlgorithm(webAlgorithmName, algorithm, hashAlgorithm);
			} catch {
				hashAlgorithm.Dispose();
				throw;
			}
		}

		internal override byte[] Sign(byte[] securedInput) {
			return algorithm.SignData(securedInput, this.hashAlgorithm);
		}

		internal override bool Verify(byte[] securedInput, byte[] signature) {
			return algorithm.VerifyData(securedInput, this.hashAlgorithm, signature);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				// We only own the hash algorithm -- not the RSA algorithm.
				this.hashAlgorithm.Dispose();
			}

			base.Dispose();
		}
	}
}
