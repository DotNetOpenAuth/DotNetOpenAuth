namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	internal class JweRsaEncryptionAlgorithm : JwtEncryptionAlgorithm {
		private readonly RSACryptoServiceProvider recipientPublicKey;

		private readonly bool useOaepPadding;

		internal JweRsaEncryptionAlgorithm(RSACryptoServiceProvider recipientPublicKey, bool useOaepPadding = true)
			: base(useOaepPadding ? JsonWebEncryptionAlgorithms.RSA_OAEP : JsonWebEncryptionAlgorithms.RSA1_5, JsonWebEncryptionMethods.A256CBC) {
			Requires.NotNull(recipientPublicKey, "recipientPublicKey");
			this.recipientPublicKey = recipientPublicKey;
			this.useOaepPadding = useOaepPadding;
		}

		internal override void Encrypt(byte[] plainText, out byte[] cipherText, out byte[] integrityValue) {
			cipherText = this.recipientPublicKey.Encrypt(plainText, this.useOaepPadding);
			integrityValue = null; // RSA is an AEAD algorithm, so it doesn't need a separate integrity check.
		}

		internal override byte[] Decrypt(byte[] cipherText, byte[] integrityValue) {
			throw new NotImplementedException();
		}
	}
}
